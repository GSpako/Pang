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

enum GunActions { Start, Shoot }

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
    public GameObject Hook;

    MessageManager hookPoolMail;

    GameObject HookPoolobj;

    [SerializeField]
    RTDESKEngine Engine;   //Shortcut

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

        HookPoolobj = GameObject.Find("/Managers/HookPool");
        hookPoolMail = RTDESKEntity.getMailBox("HookPool");
    }

    void ReceiveMessage(MsgContent Msg)
    {
        Transform p;

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
                //Sending automessage
                if (name == Msg.Sender.name)
                    switch ((int)a.action)
                    {
                        case (int)GunActions.Shoot:
                            // send a mesage to the hook pool, to create the hook
                            Transform ActMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                            ActMsg.V3 = this.gameObject.transform.position;
                            Engine.SendMsg(ActMsg, HookPoolobj, hookPoolMail, tenMillis);
                
                            Engine.PushMsg(Msg); 
                            break;

                        case (int)GunActions.Start:
                            Engine.PushMsg(Msg);
                            break;

                        default:
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)UserActions.Start: //Shoot
                            // Send a msg to the self msg shoot action to shoot the gun
                            Action ActMsg;
                            ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            ActMsg.action = (int)GunActions.Shoot;
                            Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);
                            break;

                        case (int)UserActions.End:

                            break;

                    }
                    Engine.PushMsg(Msg);
                }
                break;
        }
    }
}
