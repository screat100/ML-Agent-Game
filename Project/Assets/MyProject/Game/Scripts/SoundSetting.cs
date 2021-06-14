using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSetting : MonoBehaviour
{
    public void ChangeBGMVolume()
    {
        // apply
        Slider bgmSlider = GameObject.Find("slider_bgm").GetComponent<Slider>();
        GameManager.instance.bgmVolume = bgmSlider.value;

        // setting ui
        Text text_bgm = GameObject.Find("text_bgm").GetComponent<Text>();
        int bgmVolume = (int)(GameManager.instance.bgmVolume*100);
        text_bgm.text = bgmVolume.ToString() + "%";

        GameObject.Find("Sounds").GetComponent<SoundManager>().ControllBGMVolume();
    }

    public void ChangeSFXVolume()
    {
        // apply
        Slider sfxSlider = GameObject.Find("slider_sfx").GetComponent<Slider>();
        GameManager.instance.sfxVolume = sfxSlider.value;

        // setting ui 
        Text text_sfx = GameObject.Find("text_sfx").GetComponent<Text>();
        int sfxVolume = (int)(GameManager.instance.sfxVolume*100);
        text_sfx.text = sfxVolume.ToString() + "%";

        GameObject.Find("Sounds").GetComponent<SoundManager>().ControllSFXVolume();
    }

    public void PlaySound(string name)
    {
        GameObject.Find("Sounds").GetComponent<SoundManager>().PlayAudio(name);
    }
}
