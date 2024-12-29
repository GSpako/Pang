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

public enum HookActions { Start, Grow, Destroy }



// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class Hook : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, hundredMillis;

    public float growRate = 0.01f;

    float MIN_HEIGHT = 0f;
    float height = 0f;
    GameObject f;
    bool growing = false;

    [SerializeField]
    RTDESKEngine Engine;   //Shortcut
    SpriteRenderer spriteRenderer;

    MessageManager hookPoolMail, audioPoolMail;

    GameObject HookPoolobj, AudioPoolObj;

    [SerializeField]
    AudioClip clip;

    private void Awake()
    {
        //Assign the "listener" to the normalized component RTDESKEntity. Every gameObject that wants to receive a message must have a public mailbox
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        spriteRenderer = GetComponent<SpriteRenderer>();

        /*
        All code that normally goes into start has to go on awake since at
        instantiating and sending a message start is too slow, therefore it doesnt work correctly
        */
        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;
        //dummy variable
        Action ActMsg;

        RTDESKInputManager IM = Engine.GetInputManager();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);
        hundredMillis = Engine.ms2Ticks(100);

        //routes and mailbox
        HookPoolobj = GameObject.Find("/Managers/HookPool");
        hookPoolMail = RTDESKEntity.getMailBox("HookPool");
        AudioPoolObj = GameObject.Find("/Managers/SoundManager");
        audioPoolMail = RTDESKEntity.getMailBox("SoundManager");
        
        MIN_HEIGHT = spriteRenderer.size.y;
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
                        case (int)HookActions.Grow:

                            if (Time.timeScale == 0)
                            {
                                Engine.SendMsg(Msg, hundredMillis);
                            }
                            else if (growing)
                            {
                                if (height > 43.43f)
                                {
                                    DestroyHook();
                                    Engine.PushMsg(Msg);
                                }
                                else
                                {
                                    height += growRate;
                                    spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);// height;
                                    Engine.SendMsg(Msg,gameObject,ReceiveMessage, tenMillis);
                                }
                            }
                            else
                            {
                                Engine.PushMsg(Msg);
                            }

                            break;

                        case (int)HookActions.Start:

                           
                            height = MIN_HEIGHT;

                            Action growMsg;
                            growing = true;
                            

                            growMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            growMsg.action = (int)HookActions.Grow;
                            Engine.SendMsg(growMsg, gameObject, ReceiveMessage, tenMillis);

                            Engine.PushMsg(Msg);
                            break;

                        case (int)HookActions.Destroy:
                            //Debug.Log("Destroying Hook");
                            DestroyHook();
                            
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

                            break;

                        case (int)UserActions.End:
                            DestroyHook();
                            break;

                    }
                    Engine.PushMsg(Msg);
                }
                break;
        }

        void DestroyHook()
        {
            //Destroy(gameObject);

            //Necesitaba un tipo disponible para mandar el mensaje, asi que Object
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, MIN_HEIGHT);// height;
            ObjectMsg a = (ObjectMsg)Engine.PopMsg((int)UserMsgTypes.Object);
            a.o = this.gameObject;
            growing = false;
            Engine.SendMsg(a, HookPoolobj, hookPoolMail, tenMillis);

            AudioMsg au = (AudioMsg)Engine.PopMsg((int)UserMsgTypes.Audio);
            au.audio = clip;
            Engine.SendMsg(au, AudioPoolObj, audioPoolMail, tenMillis);
        }


    }

}
