using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class CvR3_Area : MonoBehaviour
{
    [System.Serializable]
    public class AgentInfo
    {
        public CvR3_Agent agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }




    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 1500; // 50 (per 1 second)

    public List<AgentInfo> chaserList;
    public List<AgentInfo> runnerList;

    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    public float agentRunSpeed = 30f;
    [HideInInspector]
    public int m_ResetTimer;
    [HideInInspector]
    public int catchedRunnerNum;


    // Start is called before the first frame update
    void Start()
    {
        chaserGroup = new SimpleMultiAgentGroup();
        runnerGroup = new SimpleMultiAgentGroup();

        foreach (var item in chaserList)
        {
            item.startPos = item.agent.transform.localPosition;
            item.startRot = item.agent.transform.localRotation;
            item.rb = item.agent.GetComponent<Rigidbody>();
            chaserGroup.RegisterAgent(item.agent);
        }
        foreach (var item in runnerList)
        {
            item.startPos = item.agent.transform.localPosition;
            item.startRot = item.agent.transform.localRotation;
            item.rb = item.agent.GetComponent<Rigidbody>();
            runnerGroup.RegisterAgent(item.agent);
        }

        m_ResetTimer = 0;
        catchedRunnerNum = 0;
        ResetScene();
    }

    private void FixedUpdate()
    {
        m_ResetTimer++;

        // bind chaser until 10 sec from start
        if(m_ResetTimer < 500)
        {
            for (int i = 0; i< chaserList.Count; i++) {
                chaserList[i].agent.transform.localPosition = chaserList[i].startPos;
            }
        }

        // Time Over => Runner win!
        if (m_ResetTimer > MaxEnvironmentSteps)
        {
            runnerGroup.AddGroupReward(2.0f);
            chaserGroup.AddGroupReward(-2.0f);
            runnerGroup.GroupEpisodeInterrupted();
            chaserGroup.GroupEpisodeInterrupted();
            ResetScene();
        }


    }


    // Runner가 잡힐 때마다 chaser 보상받음
    // 반대로 runner는 처벌
    public void RunnerIsCatched()
    {
        catchedRunnerNum++;
        chaserGroup.AddGroupReward(1.0f / runnerList.Count);
        runnerGroup.AddGroupReward(-1.0f / runnerList.Count);

        // All runners are catched => chaser win!
        if (catchedRunnerNum >= runnerList.Count)
        {
            runnerGroup.GroupEpisodeInterrupted();
            chaserGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

    }





    void ResetScene()
    {
        foreach (var item in chaserList)
        {
            item.agent.transform.localPosition = item.startPos;
            item.agent.transform.localRotation = item.startRot;
            item.rb.velocity = Vector3.zero;
            item.rb.angularVelocity = Vector3.zero;
        }

        foreach (var item in runnerList)
        {
            if (!item.agent.gameObject.activeInHierarchy)
            {
                item.agent.gameObject.SetActive(true);
                runnerGroup.RegisterAgent(item.agent);
            }

            item.agent.transform.localPosition = item.startPos;
            item.agent.transform.localRotation = item.startRot;
            item.rb.velocity = Vector3.zero;
            item.rb.angularVelocity = Vector3.zero;
        }

        m_ResetTimer = 0;
        catchedRunnerNum = 0;
    }


}
