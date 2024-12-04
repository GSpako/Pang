using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CapsuleReceiveMessage : MonoBehaviour
{
    float startTime;
    [SerializeField]
    float deltaTime;

    [SerializeField]
    GameObject rocket, cube;
    MessageManager CubesManagerMailBox;
    RTDESKEngine engine;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = MailBox;

        CubesManagerMailBox = RTDESKEntity.getMailBox("Cubes Manager");
        if (null == CubesManagerMailBox) Debug.Log("CubesManagerMailBox 2nd is null");
    }

    // Update is called once per frame
    void Update()
    {
        if((Time.time - startTime) > deltaTime)
        {
            Vector3 t = transform.forward * 3.0f;
            t.y += 0.5f;
            startTime = Time.time;
            transform.Rotate(new Vector3(0.0f, 37.0f, 0.0f));

            GameObject.Instantiate(rocket, t, transform.rotation);
            ObjectMsg Msg = (ObjectMsg)engine.PopMsg((int)UserMsgTypes.Object);
            Msg.o = GameObject.Instantiate(cube, transform.forward, transform.rotation);

            Debug.Log(gameObject.name + ": Sending " + Msg + " to " + CubesManagerMailBox);    

            engine.SendMsg(Msg, gameObject, CubesManagerMailBox, HRTimer.HRT_INMEDIATELY);
        }
    }
    void MailBox(MsgContent Msg)
    {
    }   
}
