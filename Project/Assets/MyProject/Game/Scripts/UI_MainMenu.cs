using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    public GameObject UI_Menu;
    public GameObject UI_Setting;
    public GameObject UI_SelectTeam;

    public void GoToMainMenu()
    {
        UI_Menu.SetActive(true);
        UI_SelectTeam.SetActive(false);
        UI_Setting.SetActive(false);
    }

    public void GoToSetting()
    {
        UI_Menu.SetActive(false);
        UI_SelectTeam.SetActive(false);
        UI_Setting.SetActive(true);
    }

    public void GoToSelectTeam()
    {
        UI_Menu.SetActive(false);
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
        if (isPlus)
        {
            if (GameManager.instance.policeNum < 3)
                GameManager.instance.policeNum++;

        }
        else
        {
            if (GameManager.instance.policeNum > 2)
                GameManager.instance.policeNum--;
        }

        refreshMemberNumText();

    }
    public void AddThiefNum(bool isPlus)
    {
        if (isPlus)
        {
            if (GameManager.instance.thiefNum < 6)
                GameManager.instance.thiefNum++;

        }
        else
        {
            if (GameManager.instance.thiefNum > 4)
                GameManager.instance.thiefNum--;
        }

        refreshMemberNumText();
    }
    private void refreshMemberNumText()
    {
        GameObject.Find("text_policeNum").GetComponent<Text>().text = GameManager.instance.policeNum.ToString();
        GameObject.Find("text_thiefNum").GetComponent<Text>().text = GameManager.instance.thiefNum.ToString();
    }

    /* ===      Select Team     === */

    public void SelectPoliceTeam()
    {
        GameManager.instance.playersTeam = Player.Team.police;
        GameObject.Find("Sounds").GetComponent<SoundManager>().PauseAudio("MainMenuBGM");
        ChangePhaseToWait();
    }

    public void SelectThiefTeam()
    {
        GameManager.instance.playersTeam = Player.Team.thief;
        GameObject.Find("Sounds").GetComponent<SoundManager>().PauseAudio("MainMenuBGM");
        ChangePhaseToWait();
    }

    public void ChangePhaseToWait()
    {
        GameManager.instance.phase = GameManager.Phase.waitLoading;
        SceneManager.LoadScene("Stage");
    }
}
