using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDown : MonoBehaviour
{
    public TMP_Text TimeText;
    public int TotalTime = 30;

    GameStateManager gameManager;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameStateManager>();
    }

    // Update is called once per frame
    void Update()
    {
        float timeLeft = (TotalTime - Time.timeSinceLevelLoad);
        TimeText.text = ((int) timeLeft).ToString();

        if (timeLeft <= 0)
        {
            Debug.Log("Time Run Out");
            gameManager.Death();
        }
    }
}
