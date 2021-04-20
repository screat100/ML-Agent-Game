using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ruby_goal : MonoBehaviour
{
    [SerializeField]
    ruby_AreaSetting m_areaSetting;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "runner")
        {
            other.gameObject.SetActive(false);
            m_areaSetting.Scored(other.gameObject, false); 
        }
    }
}
