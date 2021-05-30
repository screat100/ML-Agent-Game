using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class AreaSetting : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public MyAgent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }


    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 1500;

    // Cache in Inspector
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    // Cache in Inspector
    public List<GameObject> DoorList;
    int OpenDoorIndex;

    public GameObject Opener;




    private SimpleMultiAgentGroup m_AgentGroup;

    private int m_ResetTimer;

    public float agentRunSpeed;

    void Start()
    {
        m_AgentGroup = new SimpleMultiAgentGroup();
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.localPosition;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.transform.GetComponent<Rigidbody>();
            m_AgentGroup.RegisterAgent(item.Agent);
        }
        ResetScene();
    }

    // 시간에 따른 패널티를 부여한다.
    private void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_AgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        // Hurry up penalty
        m_AgentGroup.AddGroupReward(-4.5f / MaxEnvironmentSteps);
    }


    // 문 밖으로 나가면 2의 보상을 받고 에피소드를 종료한다. 
    public void Scored()
    {
        m_AgentGroup.AddGroupReward(2f);
        m_AgentGroup.EndGroupEpisode();
        ResetScene();
    }

    // 최초로 문을 열면 1의 보상을 주고, 문을 연다.
    public void DoorOpen()
    {
        DoorList[OpenDoorIndex].transform.Find("Door_left").transform.localRotation = Quaternion.Euler(0, -90, 0);
        DoorList[OpenDoorIndex].transform.Find("Door_right").transform.localRotation = Quaternion.Euler(0, 90, 0);

        m_AgentGroup.AddGroupReward(1f);
    }


    void ResetScene()
    {
        m_ResetTimer = 0;

        // reset agents
        foreach (var item in AgentsList)
        {
            item.Agent.transform.localPosition = item.StartingPos;
            item.Agent.transform.rotation = item.StartingRot;
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        // reset door & wall pos
        DoorList[OpenDoorIndex].transform.Find("Door_left").transform.localRotation = Quaternion.Euler(0, 0, 0);
        DoorList[OpenDoorIndex].transform.Find("Door_right").transform.localRotation = Quaternion.Euler(0, 0, 0);
        OpenDoorIndex = Random.Range(0, 4);


        // reset openner pos (0,0) ~ (15, 15)
        Vector3 opennerPos = new Vector3(-Random.Range(3.75f, 16f), 0.002f, Random.Range(3.75f, 16f));
        Opener.transform.localPosition = opennerPos;
        Opener.GetComponent<Opener>().isOpen = false;

    }


}
