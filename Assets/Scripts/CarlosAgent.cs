using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarlosAgent : Agent
{

    [HideInInspector] public bool dead = false;

    BufferSensorComponent bufferSensor;
    public List<Rigidbody2D> spheresInScene = new List<Rigidbody2D>();
    public int missed_Hooks = 0;

    float lastShot;
    float shotCD = 0.75f;
    float speed = .05f;
    float startTime = 0f;

    SpherePool spherePool;
    HookPool hookPool;
    
    int shotHooks = 0;

    public int ballsDestroyed = 0;
    public int totalBallsDestroyed = 0;

    void Start()
    {
        lastShot = Time.time-shotCD;
        bufferSensor = GetComponent<BufferSensorComponent>();
        spherePool = transform.parent.GetComponentInChildren<SpherePool>();
        hookPool = transform.parent.GetComponentInChildren<HookPool>();
    }
    public override void OnEpisodeBegin()
    {
        dead = false;
        ballsDestroyed = 0;
        totalBallsDestroyed = 0;
        shotHooks = 0;  
        startTime = Time.time;
        lastShot = Time.time - shotCD;

        // Use 'spherePool' to return old spheres to the pool
        foreach (Sphere sphereRb in transform.parent.GetComponentsInChildren<Sphere>())
        {
            if (sphereRb && sphereRb.gameObject.activeSelf)
            {
                spherePool.ReturnSphere(sphereRb.gameObject);
            }
        }
        spheresInScene.Clear();

        // Use hookPool to return old hooks to its pool
        foreach (Hook hook in transform.parent.GetComponentsInChildren<Hook>())
        {
            if (hook && hook.gameObject.activeSelf)
            {
                hookPool.ReturnHook(hook.gameObject);
            }
        }

        float xPos = Random.Range(-1.3f, 1.3f);
        float xPosAgent = Random.Range(-1.53f, 1.53f);
        float speedDir = Random.value;

        // Spawn a new sphere from the pool
        GameObject newSphere = spherePool.GetSphere();
        newSphere.transform.localPosition = new Vector3(xPos, 0.45f, 0f);
        newSphere.transform.localScale = Vector3.one/2; 
        // Choose a random direction to start moving in
        newSphere.GetComponent<Sphere>().vel = speedDir > .5f ? new Vector2(0.4f, 0f) : new Vector2(-0.4f, 0f);

        // Randomly position the agent
        transform.localPosition = new Vector3(xPosAgent, transform.localPosition.y, 0);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)this.transform.localPosition);
        sensor.AddObservation(hookPool.Unused());
        sensor.AddObservation(Time.time - startTime);

        foreach (Rigidbody2D rB in spheresInScene)
        {
            Vector2 relativePosition = rB.transform.position - transform.position;
            Vector2 velocity = rB.velocity;

            bufferSensor.AppendObservation(new float[] {
            relativePosition.x, relativePosition.y,
            velocity.x, velocity.y,
            rB.transform.lossyScale.x
        });
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector2 controlSignal = Vector2.zero;
        controlSignal.x = transform.localPosition.x + (speed * Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f));
        controlSignal.y = actionBuffers.ContinuousActions[1];

        float x = Mathf.Clamp(controlSignal.x, -1.53f, 1.53f);
        this.transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);

        if (controlSignal.y > 0)
        {
            if(hookPool.AnyAvailible() && Time.time > lastShot + shotCD) 
            {
                lastShot = Time.time;
                GameObject hook = hookPool.GetHook();
                hook.transform.position = transform.position - new Vector3(0, 0.08f, 0);
                shotHooks++;
            }
        }

        AddReward(-0.05f * (missed_Hooks));
        missed_Hooks = 0;
        shotHooks = 0;

        float wallProximity = Mathf.Min(Mathf.Abs(x - (-1.53f)), Mathf.Abs(x - 1.53f)); // Distance to the closest wall
        if (wallProximity < 0.2f) // Adjust the threshold as needed
        {
            AddReward(-0.01f * (0.2f - wallProximity)); // Negative reward increases as the agent gets closer to the wall
        }

        if (ballsDestroyed > 0)
        {
            AddReward(0.1f * ballsDestroyed);
            ballsDestroyed = 0;
        }

        if (totalBallsDestroyed >= 1 && spherePool.NoneActivated())
        {
            SetReward(1f);
            EndEpisode();
        }

        if (dead)
        {
            EndEpisode();
        }
    }

        public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public void AddSphere(Rigidbody2D r)
    {
        spheresInScene.Add(r);
    }

    public void RemoveSphere(Rigidbody2D r)
    {
        spheresInScene.Remove(r);
        ballsDestroyed++;
        totalBallsDestroyed++;
    }

}
