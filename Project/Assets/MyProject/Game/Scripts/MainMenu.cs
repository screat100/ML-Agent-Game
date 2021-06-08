using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject UI_MainMenu;
    public GameObject UI_Setting;
    public GameObject UI_SelectTeam;

    void Start()
    {
        GoToMainMenu();

        SetDefaultValueOfGameManager();
    }

    void SetDefaultValueOfGameManager()
    {

        GameManager.PoliceNum = 2;
        GameManager.ThiefNum = 4;
    }


    public void GoToMainMenu()
    {
        UI_MainMenu.SetActive(true);
        UI_SelectTeam.SetActive(false);
        UI_Setting.SetActive(false);
    }

    public void GoToSetting()
    {
        UI_MainMenu.SetActive(false);
        UI_SelectTeam.SetActive(false);
        UI_Setting.SetActive(true);
    }

    public void GoToSelectTeam()
    {
        UI_MainMenu.SetActive(false);
        UI_SelectTeam.SetActive(true);
        UI_Setting.SetActive(false);
    }

    public void ExitProgram()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }

    /* ===      Setting     === */
    public void AddPoliceNum(bool isPlus)
    {
        if(isPlus)
        {
            if(GameManager.PoliceNum < 3)
                GameManager.PoliceNum++;

        }
        else 
        {
            if(GameManager.PoliceNum > 2)
                GameManager.PoliceNum--;
        }

        refreshMemberNumText();

    }
    public void AddThiefNum(bool isPlus)
    {
        if(isPlus)
        {
            if(GameManager.ThiefNum < 6)
                GameManager.ThiefNum++;

        }
        else 
        {
            if(GameManager.ThiefNum > 4)
                GameManager.ThiefNum--;
        }

        refreshMemberNumText();
    }
    private void refreshMemberNumText()
    {
        GameObject.Find("text_policeNum").GetComponent<Text>().text = GameManager.PoliceNum.ToString();
        GameObject.Find("text_thiefNum").GetComponent<Text>().text = GameManager.ThiefNum.ToString();
    }

    /* ===      Select Team     === */

    public void SelectPoliceTeam()
    {
        GameManager.playersTeam = Player.Team.police;
        ChangePhaseToWait();
    }

    public void SelectThiefTeam()
    {
        GameManager.playersTeam = Player.Team.thief;
        ChangePhaseToWait();
    }

    public void ChangePhaseToWait() 
    {
        GameManager.phase = GameManager.Phase.wait;
    }

}
