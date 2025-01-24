using System.Collections.Generic;
using UnityEngine;

public class SpherePool : MonoBehaviour
{
    public GameObject spherePrefab; // Assign your sphere (ball) prefab here
    public int poolSize = 20;       // Initial number of spheres in the pool

    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    private HookPool hookPool;

    void Awake()
    {
        hookPool = GetComponent<HookPool>();

        // Pre-instantiate spheres up to poolSize
        for (int i = 0; i < poolSize; i++)
        {
            GameObject sphere = Instantiate(spherePrefab, transform);
            sphere.transform.SetParent(transform.parent);
            sphere.GetComponent<Sphere>().hookPool = hookPool;
            sphere.SetActive(false);
            poolQueue.Enqueue(sphere);
        }
    }

    public GameObject GetSphere()
    {
        if (poolQueue.Count > 0)
        {
            GameObject sphere = poolQueue.Dequeue();
            sphere.SetActive(true);
            return sphere;
        }
        else
        {
            // Optionally expand the pool if needed
            GameObject sphere = Instantiate(spherePrefab, transform);
            sphere.SetActive(true);
            return sphere;
        }
    }

    public void ReturnSphere(GameObject sphere)
    {
        sphere.SetActive(false);
        poolQueue.Enqueue(sphere);
    }

    public bool AllInactive()
    {
        return poolQueue.Count == poolSize;
    }
}
