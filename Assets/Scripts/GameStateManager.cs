using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject DeathMenu;
    public GameObject VictoryMenu;

    public AudioClip ballDestroyed;
    public AudioClip music;
    public AudioSource soundSource;
    public bool dead = false;
    public int numLeft = 8;


    bool paused = false;

    private void Start()
    {
        Time.timeScale = 1;
        soundSource = this.gameObject.AddComponent<AudioSource>();
        soundSource.clip = music;
        soundSource.loop = true;
        soundSource.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                Unpause();
                paused = false;
            }
            else
            {
                Pause();
                paused = true;
            }
        }
    }

    public void RemoveBall()
    {
        numLeft--;
        if (numLeft == 0)
        {
            Victory();
        }
        soundSource.PlayOneShot(ballDestroyed);
    }

    public void Death()
    {
        dead = true;
        DeathMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void Victory()
    {
        dead = true;
        VictoryMenu.SetActive(true);
        Time.timeScale = 0;
    }

    // Reloads the current scene
    public void ReloadScene()
    {
        Time.timeScale = 1; // Ensure the game is unpaused
        SceneManager.LoadScene(1, LoadSceneMode.Single);
        Debug.Log("Scene Reloaded");
    }

    public void MainMenu()
    {
        Time.timeScale = 1; // Ensure the game is unpaused
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void Pause()
    {
        if (dead) return;
        Time.timeScale = 0; // Pauses the game
        pauseMenu.SetActive(true);
        Debug.Log("Game Paused");
    }

    public void Unpause()
    {
        if (dead) return;
        Time.timeScale = 1; // Resumes the game
        Debug.Log("Game Unpaused");
        pauseMenu.SetActive(false);
    }
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        // Stop the simulation in the Unity Editor
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
