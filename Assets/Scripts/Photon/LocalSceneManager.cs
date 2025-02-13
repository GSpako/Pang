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

    public enum LocalSceneState
    {
        Dead,
        InProgress,
        Victory,
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
        Runner.Despawn(player);
    }
}
