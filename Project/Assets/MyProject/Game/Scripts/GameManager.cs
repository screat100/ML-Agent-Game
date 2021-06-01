using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GameManager
{
    /* ===      Sub Classes       === */
    public enum Phase
    {
        menu,
        selectTeam,
        wait,
        thiefOnly,
        play,
        result,
    }


    /* ===      Variables       === */

    public static Phase phase;

    public static GameObject ui_MainMenu = GameObject.Find("UI_MainMenu");
    public static GameObject ui_SelectTeam= GameObject.Find("UI_SelectTeam");
    public static GameObject ui_playing= GameObject.Find("UI_Playing");
    public static GameObject ui_result= GameObject.Find("UI_Result");
    
    public static Text watingTimeText = GameObject.Find("text_waitingTime").GetComponent<Text>();
    public static Text playingTimeText = GameObject.Find("text_playingTime").GetComponent<Text>();

    public static Player m_Player = GameObject.Find("Player").GetComponent<Player>();

    public static int PoliceNum;
    public static int ThiefNum;

    public static bool flowWatingTime = false;
    public static float watingTime = 0f;
    public static bool flowPlayingTime = false;
    public static float playingTime = 0f;



    
    
}
