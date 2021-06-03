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


    // UI_Canvas
    public static GameObject ui_MainMenu = GameObject.Find("UI_MainMenu");
    public static GameObject ui_Setting = GameObject.Find("UI_Setting");
    public static GameObject ui_SelectTeam= GameObject.Find("UI_SelectTeam");
    public static GameObject ui_playing= GameObject.Find("UI_Playing");
    public static GameObject ui_result= GameObject.Find("UI_Result");
    
    // UI_Setting
    public static GameObject text_policeNum = GameObject.Find("text_policeNum");
    public static GameObject text_thiefNum = GameObject.Find("text_thiefNum");


    // UI_playing
    public static Text watingTimeText = GameObject.Find("text_waitingTime").GetComponent<Text>();
    public static Text playingTimeText = GameObject.Find("text_playingTime").GetComponent<Text>();



    public static Player m_Player = GameObject.Find("Player").GetComponent<Player>();



    public static int PoliceNum;
    public static int ThiefNum;


    // play information
    public static bool flowWatingTime = false;
    public static float watingTime = 0f;
    public static bool flowPlayingTime = false;
    public static float playingTime = 0f;


    public static Player.Team recentWinner;


    public static int stage;
    public static int winNum_Police;
    public static int winNum_Thief;
    
    
}
