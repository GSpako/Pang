
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

enum SceneActions { Start, Stop, Pause, Unpause}
public class SceneManager : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis;

    [SerializeField]
    RTDESKEngine Engine;   //Shortcut
    private bool isPaused = true;

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

        //Register keys that we want to be signaled in case the user press them
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.P);

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);

        Action ActMsg;
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ActMsg.action = (int)SceneActions.Start;
        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                RTDESKInputMsg IMsg = (RTDESKInputMsg)Msg;
                switch (IMsg.c)
                {
                    case KeyCode.P:
                        
                        break;
                }
                Engine.PushMsg(Msg);
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
                //Sending automessage
                if (name == Msg.Sender.name)
                    switch ((int)a.action)
                    {
                        case (int)SceneActions.Start:
                            break;
                        case (int)SceneActions.Stop:
                            break;
                        case (int)SceneActions.Pause:
                            break;
                        case (int)SceneActions.Unpause:
                            break;
                        default:
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)SceneActions.Start: //Stop the movement of the object
                            break;
                        case (int)SceneActions.Stop:
                            break;
                    }
                    Engine.PushMsg(Msg);
                }
                break;
        }
    }
}
