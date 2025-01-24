using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPool : MonoBehaviour
{

    public GameObject hookPrefab; // Assign your hook prefab here
    public int poolSize = 20;       // Initial number of hooks in the pool

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    void Awake()
    {
        // Pre-instantiate hooks up to poolSize
        for (int i = 0; i < poolSize; i++)
        {
            GameObject hook = Instantiate(hookPrefab, transform);
            hook.transform.SetParent(transform.parent);
            hook.GetComponent<Hook>().pool = this;
            poolQueue.Enqueue(hook);
            hook.SetActive(false);
        }
    }

    public GameObject GetHook()
    {
        if (poolQueue.Count > 0)
        {
            GameObject hook = poolQueue.Dequeue();
            hook.SetActive(true);
            return hook;
        }

        return null;
    }

    public void ReturnHook(GameObject hook)
    {
        hook.SetActive(false);
        poolQueue.Enqueue(hook);
    }

    public int Unused()
    { return poolQueue.Count; }

    public bool AllInactive()
    { return poolQueue.Count == poolSize; }

    public bool AnyAvailible()
    { return poolQueue.Count > 0; }

    public bool AllActive()
    { return poolQueue.Count == 0; }
}