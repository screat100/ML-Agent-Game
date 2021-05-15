using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class CVR5_Area : MonoBehaviour
{
    [System.Serializable]
    public class AgentInfo
    {
        public CVR5_Agent agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }

    List<AgentInfo> chaserList;
    List<AgentInfo> runnerList;

    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;


    [Header("Max Environment Steps")] public int MaxEnvironmentSteps; // 50 (per 1 second)


    //[Header("Variables related visiting information")]
    //public int AreaNum;
    //public List<bool> visited;

    //GameObject[] VisitCoinList;
    //int CoinNum;


    [Header("Default Agent Setting")]
    public float agentRunSpeed = 10f;
    [HideInInspector]
    public int m_ResetTimer;
    [HideInInspector]
    public int catchedRunnerNum;

    [Header("Runner Random Spawn Position")]
    public GameObject Leftup;
    public GameObject Rightdown;

    void Start()
    {
        chaserGroup = new SimpleMultiAgentGroup();
        runnerGroup = new SimpleMultiAgentGroup();

        chaserList = new List<AgentInfo>();
        runnerList = new List<AgentInfo>();

        //visited = new List<bool>();
        //for(int i = 0; i<AreaNum; i++)
        //{
        //    visited.Add(false);
        //}


        //VisitCoinList = GameObject.FindGameObjectsWithTag("areaDetector");
        //CoinNum = 0;


        /*
         * Add Police agent
         */
        GameObject[] policeAgent = GameObject.FindGameObjectsWithTag("police");

        for (int i = 0; i < policeAgent.Length; i++)
        {
            GameObject agent = policeAgent[i];

            AgentInfo agentInfo = new AgentInfo();
            agentInfo.agent = agent.GetComponent<CVR5_Agent>();
            agentInfo.startPos = agent.transform.localPosition;
            agentInfo.startRot = agent.transform.rotation;
            agentInfo.rb = agent.GetComponent<Rigidbody>();

            chaserList.Add(agentInfo);
        }

        /*
         * Add Thief agent
         */
        GameObject[] thiefAgent = GameObject.FindGameObjectsWithTag("thief");
        for (int i = 0; i < thiefAgent.Length; i++)
        {
            GameObject agent = thiefAgent[i];

            AgentInfo agentInfo = new AgentInfo();
            agentInfo.agent = agent.GetComponent<CVR5_Agent>();
            agentInfo.startPos = agent.transform.localPosition;
            agentInfo.startRot = agent.transform.rotation;
            agentInfo.rb = agent.GetComponent<Rigidbody>();

            runnerList.Add(agentInfo);
        }

        // register agent to team-group
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

        // time penalty & advantage
        chaserGroup.AddGroupReward(-2.0f / MaxEnvironmentSteps);
        runnerGroup.AddGroupReward(2.0f / MaxEnvironmentSteps);

        // Time Over => Runner win!
        if (m_ResetTimer > MaxEnvironmentSteps)
        {
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
        chaserGroup.AddGroupReward(2.0f / runnerList.Count);
        runnerGroup.AddGroupReward(-2.0f / runnerList.Count);

        // All runners are catched => chaser win!
        if (catchedRunnerNum >= runnerList.Count)
        {
            runnerGroup.GroupEpisodeInterrupted();
            chaserGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

    }

    // pre-train!
    //public void NewAreaVisitReward()
    //{
    //    chaserGroup.AddGroupReward(1.0f / VisitCoinList.Length);
    //    CoinNum++;

    //    //if(CoinNum == VisitCoinList.Length)
    //    //{
    //    //    runnerGroup.GroupEpisodeInterrupted();
    //    //    ResetScene();
    //    //}
    //}

    public void DestinationReward()
    {
        chaserGroup.AddGroupReward(0.05f);
    }



    void ResetScene()
    {
        foreach (var item in chaserList)
        {
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

            item.agent.transform.localPosition = new Vector3(
                Random.Range(Leftup.transform.localPosition.x, Rightdown.transform.localPosition.x),
                0,
                Random.Range(Leftup.transform.localPosition.z, Rightdown.transform.localPosition.z));
            item.agent.transform.localRotation = item.startRot;
            item.rb.velocity = Vector3.zero;
            item.rb.angularVelocity = Vector3.zero;
        }

        //// re-activate visit-coin
        //for(int i=0; i<VisitCoinList.Length; i++) 
        //{
        //    VisitCoinList[i].SetActive(true);
        //}
        //CoinNum = 0;

        m_ResetTimer = 0;
        catchedRunnerNum = 0;
    }


}
