using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class PoliceThief_Area : MonoBehaviour
{
    [System.Serializable]
    public class PoliceInfo
    {
        public police_Agent agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }

    [System.Serializable]
    public class ThiefInfo
    {
        public thief_Agent agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }



    [Header("Max Environment Steps")]
    public int MaxEnvironmentSteps = 3000; // 50 (per 1 second)

    public List<PoliceInfo> policeList;
    public List<ThiefInfo> thiefList;

    private SimpleMultiAgentGroup policeGroup;
    private SimpleMultiAgentGroup thiefGroup;

    public float agentRunSpeed = 30f;

    [HideInInspector]
    public int m_ResetTimer;
    [HideInInspector]
    public int catchedThiefNum;


    // Start is called before the first frame update
    void Start()
    {
        policeGroup = new SimpleMultiAgentGroup();
        thiefGroup = new SimpleMultiAgentGroup();

        foreach (var item in policeList)
        {
            item.startPos = item.agent.transform.position;
            item.startRot = item.agent.transform.rotation;
            item.rb = item.agent.GetComponent<Rigidbody>();
            policeGroup.RegisterAgent(item.agent);
        }
        foreach (var item in thiefList)
        {
            item.startPos = item.agent.transform.position;
            item.startRot = item.agent.transform.rotation;
            item.rb = item.agent.GetComponent<Rigidbody>();
            thiefGroup.RegisterAgent(item.agent);
        }

        m_ResetTimer = 0;
        catchedThiefNum = 0;
        ResetScene();
    }

    private void FixedUpdate()
    {
        m_ResetTimer++;

        if (m_ResetTimer > MaxEnvironmentSteps)
        {
            thiefGroup.AddGroupReward(thiefList.Count);
            thiefGroup.GroupEpisodeInterrupted();
            policeGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        policeGroup.AddGroupReward(-(float)thiefList.Count / MaxEnvironmentSteps);
    }

    public void ThiefIsCatched()
    {
        catchedThiefNum++;
        policeGroup.AddGroupReward(1.0f);
        thiefGroup.AddGroupReward(-1.0f);

        if (catchedThiefNum >= thiefList.Count)
        {
            policeGroup.AddGroupReward(1.0f * thiefList.Count);
            policeGroup.EndGroupEpisode();
            thiefGroup.EndGroupEpisode();
            ResetScene();
        }

    }

    void ResetScene()
    {
        foreach (var item in policeList)
        {
            item.agent.transform.position = item.startPos;
            item.agent.transform.rotation = item.startRot;
            item.rb.velocity = Vector3.zero;
            item.rb.angularVelocity = Vector3.zero;
        }

        foreach (var item in thiefList)
        {
            if (!item.agent.gameObject.activeInHierarchy)
            {
                item.agent.gameObject.SetActive(true);
                thiefGroup.RegisterAgent(item.agent);
            }

            item.agent.transform.position = item.startPos;
            item.agent.transform.rotation = item.startRot;
            item.rb.velocity = Vector3.zero;
            item.rb.angularVelocity = Vector3.zero;
        }

        m_ResetTimer = 0;
        catchedThiefNum = 0;
    }
}
