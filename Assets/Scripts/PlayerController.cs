using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 1f;
    public GameObject hook;
    public HookPool hookpool;

    // Start is called before the first frame update
    void Start()
    {
        lastShot = - shootingColdown;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        Shooting();
    }

    private void Movement()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -speed * Time.deltaTime * new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += speed * Time.deltaTime * new Vector3(1, 0 , 0);
        }

        float x = Mathf.Clamp(transform.position.x, -1.53f, 1.53f);
        this.transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    public float shootingColdown = .2f;
    float lastShot;


    private void Shooting()
    {

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        { 
            if(Time.time > lastShot + shootingColdown)
            {   
                GameObject h = hookpool.GetHook();
                lastShot = Time.time;
                if(h != null)
                    h.transform.position =  transform.position - new Vector3(0,0.08f,0);
            }
        }
    }
}
