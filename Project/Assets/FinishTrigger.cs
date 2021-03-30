using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    [SerializeField]
    AreaSetting m_areaSetting;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "agent")
        {
            m_areaSetting.Scored();
        }
    }
}
