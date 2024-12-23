
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
[RequireComponent(typeof(RTDESKEntity))]
public class GameManager : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis;
    MessageManager playerMailBox, timerMailBox;

    public GameObject pauseMenu;
    public GameObject DeathMenu;

    bool isPaused = false;

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

        playerMailBox = RTDESKEntity.getMailBox("Player");
        timerMailBox = RTDESKEntity.getMailBox("TimerManager");
        //Register keys that we want to be signaled in case the user press them
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.P);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                RTDESKInputMsg IMsg = (RTDESKInputMsg)Msg;
                switch (IMsg.c)
                {

                    case KeyCode.P:
                        if (KeyState.DOWN == IMsg.s)
                        {
                            if (isPaused)
                            {
                                isPaused = false;
                                Unpause();
                            }
                            else
                            {
                                isPaused = true;
                                Pause();
                            }
                        }
                    break;
                }
                Engine.PushMsg(Msg);
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
               
                switch ((int)a.action)
                {
                    case (int)UserActions.Start:
                        break;
                    case (int)UserActions.End:
                        Death();
                        break;
                }
                Engine.PushMsg(Msg);
                
                break;
        }
    }
    public bool ded = false;
    void Death() 
    {
        ded = true;
        DeathMenu.SetActive(true);
        Time.timeScale = 0; 
        isPaused = true;

        Action ActMsg;
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ActMsg.action = (int)UserActions.GetSteady;
        Engine.SendMsg(ActMsg, gameObject, playerMailBox, HRTimer.HRT_INMEDIATELY);
        Engine.SendMsg(ActMsg, gameObject, timerMailBox, HRTimer.HRT_INMEDIATELY);
    }

    // Reloads the current scene
    public void ReloadScene()
    {
        Time.timeScale = 1; // Ensure the game is unpaused
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Scene Reloaded");
    }

    public void MainMenu()
    {
        Time.timeScale = 1; // Ensure the game is unpaused
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    void Pause()
    {
        if (ded) return;
        Time.timeScale = 0; // Pauses the game
        isPaused = true;
        pauseMenu.SetActive(true);
        Debug.Log("Game Paused");

        Action ActMsg;
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ActMsg.action = (int)UserActions.GetSteady;
        Engine.SendMsg(ActMsg, gameObject, playerMailBox, HRTimer.HRT_INMEDIATELY);
        Engine.SendMsg(ActMsg, gameObject, timerMailBox, HRTimer.HRT_INMEDIATELY);
    }

    public void Unpause()
    {
        if (ded) return;
        Time.timeScale = 1; // Resumes the game
        isPaused = false;
        Debug.Log("Game Unpaused");
        pauseMenu.SetActive(false);

        Action ActMsg;
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ActMsg.action = (int)UserActions.Move;
        Engine.SendMsg(ActMsg, gameObject, playerMailBox, HRTimer.HRT_INMEDIATELY);
        Engine.SendMsg(ActMsg, gameObject, timerMailBox, HRTimer.HRT_INMEDIATELY);
    }
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        // Stop the simulation in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}

