using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ingame : MonoBehaviour
{
    Text[] texts;

    void Start()
    {
        // Listing UI Objects
        texts = GameObject.Find("texts").GetComponentsInChildren<Text>();

        // 팀에 따라 게임 목표 메시지를 다르게 표기
        Text goalText = FindTextObject("text_goalMessage");
        if(GameManager.playersTeam == Player.Team.police)
        {
            goalText.text = "모든 도둑을 잡아라!";
        }
        else
        {
            goalText.text = "경찰을 피해 보석을 찾아 탈출하라!";
        }

        // 라운드 및 승 수 정보 표기
        FindTextObject("text_round").text = "Round " + GameManager.round.ToString();
        FindTextObject("text_winnumOfPolice").text = GameManager.winNum_Police.ToString();
        FindTextObject("text_winnumOfThief").text = GameManager.winNum_Thief.ToString();


    }

    Text FindTextObject(string name)
    {
        for(int i=0; i<texts.Length; i++)
        {
            if(texts[i].name == name)
                return texts[i];
        }

        return null;
    }

}
