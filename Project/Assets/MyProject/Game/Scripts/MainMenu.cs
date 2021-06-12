using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        GameManager.policeNum = 2;
        GameManager.thiefNum = 4;

        GameManager.stage = 0;
        GameManager.winNum_Police = 0;
        GameManager.winNum_Thief = 0;

        GameManager.bgmVolume = 100;
        GameManager.sfxVolume = 100;
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
            if(GameManager.policeNum < 3)
                GameManager.policeNum++;

        }
        else 
        {
            if(GameManager.policeNum > 2)
                GameManager.policeNum--;
        }

        refreshMemberNumText();

    }
    public void AddThiefNum(bool isPlus)
    {
        if(isPlus)
        {
            if(GameManager.thiefNum < 6)
                GameManager.thiefNum++;

        }
        else 
        {
            if(GameManager.thiefNum > 4)
                GameManager.thiefNum--;
        }

        refreshMemberNumText();
    }
    private void refreshMemberNumText()
    {
        GameObject.Find("text_policeNum").GetComponent<Text>().text = GameManager.policeNum.ToString();
        GameObject.Find("text_thiefNum").GetComponent<Text>().text = GameManager.thiefNum.ToString();
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
        GameManager.phase = GameManager.Phase.waitLoading;
        SceneManager.LoadScene("Stage");
    }

}
