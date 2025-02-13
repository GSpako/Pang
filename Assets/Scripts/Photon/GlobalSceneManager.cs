using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GlobalSceneManager : NetworkBehaviour
{
    public static GlobalSceneManager Instance { get; private set;}

    private List<LocalSceneManager> scenes = new List<LocalSceneManager>();

    private List<LocalSceneManager> activeScenes = new List<LocalSceneManager>();

    private int playingScenes;

    bool started{get;set;}

    void Awake(){
        if (Instance == null){
            Instance = this;
        }

    }

    public override void Spawned(){
        Debug.Log("Spawned");
        if (Runner.GameMode == GameMode.Host){
            Debug.Log("Hello");
        }
    }

    public void AddScene(LocalSceneManager l){
        scenes.Add(l);
    }

    public void RemoveScene(LocalSceneManager l){
        scenes.Remove(l);
    }

    public void StartManager(){
        playingScenes = scenes.Count;
        activeScenes = scenes;
        foreach (LocalSceneManager l in activeScenes){
            l.SpawnBall();
        }
    }

    public void SceneLost(LocalSceneManager l){

        activeScenes.Remove(l);
        playingScenes--;
        if(playingScenes <= 0){
            EndGame();
        }
    }

    private void EndGame(){
        started = false;
        Debug.Log("Acabouse");
        //Decir quien gana y toda la pesca
    }


    private void OnGUI()
    {
        if (Runner.GameMode == GameMode.Host) {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Start")){
                Debug.Log("HOLIIII");
                if(started == false){
                    started = true;
                    StartManager();
                }
            }
        }
    }

}
