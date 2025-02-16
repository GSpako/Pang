using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class LocalSceneManager : NetworkBehaviour
{
    public LocalSceneState state = LocalSceneState.InProgress;
    public int spheresLeft = 8;
    public NetworkObject player = null;

    [SerializeField]
    public NetworkPrefabRef ballPrefab;

    
    private List<NetworkObject> activeBalls = new List<NetworkObject>();

    public enum LocalSceneState
    {
        Dead,
        InProgress,
        Victory,
    }

    public void SpawnBall(int localScaled=3){
        if(state == LocalSceneState.Dead){return;}
        if(Runner.GameMode != GameMode.Host){return;}
        if(ballPrefab != null){

            //meter en una lista las bolas actuales, cuando se crea bola, esta va a la lista del local
            NetworkObject n = Runner.Spawn(ballPrefab, this.transform.position + new Vector3(0,0.6f,0), Quaternion.identity);
            n.GetComponent<PhotonBall>().localSceneManager = this;

            //Reescalar la bola en funcion de tamaño
            //HUUUUUH por que esto no va
            /*
            float newScaleFactor = 0.5f;
            for(int i = 3-localScaled;i>0;i--){
                newScaleFactor= newScaleFactor/2f;
            }
            n.GetComponent<PhotonBall>().sizeLevel = localScaled;
            //no se esta escalando
            n.transform.localScale = Vector3.one * newScaleFactor;
            */
            spheresLeft += 8;
            state = LocalSceneState.InProgress;

            activeBalls.Add(n);
        }
    }

    public void Reset(){
        activeBalls = new List<NetworkObject>();
        player.GetComponent<PhotonPlayer>().enabled = true;
        spheresLeft = 0;
    }


    public void DestroyAllBalls(){
        if(Runner.IsServer){
            if(activeBalls.Count != 0){
                foreach(NetworkObject ball in activeBalls){
                    activeBalls.Remove(ball);
                    Runner.Despawn(ball);
                }
            }
        }
    }

    public void BallDestroyed()
    {
        spheresLeft--;

        if(spheresLeft == 0)
        {
            state = LocalSceneState.Victory;
        }
    }

    public void Death()
    {
        state = LocalSceneState.Dead;
        GlobalSceneManager.Instance.SceneLost(this);
        //Creo que no hay que despawnear players tema conectarse y desconectarse
        // y temas de reiniciar partida
        //Runner.Despawn(player);
        player.GetComponent<PhotonPlayer>().enabled = false;
    }
}
