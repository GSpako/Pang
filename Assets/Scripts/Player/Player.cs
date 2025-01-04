
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

enum PlayerActions { Move, StopShooting, Start, Sleep, WakeUp, End }

// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class Player : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, fiftyMillis, halfSecond, tenMillis, reloadTimeHRT;

    public float speed = 1.0f;
    public Vector3 movement = new Vector3(0, 0, 0);
    bool shooting = false;
    MessageManager gunMailBox;

    bool paused = false;

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
        //Get the engine
        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;
        RTDESKInputManager IM = Engine.GetInputManager();

        //Register keys that we want to be signaled in case the user press them
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.W); //shoot gun
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.A); //move left
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.D); //,pve rogjt

        //Create cariables for sendTime
        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        fiftyMillis = Engine.ms2Ticks(50);
        oneSecond = Engine.ms2Ticks(1000);

        //Send the start message
        Action ActMsg;
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
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
                        //Shoot the gun when not shooting alredy, this way you cant simply spam the button
                        if (KeyState.DOWN == IMsg.s && !shooting && !paused)
                        {
                            //Send a msg to indicate the gun to shoot
                            Action gunMsg;
                            gunMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            gunMsg.action = (int)UserActions.Start;
                            Engine.SendMsg(gunMsg, gameObject, gunMailBox, HRTimer.HRT_INMEDIATELY);
                            
                            //Mark the player as shooting, so it cant move during the cooldown
                            shooting = true;
                        }
                        break;

                    case KeyCode.D:
                        //Add movement speed if the key is pressed down, remove if pressed up
                        if (KeyState.DOWN == IMsg.s)
                            movement += new Vector3(speed, 0, 0);
                        else
                            movement -= new Vector3(speed, 0, 0);

                        break;
                    case KeyCode.A:
                        //Add movement speed if the key is pressed down, remove if pressed up
                        if (KeyState.DOWN == IMsg.s)
                            movement -= new Vector3(speed, 0, 0);
                        else
                            movement += new Vector3(speed, 0, 0);

                        break;
                }
                Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Position:
                // Only move when not shooting, or when there is any spedd
                if (movement != Vector3.zero && !shooting && !paused)
                {
                    transform.Translate(movement);
                    //Move the character, limit the movement to the screen/lvl bounds
                    Vector3 pos = transform.position;

                    //limit the movement so it doesnt go outside the map
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
                        case (int)PlayerActions.StopShooting:
                            break;
                        case (int)PlayerActions.Sleep:
                            break;
                        case (int)PlayerActions.WakeUp:
                            break;
                        case (int)PlayerActions.End:
                            break;
                        case (int)PlayerActions.Start:
                            //Start the movement updating, with this we will move the caracter whenever the speed is apropiate
                            p = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                            Engine.SendMsg(p, gameObject, ReceiveMessage, fiftyMillis);

                            //Push the msg
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
                        case (int)UserActions.GetSteady: //Stop the movement of the object when pausing
                            paused = true;
                            break;
                        case (int)UserActions.Move: //Renable the movement of the object when pausing
                            paused = false;
                            break;
                        case (int)UserActions.LiveState: // Renable movement after the shooting coldown is over
                            Debug.Log("recived");
                            shooting = false;
                            break;
                    }
                    Engine.PushMsg(Msg);
                }
                break;
        }
    }
}
