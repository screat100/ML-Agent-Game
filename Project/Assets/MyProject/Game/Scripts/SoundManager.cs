using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audio_MainMenuBGM;
    public AudioSource audio_IngameBGM;
    public AudioSource audio_ResultBGM_win;
    public AudioSource audio_ResultBGM_lose;
    
    public AudioSource audio_ButtonClick;

    public void PlayButtonClickSound()
    {
        audio_ButtonClick.Play();
    }



}
