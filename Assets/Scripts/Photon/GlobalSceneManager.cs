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

    bool started{get;set;}

    void Awake(){
        if (Instance == null){
            Instance = this;
        }
    }





}
