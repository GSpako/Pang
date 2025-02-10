using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Hook : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    [HideInInspector] public HookPool pool;

    public float growRate = 1f;

    float height = 0f;
    Vector3 correctScale = Vector3.zero;

    private void OnEnable()
    {
        if(correctScale == Vector3.zero)
            correctScale = transform.localScale; 

        if(spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        height = 0.01f;
        transform.localScale = correctScale;
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (height > 43.43f)// limit growth of the hook, this way it doest go outside the map
        {
            pool.ReturnHook(gameObject);
        }
        else
        { // grow the hook each call
            height += growRate * Time.deltaTime;
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);// height;
        }
    }
}
