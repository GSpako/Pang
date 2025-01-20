using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gancho : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public float growRate = 1f;

    float height = 0f;

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
            Destroy(gameObject);
        }
        else
        { // grow the hook each call
            height += growRate * Time.deltaTime;
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);// height;
        }
    }


}
