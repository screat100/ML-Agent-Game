using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Result : MonoBehaviour
{

    public GameObject image_win;
    public GameObject image_lose;

    SoundManager soundManager;

    void Start()
    {
        soundManager = GameObject.Find("Sounds").GetComponent<SoundManager>();

        if(GameManager.playersTeam == GameManager.recentWinner)
        {
            image_win.SetActive(true);
            soundManager.PlayAudio("Win");
        }

        else
        {
            image_lose.SetActive(true);
            soundManager.PlayAudio("Lose");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
