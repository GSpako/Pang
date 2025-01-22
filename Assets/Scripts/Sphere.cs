using UnityEngine;

public class Sphere : MonoBehaviour
{
    public Vector2 vel = new Vector2(0.4f, .2f);
    public GameObject ballPrefab; // Not strictly needed if all sphere creation goes through SpherePool

    private PakoAgent pakoAgent;
    private GameStateManager gameManager;
    private Rigidbody2D rb;

    // Initialize references on enable or start
    void OnEnable()
    {
        // Because we are pooling, OnEnable might be called multiple times
        if (pakoAgent == null)
            pakoAgent = transform.parent.GetComponentInChildren<PakoAgent>();

        if (gameManager == null)
            gameManager = transform.parent.GetComponentInChildren<GameStateManager>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Apply current velocity
        rb.velocity = vel;

        // Inform PakoAgent of a newly "spawned" sphere
        if (pakoAgent != null)
        {
            pakoAgent.AddSphere(rb);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Example collision checks
        if (other.TryGetComponent<PlayerController>(out PlayerController p))
        {
            gameManager?.Death();
            gameManager?.DestroyedSoundEffect();
        }
        else if (other.TryGetComponent<Gancho>(out Gancho g))
        {
            Destroy(g.gameObject);
            DestroyBall();
            gameManager?.DestroyedSoundEffect();
        }
        else if (other.TryGetComponent<PakoAgent>(out PakoAgent agent))
        {
            agent.dead = true;
        }
    }

    private void DestroyBall()
    {
        Vector3 scale = transform.localScale;

        // If sphere is too small, return to pool
        if (scale.x < 0.1f)
        {
            ReturnToPool();
            gameManager?.RemoveBall();
        }
        else // Otherwise, split into two smaller spheres
        {
            SplitBall(scale);
        }
    }

    private void SplitBall(Vector3 scale)
    {
        //Debug.Log("Splitting Ball");

        // Retrieve the SpherePool in the parent environment
        SpherePool pool = transform.parent.GetComponentInChildren<SpherePool>();

        // Calculate a horizontal offset for the split
        float offsetX = (scale.x / 4f) + ((scale.x / 2f) * 0.2f);

        transform.localScale = scale * 0.5f; // Halve the size
        transform.localPosition += new Vector3(offsetX, 0f, 0f); // Shift to the right
        rb.velocity = new Vector2(vel.x, vel.y);


        GameObject b2 = pool.GetSphere();
        b2.transform.localScale = scale * 0.5f;
        b2.transform.localPosition = transform.localPosition - new Vector3(offsetX, 0f, 0f);
        b2.GetComponent<Sphere>().vel = new Vector2(-vel.x, vel.y);
        b2.GetComponent<Rigidbody2D>().velocity = new Vector2(-vel.x, vel.y);

    }

    private void ReturnToPool()
    {
        if (pakoAgent != null)
        {
            pakoAgent.RemoveSphere(rb);
        }

        SpherePool pool = transform.parent.GetComponentInChildren<SpherePool>();
        pool.ReturnSphere(gameObject);
    }

}
