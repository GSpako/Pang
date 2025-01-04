using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControll : MonoBehaviour
{
    
    // Method to exit the game (or stop play mode in the Unity Editor)
    public void ExitGame()// exit game, and stop simulation
    {
        Debug.Log("Exiting game...");
        // Stop the simulation in the Unity Editor
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void LoadGameScene() // load game scene
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
        Debug.Log("Loading scene: Game");
    }

}
