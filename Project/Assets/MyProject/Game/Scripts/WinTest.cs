using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTest : MonoBehaviour
{
    public void WinPolice()
    {
        GameObject.Find("GameArea").GetComponent<StageSetting>().EndGame(true);
    }
    public void WinThief()
    {
        GameObject.Find("GameArea").GetComponent<StageSetting>().EndGame(false);
    }
}
