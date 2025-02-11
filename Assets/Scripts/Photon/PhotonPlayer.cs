using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.Networking;

public class PhotonPlayer : NetworkBehaviour {
    
    private NetworkCharacterController _cc;
    private void Awake(){
        GameObject _player = transform.GetChild(0).gameObject;
        _player.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
        _cc = GetComponent<NetworkCharacterController>();
    }
    public override void FixedUpdateNetwork() {
        if(GetInput(out Spawner.NetworkInputData data)) {
            //data.direction.Normalize();
            _cc.Move(5 * Runner.DeltaTime * data.direction);
        }
    }
}
