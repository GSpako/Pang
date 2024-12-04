/**
 * Cohete: Basic Message Management for the cube game object
 *
 * Copyright(C) 2022
 *
 * Prefix: Coh_

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

using System;
using UnityEngine;

//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

public class Cohete : MonoBehaviour
{
    private enum AnimatedChannels { Speed, Rotation, Fuel, ChangeColor, End, Destroy }

    // In ticks to wait until next sampling
    HRT_Time[] samplingPeriod           = new HRT_Time  [Enum.GetNames(typeof(AnimatedChannels)).Length];
    float   [] samplingPeriodSeconds    = new float     [Enum.GetNames(typeof(AnimatedChannels)).Length];

    public float linearSpeed, //in m/s
                 angularSpeed,//in laps/s

                 fuelTank,    //in liters
                 fuelSpeed;   //in liters/s

    double NSSP;    //Nyquist-Shannon Sampling Period
    bool sendingMsgs = true;

    [SerializeField]
    GameObject RTDESKEngineObject;
    RTDESKEngine Engine;

    MeshRenderer renderComponent;

    void sendMsg(MsgContent Msg, HRT_Time DeltaTime)
    {
        if (sendingMsgs)
            Engine.SendMsg(Msg, DeltaTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        Transform     SpeedMsg;
        Action        RotMsg, ActionMsg;

        transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f), Space.Self);
        transform.localScale = new Vector3(0.0075f, 0.0075f, 0.0075f);

        angularSpeed = UnityEngine.Random.Range(100, 1000);
        //Set sampling period to match 1.5 times the Nyquist-Shannon theorem
        NSSP = 1.0d/(angularSpeed * 2.0d * 1.5d);
        angularSpeed *= 360.0f;//Convert to degrees/s

        samplingPeriodSeconds[(int)AnimatedChannels.Rotation]   = (float)NSSP;
        samplingPeriodSeconds[(int)AnimatedChannels.Speed]      = 1.0f/50.0f;
        samplingPeriodSeconds[(int)AnimatedChannels.Fuel]       = 0.001f;    //1 KHz

        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;

        samplingPeriod[(int)AnimatedChannels.Rotation]  = Engine.ms2Ticks(1000.0f * NSSP);
        samplingPeriod[(int)AnimatedChannels.Speed]     = Engine.ms2Ticks(50);   //20 Hz
        samplingPeriod[(int)AnimatedChannels.Fuel]      = Engine.ms2Ticks(1);    //1 KHz

        //Create the messages to send
        //Debug.Log("Solicitud de mensaje de velocidad de cohete");
        //Get a new message to change position
        SpeedMsg            = (Transform)Engine.PopMsg((int)UserMsgTypes.Speed);
        //Updtate origin and destination of the message/envelope
        SpeedMsg.Sender     = gameObject;
        SpeedMsg.Receiver   = ReceiveMessage;
        //Update the content of the message
        SpeedMsg.V3 = UnityEngine.Random.insideUnitSphere;
        if (SpeedMsg.V3.y < 0) SpeedMsg.V3.y = -SpeedMsg.V3.y;
        SpeedMsg.V3 *= linearSpeed;

        //Debug.Log("Solicitud de mensaje de rotación de cohete");
        //Get a new message to change position
        RotMsg          = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Updtate origin and destination of the message/envelope
        RotMsg.Sender   = gameObject;
        RotMsg.Receiver = ReceiveMessage;
        RotMsg.action   = (int)AnimatedChannels.Rotation;

        fuelTank  = UnityEngine.Random.Range(1000, 2000);
        fuelSpeed = UnityEngine.Random.Range(1, 2);

        //Debug.Log("Solicitud de mensaje de control de combustible");
        //Get a new message to activate a new action in the object
        ActionMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Updtate origin and destination of the message/envelope
        ActionMsg.Sender   = gameObject;
        ActionMsg.Receiver = ReceiveMessage;
        //Update the content of the message sending and activation 
        ActionMsg.action = (int)AnimatedChannels.Fuel;
        Engine.SendMsg(ActionMsg, samplingPeriod[(int)AnimatedChannels.Fuel]);

        //Debug.Log("Solicitud de mensaje de control de cambio de color");
        //Get a new message to activate a new action in the object
        ActionMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Updtate origin and destination of the message/envelope
        ActionMsg.Sender = gameObject;
        ActionMsg.Receiver = ReceiveMessage;
        //Update the content of the message sending and activation 
        ActionMsg.action = (int)AnimatedChannels.ChangeColor;
        Engine.SendMsg(ActionMsg, Engine.ms2Ticks(300));

        //Debug.Log("Solicitud de mensaje de control de finalización");
        //Get a new message to activate a new action in the object
        ActionMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Updtate origin and destination of the message/envelope
        ActionMsg.Sender = gameObject;
        ActionMsg.Receiver = ReceiveMessage;
        //Update the content of the message sending and activation 
        ActionMsg.action = (int)AnimatedChannels.End;
        Engine.SendMsg(ActionMsg, Engine.ms2Ticks(5000));

        renderComponent = GetComponent<MeshRenderer>();
        if (null == renderComponent) Debug.Log("Error. No se ha podido obtener el mesh renderer del cohete");

        //Send the messages to itself
        //Debug.Log("Enviando mensajes de activación");
        Engine.SendMsg(SpeedMsg, samplingPeriod[(int)AnimatedChannels.Speed]);
        Engine.SendMsg(RotMsg, samplingPeriod[(int)AnimatedChannels.Rotation]);      
    }

    /**
        * @fn void ReceiveMessage(MsgContent Msg)
        * Manages a time ordered user message
        * @param Msg The data structure that holds the user data properly
        */
    void ReceiveMessage(MsgContent Msg)
    {
        Transform p;

        switch (Msg.Type)
        {
            case (int)UserMsgTypes.Speed:
                //Update the content of the message
                //Debug.Log("Speed");
                p = (Transform)Msg;
                transform.Translate(p.V3 * samplingPeriodSeconds[(int)AnimatedChannels.Speed]);
                sendMsg(Msg, samplingPeriod[(int)AnimatedChannels.Speed]);
                //Este mensaje se reutiliza para volver a mandarse a sí mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                break;
            case (int)UserMsgTypes.Rotation:
                //Debug.Log("Rotation");
                //Update the content of the message
                transform.Rotate(Vector3.forward * samplingPeriodSeconds[(int)AnimatedChannels.Rotation]* angularSpeed);
                sendMsg(Msg, samplingPeriod[(int)AnimatedChannels.Rotation]);
                //Este mensaje se reutiliza para volver a mandarse a sí mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                break;
            case (int)UserMsgTypes.Action:
                //Debug.Log("Action");
                Action a;
                a = (Action)Msg;
                //Sending automessage
                switch ((int)a.action)
                {
                    case (int)AnimatedChannels.Fuel:
                        //Debug.Log("Calculating fuel comsumption");
                        fuelTank -= fuelSpeed * samplingPeriodSeconds[(int)AnimatedChannels.Fuel];

                        //Debug.Log("Sampling period Fuel " + samplingPeriod[(int)AnimatedChannels.Fuel]);
                        //Reuse the received message to resend it again to itself
                        sendMsg(Msg, samplingPeriod[(int)AnimatedChannels.Fuel]);
                        break;
                    case (int)AnimatedChannels.ChangeColor:
                        //Debug.Log("Change color");
                        Color c = new Color();
                        c = renderComponent.material.color;
                        c.r *= 0.7f;
                        c.g *= 0.7f;
                        c.b *= 0.7f;
                        renderComponent.material.SetColor("_Color", c);
                        //Dispose the message not used
                        Engine.PushMsg(Msg);
                        break;
                    case (int)AnimatedChannels.End:
                        //Debug.Log("Ending rocket");

                        a.action = (int)AnimatedChannels.Destroy;
                        //Dispose the message not used
                        sendMsg(a, Engine.ms2Ticks(300.0f));
                        //Stop sending messages
                        sendingMsgs = false;
                        break;
                    case (int)AnimatedChannels.Destroy:
                        //Debug.Log("Destroy rocket");

                        Engine.PushMsg(Msg);
                        //Dispose the message not used
                        Destroy(gameObject);
                        break;
                    default:
                        Engine.PushMsg(Msg);
                        break;
                }
                break;
        }
    }
}
