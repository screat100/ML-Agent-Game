using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource[] BGMList;
    AudioSource[] SFXList;

    private void Start() 
    {
        BGMList = GameObject.Find("BGMs").GetComponentsInChildren<AudioSource>();
        SFXList = GameObject.Find("SFXs").GetComponentsInChildren<AudioSource>();

        PlayAudio("MainMenuBGM");

        DontDestroyOnLoad(gameObject);
    }

    // play manager
    public void PlayAudio(string name)
    {
        for(int i=0; i<BGMList.Length; i++) 
        {
            if(BGMList[i].transform.name == name)
            {
                BGMList[i].Play();
                return;
            }
        }

        for(int i=0; i<SFXList.Length; i++) 
        {
            if(SFXList[i].transform.name == name)
            {
                SFXList[i].Play();
                return;
            }
        }
    }
    public void PlayAudio(string name, Vector3 point)
    {
        for (int i = 0; i < BGMList.Length; i++)
        {
            if (BGMList[i].transform.name == name)
            {
                AudioSource.PlayClipAtPoint(BGMList[i].clip, point);
                return;
            }
        }

        for (int i = 0; i < SFXList.Length; i++)
        {
            if (SFXList[i].transform.name == name)
            {
                AudioSource.PlayClipAtPoint(SFXList[i].clip, point);
                return;
            }
        }
    }
    public void PauseAudio(string name)
    {
        for(int i=0; i<BGMList.Length; i++) 
        {
            if(BGMList[i].transform.name == name)
            {
                BGMList[i].Pause();
                return;
            }
        }

        for(int i=0; i<SFXList.Length; i++) 
        {
            if(SFXList[i].transform.name == name)
            {
                SFXList[i].Pause();
                return;
            }
        }
    }

    // Setting
    public void ControllBGMVolume()
    {
        for(int i=0; i<BGMList.Length; i++)
        {
            BGMList[i].volume = GameManager.bgmVolume;
        }
    }
    public void ControllSFXVolume()
    {
        for(int i=0; i<SFXList.Length; i++)
        {
            SFXList[i].volume = GameManager.sfxVolume;
        }
    }



}
