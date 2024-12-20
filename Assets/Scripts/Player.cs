
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

enum PlayerActions { Move, Idle, Start, Sleep, WakeUp, End }

// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class Player : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, fiftyMillis, halfSecond, tenMillis;

    public float speed = 1.0f;
    public Vector3 movement = new Vector3(0, 0, 0);
    bool shooting = false;
    MessageManager gunMailBox;

    [SerializeField]
    RTDESKEngine Engine;   //Shortcut

    private void Awake()
    {
        //Assign the "listener" to the normalized component RTDESKEntity. Every gameObject that wants to receive a message must have a public mailbox
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        gunMailBox = RTDESKEntity.getMailBox("Gun");

        if (null == gunMailBox) Debug.LogError("gunMailBox is null");
    }

    // Start is called before the first frame update
    void Start()
    {
        Action ActMsg;
        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;
        RTDESKInputManager IM = Engine.GetInputManager();

        //Register keys that we want to be signaled in case the user press them
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.W);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.A);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.D);

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        fiftyMillis = Engine.ms2Ticks(50);
        oneSecond = Engine.ms2Ticks(1000);

        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)PlayerActions.Start;

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        Transform p;

        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                RTDESKInputMsg IMsg = (RTDESKInputMsg)Msg;
                switch (IMsg.c)
                {
                    case KeyCode.W:
                        Action gunMsg;
                        gunMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);

                        if (KeyState.DOWN == IMsg.s)
                        {
                            gunMsg.action = (int)UserActions.Start;
                            Engine.SendMsg(gunMsg, gameObject, gunMailBox, HRTimer.HRT_INMEDIATELY);
                            shooting = true;
                        }
                        else
                        {
                            gunMsg.action = (int)UserActions.End;
                            Engine.SendMsg(gunMsg, gameObject, gunMailBox, HRTimer.HRT_INMEDIATELY);
                            shooting = false;
                        }
                        break;

                    case KeyCode.D:

                        if (KeyState.DOWN == IMsg.s)
                            movement += new Vector3(speed, 0, 0);
                        else
                            movement -= new Vector3(speed, 0, 0);

                        break;
                    case KeyCode.A:

                        if (KeyState.DOWN == IMsg.s)
                            movement -= new Vector3(speed, 0, 0);
                        else
                            movement += new Vector3(speed, 0, 0);

                        break;
                }
                Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Position:

                if (movement != Vector3.zero && !shooting)
                {
                    transform.Translate(movement);
                    Vector3 pos = transform.position;

                    if (transform.position.x > 1.53f)
                        pos.x = 1.53f;
                    if (transform.position.x < -1.53f)
                        pos.x = -1.53f;

                    transform.position = pos;
                }

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
                        case (int)PlayerActions.Idle:
                            break;
                        case (int)PlayerActions.Sleep:
                            break;
                        case (int)PlayerActions.WakeUp:
                            break;
                        case (int)PlayerActions.End:
                            break;
                        case (int)PlayerActions.Start:

                            p = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                            //We have to start the Object behaviour
                            Engine.SendMsg(p, gameObject, ReceiveMessage, fiftyMillis);

                            Engine.PushMsg(Msg);
                            break;
                        case (int)PlayerActions.Move:

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
