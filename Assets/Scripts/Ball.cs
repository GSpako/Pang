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

enum BallActions { Start, Destroy }

// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class Ball : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis;

    public Vector2 vel = new Vector2(0.4f, 0);

    [SerializeField]
    RTDESKEngine Engine;   //Shortcut
    Rigidbody2D rg;
    public GameObject BallPrefab;

    private void Awake()
    {
        //Assign the "listener" to the normalized component RTDESKEntity. Every gameObject that wants to receive a message must have a public mailbox
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;

        rg = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Action ActMsg;
        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);

        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ActMsg.action = (int)BallActions.Start;
        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);

        /*
        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ActMsg.action = (int)BallActions.Destroy;
        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, Engine.ms2Ticks(5000));
        */
        
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
                        case (int)BallActions.Start:
                            //Initialize the object speed
                            rg.velocity = vel;

                            break;
                        case (int)BallActions.Destroy:
                            //Destroy the ball
                            Vector3 scale = gameObject.transform.localScale;
                            //If small enough destroy the ball
                            if (scale.x < .1f)
                            {
                                Engine.PushMsg(Msg);
                                Destroy(gameObject);
                            }
                            else //if big enough split the ball into two, and make them move into opossite directions
                            {
                                GameObject b1 = Instantiate(BallPrefab, transform.position, Quaternion.identity);
                                b1.transform.localScale = new Vector3(scale.x / 2, scale.y / 2, scale.z / 2);
                                b1.transform.position = new Vector3(transform.position.x + ((scale.x / 4) + (scale.x / 2) * 0.2f), transform.position.y, transform.position.z);
                                
                                b1.GetComponent<Ball>().vel = new Vector2(-vel.x, vel.y);

                                GameObject b2 = Instantiate(BallPrefab, transform.position, Quaternion.identity);
                                b2.transform.localScale = new Vector3(scale.x / 2, scale.y / 2, scale.z / 2);
                                b2.transform.position = new Vector3(transform.position.x - ((scale.x / 4) + (scale.x / 2) * 0.2f), transform.position.y, transform.position.z);
                                
                                //Destroy the current sphere
                                Engine.PushMsg(Msg);
                                Destroy(gameObject);
                            }
                            
                            break;
                        default:
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)UserActions.Start:
                            break;
                        
                        default:
                            break;

                    }
                    Engine.PushMsg(Msg);
                }
                break;
        }
    }
}
