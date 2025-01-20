
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
using UnityEngine.SceneManagement;


// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity), typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis;

    AudioSource source;
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
        source = GetComponent<AudioSource>();
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)UserMsgTypes.Position:
                break;
            case (int)UserMsgTypes.Rotation:
                break;
            case (int)UserMsgTypes.Scale:
                break;
            case (int)UserMsgTypes.TRE:
                break;
            case (int)UserMsgTypes.Audio:
                AudioMsg a;
                AudioClip clip;
                a = (AudioMsg)Msg;
                clip = a.audio;
                source.clip = clip;
                source.Play();
                Engine.PushMsg(Msg);
                
                break;
            default:
                Engine.PushMsg(Msg);
                break;
        }
    }
}