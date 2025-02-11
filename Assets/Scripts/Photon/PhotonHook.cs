using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.Networking;

public class PhotonHook : NetworkBehaviour {

    [Networked] private TickTimer vida { get; set; }
    public void InitVida() {
        vida = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }
    public override void FixedUpdateNetwork() {
        if (vida.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += 5.0f * Runner.DeltaTime * transform.forward;
    }

}

