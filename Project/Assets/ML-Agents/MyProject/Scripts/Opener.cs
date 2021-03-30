using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opener : MonoBehaviour
{
    [SerializeField]
    AreaSetting m_areaSetting;

    public bool isOpen;

    private void Awake()
    {
        isOpen = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "agent" && !isOpen)
        {
            m_areaSetting.DoorOpen();
            isOpen = true;
        }

    }

}
