using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{

    private NetworkRunner _runner; 

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private NetworkPrefabRef _globalGameManagerPrefab;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private Dictionary<PlayerRef, NetworkObject> _spawnedScenarios = new Dictionary<PlayerRef, NetworkObject>();

    public struct scenario {public GameObject main;
                            public List<GameObject> extras;
                            }

    public scenario scenarios;

    public GameObject referenceScenario;

    public struct NetworkInputData: INetworkInput{public Vector2 direction;}

    //[HideInInspector]
    public GlobalSceneManager globalManager;

    public string RoomName;


   /* void Start(){
        scenarios.main = referenceScenario.transform.GetChild(0).gameObject;
        RectTransform rt = scenarios.main.GetComponent (typeof (RectTransform)) as RectTransform;
        rt.sizeDelta = new Vector2 (Screen.width/2, Screen.height/2);
        scenarios.extras = new List<GameObject>();
    }

    void Update(){
        RectTransform rt = scenarios.main.GetComponent (typeof (RectTransform)) as RectTransform;
        rt.sizeDelta = new Vector2 (Screen.width/2, Screen.height/2);
    }*/

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        if (runner.IsServer) {
            // Create a unique position for the player (player.RawEncoded % runner.Config.Simulation.PlayerCount)
            Vector3 spawnPosition = new Vector3(((player.RawEncoded % runner.Config.Simulation.PlayerCount)-2)* 3.5f, 0, 0);
            //EL instanciar la escena ya spawnea un player
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition - new Vector3(0,0.5f,0), Quaternion.identity, player);
            NetworkObject networkScenario = runner.Spawn(referenceScenario, spawnPosition, Quaternion.identity, player);
            networkScenario.gameObject.GetComponent<LocalSceneManager>().player = networkPlayerObject;
            networkPlayerObject.transform.SetParent(networkScenario.transform,true);


            globalManager.AddScene(networkPlayerObject.gameObject.GetComponentInParent<LocalSceneManager>());
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
            _spawnedScenarios.Add(player, networkScenario);
        }
        //a lo mejor ni hace falta, solo el add scene

        if (runner.IsClient) {
            if(_spawnedCharacters.TryGetValue(player,out NetworkObject n)){
                if(_spawnedScenarios.TryGetValue(player,out NetworkObject s)){
                    s.gameObject.GetComponent<LocalSceneManager>().player = n;
                    //n.transform.SetParent(s.transform,true);
                    globalManager.AddScene(n.gameObject.GetComponentInParent<LocalSceneManager>());             
                }
            }
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject)) {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
        //borrar tmb el escenario cuando se desconecte el jugador
        if (_spawnedScenarios.TryGetValue(player, out NetworkObject networkScene)) {
            runner.Despawn(networkScene);
            _spawnedScenarios.Remove(player);
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input) { 
        NetworkInputData data = new NetworkInputData();
        if(Input.GetKey(KeyCode.W)|| Input.GetKey(KeyCode.Space)) data.direction+= new Vector2(0,1);
        if(Input.GetKey(KeyCode.A)) data.direction+= new Vector2(-1,0);
        if(Input.GetKey(KeyCode.D)) data.direction+= new Vector2(1,0);

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
    ArraySegment<byte> data) {}
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float
    progress) {}


    
    async void StartGame(GameMode mode) {
    // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        gameObject.AddComponent<RunnerSimulatePhysics2D>().ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateForward;
        // Create the NetworkSceneInfo from the current scene
        SceneRef scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) 
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        // Start or join (depends on gamemode) a session with a specific name
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = RoomName,
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        if(_runner.GameMode == GameMode.Host){
            NetworkObject gman = _runner.Spawn(_globalGameManagerPrefab);
            globalManager = gman.GetComponent<GlobalSceneManager>();
        }else{

        }
    }

    private void OnGUI()
    {
        if (_runner == null) {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
                StartGame(GameMode.Host);
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
                StartGame(GameMode.Client);
        }
    }

    private void initImage(){
        GameObject gb = new GameObject("");
        gb.transform.parent = referenceScenario.transform.GetChild(1);
        gb.AddComponent<RectTransform>();
        gb.AddComponent<CanvasRenderer>();
        gb.AddComponent<RawImage>();
    }

}
