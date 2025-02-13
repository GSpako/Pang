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

    public enum LocalSceneState
    {
        Dead,
        InProgress,
        Victory,
    }

    public void SpawnBall(){
        if(state == LocalSceneState.Dead){return;}

        if(ballPrefab != null){
            Runner.Spawn(ballPrefab, this.transform.position + new Vector3(0,0.6f,0), Quaternion.identity);
            spheresLeft += 8;
            state = LocalSceneState.InProgress;
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
        Runner.Despawn(player);
    }
}
