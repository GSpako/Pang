using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class PhotonBall : NetworkBehaviour
{
    // The current size level (e.g., 3 = largest; 0 = smallest�won't split further)
    [SerializeField]
    public int sizeLevel = 3;

    // Reference to the ball prefab used for spawning new balls.
    // This must be a NetworkPrefabRef registered in Fusion's Network Prefabs.
    [SerializeField]
    public NetworkPrefabRef ballPrefab;

    // How far apart to spawn the two new balls when splitting.
    [SerializeField]
    public float splitOffset = 0.5f;

    // Layer mask used in the overlap sphere to filter collision objects.
    // Make sure this includes the layers your players and hook objects are on.
    [SerializeField]
    public LayerMask collisionLayers;

    // List to store the results of the overlap query.
    private List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

    // Cached reference to the NetworkRigidbody2D component (from the Fusion Physics Addon).
    private NetworkRigidbody2D networkRigidbody2D;
    
    [HideInInspector]
    public LocalSceneManager localSceneManager;

    public override void Spawned()
    {
        //this.transform.parent = 
        // Cache the NetworkRigidbody2D when the object spawns.
        networkRigidbody2D = GetComponent<NetworkRigidbody2D>();

        networkRigidbody2D.Rigidbody.velocity = new Vector2(0.2f, .5f);
        localSceneManager = gameObject.GetComponentInParent<LocalSceneManager>();

        if (networkRigidbody2D == null)
        {
            Debug.LogWarning("NetworkRigidbody2D is not assigned on this prefab.", this);
        }
    }
    
    /// **NEW: Proper Collision Detection in Fusion**
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Object.HasStateAuthority) return; // Only the state authority should handle collisions.


        if (collision.gameObject.GetComponent<PhotonHook>() != null)
        {
            Debug.Log($"Collision detected with Hook");

            NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                Runner.Despawn(networkObject); // Despawn the collided object.
            }

            if(sizeLevel == 0)
            {
                localSceneManager.BallDestroyed();
                Runner.Despawn(Object);
            }
            else 
            {
                SplitBall();
            }
        }
        else if (collision.gameObject.GetComponentInParent<PhotonPlayer>() != null)
        {
            Debug.Log($"Collision detected with Player");

            //Aqui avisar al globalManager de que ha perdido el jugador

            //como indicar 
            localSceneManager.Death();
        }
    }

    // Splits the current ball into two smaller balls.
    private void SplitBall()
    {
        // Record the current position and velocity.
        Vector3 currentPos = transform.position;
        Vector3 currentVelocity = networkRigidbody2D != null ? networkRigidbody2D.Rigidbody.velocity : Vector3.zero;

        // Compute the new size level.
        int newSize = sizeLevel - 1;

        

        splitOffset = transform.localScale.x * 3 / 4;

        // Calculate spawn positions for the two new balls (offset left and right).
        Vector3 pos1 = currentPos + Vector3.left * splitOffset;
        Vector3 pos2 = currentPos + Vector3.right * splitOffset;

        // Spawn the two new balls using Fusion�s Runner.Spawn.
        NetworkObject ballObj1 = Runner.Spawn(ballPrefab, pos1, Quaternion.identity);
        NetworkObject ballObj2 = Runner.Spawn(ballPrefab, pos2, Quaternion.identity);

        ballObj1.transform.SetParent(transform.parent, true);
        ballObj2.transform.SetParent(transform.parent, true);

        // Get the PhotonBall components on the spawned objects.
        PhotonBall controller1 = ballObj1.GetComponent<PhotonBall>();
        PhotonBall controller2 = ballObj2.GetComponent<PhotonBall>();

        controller1.localSceneManager = this.localSceneManager;
        controller2.localSceneManager = this.localSceneManager;
        
        this.localSceneManager.AddBall(ballObj1,ballObj2);

        //ahora habría que meter estas bolas en la lista de bolas del local y sacar la bola anterior

        // Set the new size level.
        controller1.sizeLevel = newSize;
        controller2.sizeLevel = newSize;

        // Optionally adjust the scale based on the new size level.
        float newScaleFactor = this.transform.localScale.x/2;
        ballObj1.transform.localScale = Vector3.one * newScaleFactor;
        ballObj2.transform.localScale = Vector3.one * newScaleFactor;

        // Get the NetworkRigidbody2D components from the spawned balls.
        NetworkRigidbody2D nr1 = ballObj1.GetComponent<NetworkRigidbody2D>();
        NetworkRigidbody2D nr2 = ballObj2.GetComponent<NetworkRigidbody2D>();

        // Compute new velocities for the split balls.
        Vector3 vel1 = new Vector3(-Mathf.Abs(0.2f), Mathf.Abs(0.4f), currentVelocity.z);
        Vector3 vel2 = new Vector3(Mathf.Abs(0.2f), Mathf.Abs(0.4f), currentVelocity.z);

        // Apply the new velocities.
        if (nr1 != null)
        {
            nr1.Rigidbody.velocity = vel1;
        }
        if (nr2 != null)
        {
            nr2.Rigidbody.velocity = vel2;
        }

        // Despawn (remove) the original ball from the network.
        Runner.Despawn(Object);
    }
}
