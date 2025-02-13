using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.Networking;

public class PhotonPlayer : NetworkBehaviour {
    
    [SerializeField] private PhotonHook _prefabHook;
    private NetworkCharacterController _cc;
    private Vector3 _forward;

    private TickTimer delay;
    private void Awake(){
        GameObject _player = transform.GetChild(0).gameObject;
        _player.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
        _cc = GetComponent<NetworkCharacterController>();

    }
    public override void FixedUpdateNetwork() {

        if (GetInput(out Spawner.NetworkInputData data)) {
            _cc.Move(5 * new Vector2(data.direction.x,0) * Runner.DeltaTime);
            if (data.direction.sqrMagnitude > 0) _forward = data.direction;

            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner)) {
                if (data.direction.y>0) {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabHook, Vector3.zero
                    , Quaternion.identity,
                    Object.InputAuthority, (runner, o) => {
                        // Initialize the Ball before synchronizing it
                        o.transform.SetParent(gameObject.transform.parent, false);
                        o.transform.localPosition = transform.localPosition - new Vector3(0, 0.08f, 0);
                        });
                    
                }
            }
        }

        Vector2 pos = transform.localPosition;
        gameObject.transform.localPosition = new Vector2(Mathf.Clamp(pos.x, -1.53f, 1.53f), -0.8f);

    }
}
