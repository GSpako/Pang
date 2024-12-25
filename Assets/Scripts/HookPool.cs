#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class HookPool : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, reloadTimeHRT;
    MessageManager playerMailBox, timerMailBox;

    [SerializeField]
    RTDESKEngine Engine; // Shortcut

    // This list tracks only the currently *available* (inactive) hooks
    private List<GameObject> pool;

    // NEW: This list will track *all* hooks that have been created
    private List<GameObject> allHooks;

    [SerializeField]
    private int poolSize = 10;
    public float creationDistance = 1.0f;
    public float reloadTime = 400;

    [SerializeField]
    private GameObject hookPrefab;

    MessageManager hookMailBox;

    private void Awake()
    {
        // Assign the "listener" to the normalized component RTDESKEntity. 
        // Every GameObject that wants to receive a message must have a public mailbox
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    void Start()
    {
        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;

        playerMailBox = RTDESKEntity.getMailBox("Player");
        timerMailBox = RTDESKEntity.getMailBox("TimerManager");

        // Initialize both lists
        pool = new List<GameObject>();
        allHooks = new List<GameObject>(); // <--- NEW
        reloadTimeHRT = Engine.ms2Ticks(reloadTime);

        // Create the initial pool of hooks
        for (int i = 0; i < poolSize; i++)
        {
            GameObject aux = Instantiate(hookPrefab, new Vector3(0, -0.8f, 0), Quaternion.identity);
            aux.transform.parent = this.gameObject.transform;
            aux.name = "Hook_" + i;
            aux.SetActive(false);

            // Add to the 'pool' list
            pool.Add(aux);

            // Also keep track in the 'allHooks' list
            allHooks.Add(aux);
        }
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                break;

            case (int)UserMsgTypes.Position:
                Transform t;
                GameObject aux;
                Action ActMsg;

                // Set hook in place
                if (pool.Count > 0)
                {
                    t = (Transform)Msg;

                    Action shootRenable;
                    shootRenable = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                    shootRenable.action = (int)UserActions.LiveState;

                    if (CanCreateHook(t.V3, creationDistance))
                    {
                        // Grab first available hook
                        aux = pool[0];
                        pool.RemoveAt(0); // Remove it from 'pool' (available hooks)
                        aux.SetActive(true);
                        aux.transform.position = t.V3;

                        // Tell hook to go off
                        hookMailBox = RTDESKEntity.getMailBox(aux.name);

                        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                        ActMsg.action = (int)HookActions.Start;
                        Engine.SendMsg(ActMsg, aux, hookMailBox, 0);

                        Debug.Log("Not Instant");

                        Engine.SendMsg(shootRenable, gameObject, playerMailBox, reloadTimeHRT);
                    }
                    else 
                    {
                        Debug.Log("Instant");
                        Engine.SendMsg(shootRenable, gameObject, playerMailBox, 1);
                    }
                }

                Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Object:
                // Message for putting a hook back in the pool
                ObjectMsg obj = (ObjectMsg)Msg;

                obj.o.SetActive(false);
                // When a hook is returned to the pool, add it back to the 'pool' list
                pool.Add(obj.o);

                Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Scale:
                break;
            case (int)UserMsgTypes.TRE:
                break;
            case (int)UserMsgTypes.Action:
                break;
        }
    }

    bool CanCreateHook(Vector3 position, float distance)
    {

        for (int i = 0; i < allHooks.Count; i++)
        {
            if (!allHooks[i].activeInHierarchy)
                continue; 

            float currentDistance = Vector3.Distance(allHooks[i].transform.position, position);

            if (currentDistance <= distance)
            {
                return false;
            }
        }

        // If we made it through the loop, no hooks are close enough
        return true;
    }

}
