using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class ballTest : MonoBehaviour
{
    Rigidbody2D rg;

    // Start is called before the first frame update
    void Start()
    {
        rg = GetComponent<Rigidbody2D>();
        rg.velocity = new Vector2(0.4f, 0);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
