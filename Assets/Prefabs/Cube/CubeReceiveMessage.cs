/**
 * CubeReceiveMessage: Basic Message Management for the cube game object
 *
 * Copyright(C) 2022
 *
 * Prefix: CRM_

 * @Author: Dr. Ramón Mollá Vayá
 * @Date:	11/2022
 * @Version: 2.0
 *
 * Update:
 * Date:	
 * Version: 
 * Changes:
 *
 */

#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

using UnityEngine;

//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

enum CubeActions {Idle, Start, Sleep, Watch, Run, Walk, Turn, ChangeColor }

public class LiveState
{
    public float life,  ///<Amount of live in the interval [0,1]
                 gas;   ///<Amount of live in the interval [0,1]
    public uint  ammo;  ///<Amount of bullets remaining [0,100)
}

// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class CubeReceiveMessage : MonoBehaviour
{
    public enum CubeStates{
        Moving,
        Steady
    }

    //Initial cube state
    CubeStates state = CubeStates.Moving;

    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis;

    [SerializeField]
     Vector3            speed;
     RTDESKEngine       Engine;   //Shortcut

    LiveState lifeState;

    Renderer renderComponent;

    string myName;

    private void Awake()
    {
       
    }

    // Start is called before the first frame update
    void Start()
    {
        Transform       PosMsg, RotMsg;
        Action          ActMsg;
        RTDESKEntity rTDESKEntity = GetComponent<RTDESKEntity>();

        Engine = rTDESKEntity.RTDESKEngineScript;
        RTDESKInputManager IM = rTDESKEntity.GetRTDESKInputManager();
        //Assign the "listener" to the normalized component RTDESKEntity. Every gameObject that wants to receive a message must have a public mailbox
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;

        //Register keys that we want to be signaled in case the user press them
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.UpArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.DownArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.LeftArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.RightArrow);

        //Debug.Log("Solicitud de Posición");
        //Get a new message to change position
        PosMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
        //Update the content of the message
        PosMsg.V3 = speed; // new Vector3(0.0005f, 0.0002f, 0.001f);

        //Debug.Log("Solicitud de rotación");
        //Get a new message to change position
        RotMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Rotation);
          //Update the content of the message
        RotMsg.V3 = new Vector3(0.01f, 0.011f, 0.015f);

        //Debug.Log("Solicitud de Acción por cubo");
        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)CubeActions.ChangeColor;

        myName = gameObject.name;

        renderComponent = GetComponent<Renderer>();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis  = Engine.ms2Ticks(10);
        oneSecond  = Engine.ms2Ticks(1000);

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, halfSecond);
        Engine.SendMsg(PosMsg, gameObject, ReceiveMessage, halfSecond);
        Engine.SendMsg(RotMsg, gameObject, ReceiveMessage, halfSecond);
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
                    case KeyCode.UpArrow:
                        if (KeyState.DOWN == IMsg.s)
                            transform.Translate(Vector3.forward);
                        //Debug.Log("Flecha arriba");
                        break;
                    case KeyCode.DownArrow:
                        if (KeyState.DOWN == IMsg.s)
                            transform.Translate(Vector3.back);
                        //Debug.Log("Flecha abajo");
                        break;
                    case KeyCode.LeftArrow:
                        if (KeyState.DOWN == IMsg.s)
                            transform.Translate(Vector3.left);
                        //Debug.Log("Flecha a la izquierda");
                        break;
                    case KeyCode.RightArrow:
                        if (KeyState.DOWN == IMsg.s)
                            transform.Translate(Vector3.right);
                        //Debug.Log("Flecha a la derecha");
                        break;
                    }
                Engine.PushMsg(Msg);
                break;
            case (int)UserMsgTypes.Position:
                //Avoids mesages sent by error. Only recognize self messages
                //Debug.Log("Sender name " + Msg.Sender.name + " My name " + myName);
                if (myName == Msg.Sender.name)
                {
                    //Update the content of the message
                    p = (Transform)Msg;
                    if (CubeStates.Moving == state)
                    {
                        transform.Translate(p.V3);
                        Engine.SendMsg(Msg, tenMillis);
                        //Este mensaje se reutiliza para volver a mandarse a sí mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                    }
                    else Engine.PushMsg(Msg);
                }
                else Engine.PushMsg(Msg);
                break;
            case (int)UserMsgTypes.Rotation:
                Engine.PushMsg(Msg);
                break;
            case (int)UserMsgTypes.Scale:
                Engine.PushMsg(Msg);
                break;
            case (int)UserMsgTypes.TRE:
                Engine.PushMsg(Msg);
                break;
            case (int)UserMsgTypes.Action:
                Action a;
                a = (Action)Msg;
                //Sending automessage
                if (myName == Msg.Sender.name)
                    switch ((int)a.action)
                    {
                        case (int)CubeActions.Idle:
                            if (Engine.GetRealTime() - userTime > halfSecond)
                            {
                                a.action = (int)CubeActions.Walk;
                                //Reuse the received message to resend it again to itself
                                Engine.SendMsg(Msg, halfSecond);
                            }                                
                            break;
                        case (int)CubeActions.Sleep:
                            Engine.PushMsg(Msg);
                            break;
                        case (int)CubeActions.Watch:
                            Engine.PushMsg(Msg);
                            break;
                        case (int)CubeActions.Run:
                            Engine.PushMsg(Msg);
                            break;
                        case (int)CubeActions.Walk:
                            //Reactive behaviour. No responsive, no proactive
                            //Ya no se va a volver a gastar este tipo de mensaje. Devolver al pool
                            Engine.PushMsg(Msg);
                            break;
                        case (int)CubeActions.Turn:
                            Engine.PushMsg(Msg);
                            break;
                        case (int)CubeActions.ChangeColor:
                            Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
                            renderComponent.material.SetColor("_Color", randomColor);
                            Engine.SendMsg(Msg, (HRT_Time)(((double)oneSecond)*(1-Random.value*0.5d)));
                            //Este mensaje se reutiliza para volver a mandarse a sí mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                            break;
                        case (int)CubeActions.Start:
                            //We have to start the Cube behaviour
                            Engine.PushMsg(Msg);
                            break;
                        default:
                            Engine.PushMsg(Msg);
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)UserActions.GetSteady: //Stop the movement of the object
                            state = CubeStates.Steady;
                            break;
                        case (int)UserActions.Move:
                            state = CubeStates.Moving;
                            Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                            TMsg.V3 = new Vector3(0.0005f, 0.0002f, 0.001f);//speed;
                            Engine.SendMsg(TMsg, gameObject, ReceiveMessage, tenMillis);
                            break;
                    }
                    Engine.PushMsg(Msg);
                }
        break;
        }
    }
}
