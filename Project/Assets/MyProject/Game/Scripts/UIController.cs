using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    StageSetting m_StageSetting;
    Player m_Player;


    /* ===      base functions       === */
    void Start() 
    {
        m_StageSetting = GameObject.Find("GameArea").GetComponent<StageSetting>();

        // Activate Player's Controll
        GameManager.m_Player.controllActivate = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;


        // Let Wating time flow
        GameManager.watingTime = 3f;
        GameManager.flowWatingTime = true;

        GameManager.watingTimeText.gameObject.SetActive(true);
        GameManager.playingTimeText.gameObject.SetActive(false);

        // Initialization
        m_StageSetting.ResetInitialInformation();
    }

    void Update()
    {
        if(GameManager.phase == GameManager.Phase.wait 
        && GameManager.flowWatingTime)
        {
            GameManager.watingTimeText.text = ((int)GameManager.watingTime+1).ToString();
            GameManager.watingTime -= Time.deltaTime;

            if(GameManager.watingTime <= 0f)
            {
                ChangePhaseToThiefOnly();
            }
        }

        else if (GameManager.phase == GameManager.Phase.thiefOnly 
        && GameManager.flowWatingTime)
        {
            GameManager.watingTimeText.text = ((int)GameManager.watingTime+1).ToString();
            GameManager.watingTime -= Time.deltaTime;

            if(GameManager.watingTime <= 0f)
            {
                ChangePhaseToPlay();
            }
        }

        else if (GameManager.phase == GameManager.Phase.play
        && GameManager.flowPlayingTime)
        {
            GameManager.playingTime += Time.deltaTime;

            int minute = (int)((int)GameManager.playingTime / 60);
            int second = (int)GameManager.playingTime - minute*60; 
            
            if(second < 10)
                GameManager.playingTimeText.text = "0"+ minute.ToString() + " : 0" + second.ToString();
            else 
                GameManager.playingTimeText.text = "0"+ minute.ToString() + " : " + second.ToString();

            if(GameManager.playingTime >= 300f)
            {
                // Time Over!
            }
        }
    }


    public void ChangePhaseToThiefOnly() 
    {
        GameManager.phase = GameManager.Phase.thiefOnly;
        GameManager.watingTime = 10f;
    }

    public void ChangePhaseToPlay() 
    {
        GameManager.phase = GameManager.Phase.play;

        GameManager.playingTimeText.gameObject.SetActive(true);
        GameManager.watingTimeText.gameObject.SetActive(false);

        GameManager.flowPlayingTime = true;
        GameManager.playingTime = 0f;
    }

    public void ChangePhaseToResult() 
    {
        GameManager.phase = GameManager.Phase.result;
        GameManager.ui_MainMenu.SetActive(false);
        GameManager.ui_SelectTeam.SetActive(false);
        GameManager.ui_playing.SetActive(false);
        GameManager.ui_result.SetActive(true);

        GameManager.m_Player.controllActivate = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }



}
