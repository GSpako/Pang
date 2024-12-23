
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
    HRT_Time oneSecond, halfSecond, tenMillis;
    MessageManager playerMailBox, timerMailBox;



    [SerializeField]
    RTDESKEngine Engine;   //Shortcut

    private Queue<GameObject> pool;

    [SerializeField]
    private int poolSize = 10;

    [SerializeField]
    private GameObject hookPrefab;

    private void Awake()
    {
        //Assign the "listener" to the normalized component RTDESKEntity. Every gameObject that wants to receive a message must have a public mailbox
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start()
    {

        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;
        RTDESKInputManager IM = Engine.GetInputManager();

        playerMailBox = RTDESKEntity.getMailBox("Player");
        timerMailBox = RTDESKEntity.getMailBox("TimerManager");
        //Register keys that we want to be signaled in case the user press them

        pool = new Queue<GameObject>();
        for (int i = 0;i<poolSize;i++){
            GameObject aux = Instantiate(hookPrefab, transform.position, Quaternion.identity);
            aux.transform.parent = this.gameObject.transform;
            aux.SetActive(false);
            pool.Enqueue(aux);
        } 
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                break;
            case (int)UserMsgTypes.Position:
                break;
            case (int)UserMsgTypes.Rotation:
                break;
            case (int)UserMsgTypes.Scale:
                break;
            case (int)UserMsgTypes.TRE:
                break;
            case (int)UserMsgTypes.Action:
                Action a;
                a = (Action)Msg;
               
                switch ((int)a.action)
                {
                    case (int)UserActions.Start:
                        break;
                    case (int)UserActions.End:
                        break;
                }
                Engine.PushMsg(Msg);
                
                break;
        }
    }
}
