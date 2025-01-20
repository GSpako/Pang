using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    GameStateManager gameManager;
    Rigidbody2D rb;

    public Vector2 vel = new Vector2(0.4f, 0);
    public GameObject BallPrefab;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindAnyObjectByType<GameStateManager>();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = vel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out PlayerController p))
        {
            gameManager.Death();
        }
        else if (other.gameObject.TryGetComponent<Gancho>(out Gancho g))
        {
            Destroy(g.gameObject);
            DestroyBall();
        }
    }

    private void DestroyBall()
    {
        Vector3 scale = gameObject.transform.localScale;
        //If small enough destroy the ball
        if (scale.x < .1f)
        {
            Destroy(gameObject);
            gameManager.RemoveBall();
        }
        else //if big enough split the ball into two, and make them move into opossite directions
        {
            //Create a new ball, and move it to the side, add the apropiate speed
            GameObject b1 = Instantiate(BallPrefab, transform.position, Quaternion.identity);
            b1.transform.localScale = new Vector3(scale.x / 2, scale.y / 2, scale.z / 2);
            b1.transform.position = new Vector3(transform.position.x + ((scale.x / 4) + (scale.x / 2) * 0.2f), transform.position.y, transform.position.z);


            //Create a another ball, and move it to the other side, add the opsite speed
            GameObject b2 = Instantiate(BallPrefab, transform.position, Quaternion.identity);
            b2.transform.localScale = new Vector3(scale.x / 2, scale.y / 2, scale.z / 2);
            b2.transform.position = new Vector3(transform.position.x - ((scale.x / 4) + (scale.x / 2) * 0.2f), transform.position.y, transform.position.z);
            b2.GetComponent<Sphere>().vel = new Vector2(-vel.x, vel.y);

            //Destroy the current sphere
            Destroy(gameObject);
        }
    }
}
