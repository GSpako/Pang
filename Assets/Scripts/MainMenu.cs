using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.SceneManagement.SceneManager;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadCarlosLvl()
    {
        LoadScene(2);
    }

    public void loadPakoLvl()
    {
        LoadScene(1);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
