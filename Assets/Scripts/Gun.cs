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

using System;
using UnityEngine;
using static MsgContent;

enum GunActions { Start, Grow }

// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class Gun : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis;

    public float growRate = 0.01f;

    float MIN_HEIGHT = 0f;
    float height = 0f;

    bool growing = false;

    [SerializeField]
    RTDESKEngine Engine;   //Shortcut
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        //Assign the "listener" to the normalized component RTDESKEntity. Every gameObject that wants to receive a message must have a public mailbox
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Action ActMsg;
        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;
        RTDESKInputManager IM = Engine.GetInputManager();

        //Register keys that we want to be signaled in case the user press them
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.A);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.D);

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);

        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)GunActions.Start;

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        Transform p;

        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                break;

            case (int)UserMsgTypes.Position:

                Engine.SendMsg(Msg, tenMillis);
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
                        case (int)GunActions.Grow:

                            if(growing)
                            {
                                if (height > 43.43f)
                                {
                                    ResetGun();
                                    Engine.PushMsg(Msg);
                                }
                                else
                                {
                                    height += growRate;
                                    spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);// height;
                                    Engine.SendMsg(Msg, tenMillis);
                                }
                            }
                            else 
                                Engine.PushMsg(Msg); 

                            break;

                        case (int)GunActions.Start:

                            MIN_HEIGHT = spriteRenderer.size.y;
                            height = MIN_HEIGHT;

                            Engine.PushMsg(Msg);
                            break;

                        default:
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)UserActions.Start: //Stop the movement of the object
                            Action growMsg;

                            growing = true;

                            growMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            growMsg.action = (int)GunActions.Grow;
                            Engine.SendMsg(growMsg, gameObject, ReceiveMessage, tenMillis);
                            break;

                        case (int)UserActions.End:

                            ResetGun();
                            break;

                    }
                    Engine.PushMsg(Msg);
                }
                break;
        }

        void ResetGun()
        {
            growing = false;

            height = MIN_HEIGHT;
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);
        }
    }

}
