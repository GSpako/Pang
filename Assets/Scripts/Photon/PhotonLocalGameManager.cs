using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.Networking;

public class PhotonLocalGameManager : NetworkBehaviour {
    
    public NetworkObject [] balls;
    public NetworkObject ballPrefab;
    void Spawned()
    {
        balls = new NetworkObject[30];
    }

    void StartPlay(){
        Runner.Spawn(ballPrefab, this.transform.position, Quaternion.identity);
    }

    void FixedUpdateNetwork()
    {
        
    }
}
