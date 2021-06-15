using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Result : MonoBehaviour
{

    public GameObject UI_roundResult;
    public GameObject UI_finalResult;

    public GameObject image_win;
    public GameObject image_lose;

    SoundManager soundManager;

    void Start()
    {
        soundManager = GameObject.Find("Sounds").GetComponent<SoundManager>();

    
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        // ========= final result ===============
        if(GameManager.instance.winNum_Police >=2 || GameManager.instance.winNum_Thief >=2)
        {
            UI_roundResult.SetActive(false);
            UI_finalResult.SetActive(true);

            // win
            if(GameManager.instance.recentWinner == GameManager.instance.playersTeam)
            {
                GameObject.Find("image_win").SetActive(true);
                GameObject.Find("image_lose").SetActive(false);
                soundManager.PlayAudio("Win");
            }

            // lose
            else
            {
                GameObject.Find("image_win").SetActive(false);
                GameObject.Find("image_lose").SetActive(true);
                soundManager.PlayAudio("Lose");
            }

            // score
            GameObject.Find("text_police").GetComponent<Text>().text = GameManager.instance.winNum_Police.ToString();
            GameObject.Find("text_thief").GetComponent<Text>().text = GameManager.instance.winNum_Thief.ToString();

            // record
            if(GameManager.instance.playersTeam == Player.Team.police)
            {
                GameObject.Find("text_record").GetComponent<Text>().text = "내가 잡은 도둑 수 : " + GameManager.instance.policePlayerRecord.ToString();
            }
            else
            {
                float time = GameManager.instance.thiefPlayerRecord / 3f;

                int min = ((int)time)/60;
                int sec = ((int)time) - 60*min;
                int milsec = (int)((time - (float)((int)time))*100);

                GameObject.Find("text_record").GetComponent<Text>().text = "소요 시간 : " + min.ToString() + "분 " + sec.ToString() + "초 " + milsec.ToString();
            }

        }

        // ========= round result ===============
        else
        {
            UI_roundResult.SetActive(true);
            UI_finalResult.SetActive(false);

            // n-round text
            Text roundText = GameObject.Find("text_round").GetComponent<Text>();
            switch(GameManager.instance.round)
            {
                case 1:
                    roundText.text = "1st Round";
                    break;

                case 2:
                    roundText.text = "2nd Round";
                    break;
            }


            // win
            if(GameManager.instance.recentWinner == GameManager.instance.playersTeam)
            {
                GameObject.Find("text_winorlose").GetComponent<Text>().text = "WIN";
                soundManager.PlayAudio("Win");
            }

            // lose
            else
            {
                GameObject.Find("text_winorlose").GetComponent<Text>().text = "LOSE";
                soundManager.PlayAudio("Lose");
            }

            // score
            GameObject.Find("text_police").GetComponent<Text>().text = GameManager.instance.winNum_Police.ToString();
            GameObject.Find("text_thief").GetComponent<Text>().text = GameManager.instance.winNum_Thief.ToString();
        }
    }

    
    /*      ===     button      ===         */
    public void GoToNextRound()
    {
        SceneManager.LoadScene("Stage");
    }


    public void GoToMainMenu()
    {
        GameManager.instance.round = 0;
        GameManager.instance.winNum_Thief = 0;
        GameManager.instance.winNum_Police = 0;
        GameManager.instance.policePlayerRecord = 0;
        GameManager.instance.thiefPlayerRecord = 0;
        GameManager.instance.phase = GameManager.Phase.selectTeam;

        Destroy(soundManager.gameObject);
        SceneManager.LoadScene("MainMenu");
    }

}
