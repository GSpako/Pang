using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.Networking;

public class PhotonHook : NetworkBehaviour {

    [Networked] private TickTimer vida { get; set; }
    SpriteRenderer spriteRenderer;

    public float growRate = 1f;
    [Networked] private float height { get; set; }


    public override void Spawned()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        height = 2.8f;
    }

    public override void FixedUpdateNetwork() {

        if (height > 35f)// limit growth of the hook, this way it doest go outside the map
        {
            Runner.Despawn(Object);
        }
        else
        {
            height += growRate * Runner.DeltaTime;
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);// height;
        }
    }

}

