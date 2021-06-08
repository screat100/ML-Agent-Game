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

    public static Player.Team playersTeam;

    public static Phase phase;

    public static int policeNum;
    public static int thiefNum;


    public static bool flowWatingTime = false;
    public static float watingTime = 0f;
    public static bool flowPlayingTime = false;
    public static float playingTime = 0f;


    public static int stage;
    public static int winNum_Police;
    public static int winNum_Thief;
    
    
}
