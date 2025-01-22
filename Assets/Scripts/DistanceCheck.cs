using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DistanceCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localPosition.y < -1000f)
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        //gameObject.SetActive(true);
    }
}
