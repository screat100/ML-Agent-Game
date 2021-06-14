using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /* ===      Sub Classes       === */
    public enum Phase
    {
        menu,
        selectTeam,
        waitLoading,
        policesWating,
        play,
        result,
    }


    /* ===      Variables       === */


    // for stage
    public Player.Team playersTeam;

    public Phase phase;

    public int policeNum = 2;
    public int thiefNum = 4;

    public int round = 0;
    public int winNum_Police = 0;
    public int winNum_Thief = 0;
    public Player.Team recentWinner;

    // for user setting
    public float bgmVolume = 1;
    public float sfxVolume = 1;

    // for game record
    public int policePlayerRecord;
    public float thiefPlayerRecord;
    
    
}
