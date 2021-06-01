using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* ===      Variables       === */
    public GameObject ui_MainMenu;
    public GameObject ui_SelectTeam;
    public GameObject ui_playing;
    public GameObject ui_result;

    

    public float time;
    float maxPlayTime = 300;
    public bool timeFlow = false;


    /* ===      base functions       === */
    void Start()
    {
        //ui_MainMenu.SetActive(true);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        time = 0;
        
    }

    void Update()
    {
        if(timeFlow) 
        {
            time += Time.deltaTime;

            if(time >= maxPlayTime) 
            {
                Debug.Log("Time Over !");
            }
        }
    }

    
    /* ===      dd       === */
}
