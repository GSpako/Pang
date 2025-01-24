using UnityEngine;

public class Sphere : MonoBehaviour
{
    public Vector2 vel = new Vector2(0.4f, .4f);
    public GameObject ballPrefab; // Not strictly needed if all sphere creation goes through SpherePool

    [HideInInspector] public HookPool hookPool;
    private PakoAgent pakoAgent;
    private GameStateManager gameManager;
    private Rigidbody2D rb;
    private SpherePool pool;

    // Initialize references on enable or start
    void OnEnable()
    {
        // Because we are pooling, OnEnable might be called multiple times
        if (pakoAgent == null)
            pakoAgent = transform.parent.GetComponentInChildren<PakoAgent>();

        if (gameManager == null)
            gameManager = transform.parent.GetComponentInChildren<GameStateManager>();

        if (pool == null)
            pool = transform.parent.GetComponentInChildren<SpherePool>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Apply current velocity
        if(rb.velocity.magnitude <= 0)
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
        else if (other.TryGetComponent<Hook>(out Hook g))
        {
            if (!g.gameObject.activeInHierarchy) return;

            hookPool.ReturnHook(other.gameObject);
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
        SpherePool pool = transform.parent.GetComponentInChildren<SpherePool>();
        Vector3 originalPos = transform.localPosition;
        float offsetX = scale.x * 0.5f;

        // Move (and resize) this original sphere to the RIGHT by 'offsetX'
        transform.localPosition = new Vector3(
            originalPos.x + offsetX,
            originalPos.y,
            originalPos.z
        );
        transform.localScale = scale / 2f;
        rb.velocity = new Vector2(Mathf.Abs(vel.x), vel.y);   


        // Place the second sphere to the LEFT of the original position
        GameObject b2 = pool.GetSphere();
        b2.transform.localPosition = new Vector3(
            originalPos.x - offsetX,
            originalPos.y,
            originalPos.z
        );
        b2.transform.localScale = scale / 2f;
        b2.GetComponent<Sphere>().rb.velocity = new Vector2(-Mathf.Abs(vel.x), vel.y);
    }


    private void ReturnToPool()
    {
        if(pakoAgent != null) 
            pakoAgent.RemoveSphere(rb);
        

        pool.ReturnSphere(gameObject);
    }

}
