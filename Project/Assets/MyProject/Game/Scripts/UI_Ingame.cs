using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ingame : MonoBehaviour
{
    Text[] texts;

    /* 미니 맵 */
    Camera m_minimapCam;

    /* 미니맵 cullingMask 역할별 Layer설정*/
    string[] thief_cullingLayer = new string[] { "goal", "thief_img", "wall", "sensedthing" };
    string[] police_cullingLayer = new string[] {"police_img", "wall","sensedthing" };

    void Start()
    {
        // Listing UI Objects
        texts = GameObject.Find("texts").GetComponentsInChildren<Text>();
        m_minimapCam = GameObject.Find("Minimap_Camera").GetComponent<Camera>();

        // 팀에 따라 게임 목표 메시지를 다르게 표기
        Text goalText = FindTextObject("text_goalMessage");
        if(GameManager.instance.playersTeam == Player.Team.police)
        {
            goalText.text = "모든 도둑을 잡아라!";
            m_minimapCam.cullingMask = LayerMask.GetMask(police_cullingLayer);
        }
        else
        {
            goalText.text = "경찰을 피해 보석을 찾아 탈출하라!";
            m_minimapCam.cullingMask = LayerMask.GetMask(thief_cullingLayer);
        }

        // 라운드 및 승 수 정보 표기
        FindTextObject("text_round").text = "Round " + GameManager.instance.round.ToString();
        FindTextObject("text_winnumOfPolice").text = GameManager.instance.winNum_Police.ToString();
        FindTextObject("text_winnumOfThief").text = GameManager.instance.winNum_Thief.ToString();
        FindTextObject("text_remainedThief").text = GameObject.Find("GameArea").GetComponent<StageSetting>().willCatchNum.ToString();
    }
    private void FixedUpdate()
    {
        /*남은 도둑 수 변경*/
        FindTextObject("text_remainedThief").text = GameObject.Find("GameArea").GetComponent<StageSetting>().willCatchNum.ToString();
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
