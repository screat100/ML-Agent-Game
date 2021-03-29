using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opener : MonoBehaviour
{
    [SerializeField]
    AreaSetting m_areaSetting;

    public bool isOpen;

    int Remain_Member;
    
    private void Awake()
    {
        isOpen = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "agent")
        {
            Remain_Member++;
            if(Remain_Member>=2&&!isOpen)
            {
                m_areaSetting.DoorOpen();
                isOpen = true;
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "agent")
        {
            Remain_Member--;
            if(Remain_Member<2 && isOpen)
            {
                m_areaSetting.DoorClose();
                isOpen = false;
            }
        }
    }

}
