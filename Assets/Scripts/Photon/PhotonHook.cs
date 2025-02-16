using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PhotonHook : NetworkBehaviour {

    [Networked] private float height { get; set; }
    private SpriteRenderer spriteRenderer;

    public float growRate = 1f;
    private bool isSpawned = false;

    public override void Spawned() {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize height only on the authoritative client
        if (Object.HasStateAuthority) {
            height = 2.8f;
        }

        isSpawned = true;
    }

    public override void FixedUpdateNetwork() {
        if (!isSpawned) return;

        // Only authoritative client should grow the hook
        if (Object.HasStateAuthority) {
            if (height > 35f) {
                Runner.Despawn(Object);
            } else {
                height += growRate * Runner.DeltaTime;
            }
        }
    }

    public override void Render()
    {
        if (!isSpawned) return;

        // Update the sprite size using the interpolated networked height
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);
    }

}
