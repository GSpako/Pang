
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
using TMPro;
enum TimerActions { Start, Update, End}

// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]

public class Timer : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, tenMillis;

    [SerializeField]
    RTDESKEngine Engine;   //Shortcut
    [SerializeField]
    TextMeshProUGUI text;

    public int totalTime = 0;

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


        Action ActMsg;
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ActMsg.action = (int)TimerActions.Start;

        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
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
                        case (int)TimerActions.Start:
                            text.text = totalTime.ToString();

                            Action act;
                            act = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            act.action = (int)TimerActions.Update;
                            Engine.SendMsg(act, gameObject, ReceiveMessage, oneSecond);
                            break;
                        
                        case (int)TimerActions.Update:// Update the text, if  time has run out, send mesage to stop game
                            int currentTime = int.Parse(text.text);
                            text.text = (currentTime - 1).ToString();

                            if((currentTime - 1) == 0)
                            {

                                Engine.PushMsg(Msg);
                            }
                            else 
                            {
                                Engine.SendMsg(Msg, oneSecond);
                            }
                            break;

                        case (int)TimerActions.End:

                            break;
                        default:
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)UserActions.GetSteady: //Stop the movement of the object
                            break;
                        case (int)UserActions.Move:
                            break;
                    }
                    Engine.PushMsg(Msg);
                }
                break;
        }
    }
}
