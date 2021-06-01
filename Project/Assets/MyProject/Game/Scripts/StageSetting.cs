using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class StageSetting : MonoBehaviour
{
    /* ===      Sub Classes     ===  */

    public class PoliceInfo
    {
        public PoliceAgent agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }

    public class ThiefInfo
    {
        public ruby_runner agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }

    public enum TrainBrain
    {
        DetectGoalBrain,
        DetectRubyBrain,
        RunBrain,
        TotalBrain
    }



    /* ===      Variables     ===  */

    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 30000; // 50 (per 1 second)

    public GameObject[] policeAgents;
    public GameObject[] thiefAgents;

    // 각 팀의 유닛들(에이전트+플레이어)을 리스트로 저장, 씬 리셋 시 활용
    public List<PoliceInfo> policeList;             
    public List<ThiefInfo> runnerList;              

    // 에이전트 그룹 => 그룹 보상으로 학습
    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    public List<GameObject> Sectors;
    public List<GameObject> Doorlist;
    public rubytrigger rubyTrigger;

    public float agentRunSpeed = 30f;
    public float runnerDetectRadius;                // 러너가 골을 감지할수있는 거리.

    [HideInInspector] public int m_ResetTimer;
    [HideInInspector] public int willCatchNum;      // Chaser가 잡을수있는 러너 수

    //bool rubygoal = false;                          // 루비를 든 러너 탈출 확인
    int escapenum = 0;                              // 탈출 성공한 러너 수

    [System.NonSerialized] public int goalIndex;    // 현재 goal의 index
    [System.NonSerialized] public int key_player;   // 현재 runnerlist중 ruby를 가진 멤버의 인덱스
    [System.NonSerialized] public bool findruby;    // 루비를 러너가 먹었을 시 활성화.
    [System.NonSerialized] public bool DetectGoal;  // 러너들이 골의 위치를 봤는가?

    public TrainBrain train; // thief agent의 brain

    GameObject[] visitCoinList;                     // police agent가 방문해야 하는 trigger list
    int coinNum;                                    // 방문할 때마다 +1
    [HideInInspector] public int catchedRunnerNum;  // 잡힌 도둑의 수



    /* ===      Base Functions     ===  */

    void Start()
    {

        chaserGroup = new SimpleMultiAgentGroup();
        runnerGroup = new SimpleMultiAgentGroup();

        /* ===      Find Police Agents and Register to Team Agent Group     ===  */

        for (int i = 0; i < policeAgents.Length; i++)
        {
            GameObject agent = policeAgents[i];

            PoliceInfo agentInfo = new PoliceInfo();
            agentInfo.agent = agent.GetComponent<PoliceAgent>();
            agentInfo.startPos = agent.transform.localPosition;
            agentInfo.startRot = agent.transform.rotation;
            agentInfo.rb = agent.GetComponent<Rigidbody>();

            policeList.Add(agentInfo);
        }

        foreach (var item in policeList)
        {
            item.startPos = item.agent.transform.localPosition;
            item.startRot = item.agent.transform.localRotation;
            item.rb = item.agent.GetComponent<Rigidbody>();
            chaserGroup.RegisterAgent(item.agent);
        }



        /* ===      Find Thief Agents and Register to Team Agent Group     ===  */

        for (int i = 0; i < thiefAgents.Length; i++)
        {
            GameObject agent = thiefAgents[i];

            ThiefInfo agentInfo = new ThiefInfo();
            agentInfo.agent = agent.GetComponent<ruby_runner>();
            agentInfo.startPos = agent.transform.localPosition;
            agentInfo.startRot = agent.transform.rotation;
            agentInfo.rb = agent.GetComponent<Rigidbody>();

            runnerList.Add(agentInfo);
        }

        foreach (var item in runnerList)
        {
            item.startPos = item.agent.transform.localPosition;
            item.startRot = item.agent.transform.localRotation;
            item.rb = item.agent.GetComponent<Rigidbody>();
            runnerGroup.RegisterAgent(item.agent);
        }

        /* ===      Initiate variables for police team    ===  */

        visitCoinList = GameObject.FindGameObjectsWithTag("areaDetector");
        coinNum = 0;
        catchedRunnerNum = 0;

        /* ===      Set Default Environment     ===  */
        m_ResetTimer = 0;
        willCatchNum = runnerList.Count;
        ResetScene();
    }

    void FixedUpdate()
    {
        m_ResetTimer++;

        //Thief 중 한 명이 RaySensor를 통해 Goal(탈출구)를 발견하면 CapsuleCollider를 true로 만듦
        if (gameObject.GetComponent<CapsuleCollider>().isTrigger == true && !DetectGoal)
        {
            runnerGroup.AddGroupReward(1f);
            DetectGoal = true;
        }


        /* ===      Thief : 현재 사용 중인 brain에 따라 reward를 다르게 받는다.     ===  */
        if (train == TrainBrain.RunBrain)
        {
            runnerGroup.AddGroupReward(1f / MaxEnvironmentSteps);
        }

        else if (train == TrainBrain.DetectGoalBrain || train == TrainBrain.DetectRubyBrain)
        {
            runnerGroup.AddGroupReward(-1f / MaxEnvironmentSteps);
        }


        // Time Over
        if (m_ResetTimer > MaxEnvironmentSteps)
        {
            chaserGroup.AddGroupReward(-willCatchNum);

            if (train == TrainBrain.RunBrain)
                runnerGroup.AddGroupReward(1f);

            runnerGroup.GroupEpisodeInterrupted();
            chaserGroup.GroupEpisodeInterrupted();

            ResetScene();
        }

    }



    /* ===      Externally executed functions      ===  */

    public void ResetAgentsNum()
    {
        // police
        for(int i=0; i<3; i++)
        {
            if(i <GameManager.PoliceNum)
            {
                policeAgents[i].SetActive(true);
            }
            else 
            {
                policeAgents[i].SetActive(false);
            }
        }

        // thief
        for(int i=0; i<8; i++) 
        { 
            if(i <GameManager.ThiefNum)
            {
                thiefAgents[i].SetActive(true);
            }
            else 
            {
                thiefAgents[i].SetActive(false);
            }

        }
    }


    // round 종료 후 scene을 초기화
    void ResetScene()
    {
        foreach (var item in policeList)
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
            item.agent.hasruby = false;
            item.agent.ruby.SetActive(false);
            item.agent.m_navagent.ResetPath();
        }


        m_ResetTimer = 0;
        willCatchNum = runnerList.Count;
        //rubygoal = false;

        DetectGoal = false;
        gameObject.GetComponent<CapsuleCollider>().isTrigger = false;
        escapenum = 0;

        Doorlist[goalIndex].GetComponent<ruby_goal>().Goal_reset();

        if (train == TrainBrain.DetectGoalBrain)
        {
            random_goal();
        }

        else if (train == TrainBrain.DetectRubyBrain)
        {
            RandomPos_ruby(); //루비 위치 랜덤으로 활성화
        }

        else if (train == TrainBrain.TotalBrain)
        {
            RandomPos_ruby();
        }



        // re-activate visit-coin
        for (int i = 0; i < visitCoinList.Length; i++)
        {
            visitCoinList[i].SetActive(true);
        }
        catchedRunnerNum = 0;
        coinNum = 0;
    }

    // 루비를 얻으면 thief에게 보상을 주는 함수 (외부에서 실행)
    public void Reward_RubyGet()
    {
        runnerGroup.AddGroupReward(1.0f);

        if (train == TrainBrain.DetectRubyBrain)
        {
            runnerGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }


    // 루비를 얻으면 thief에게 보상을 주는 함수 (외부에서 실행)
    public void Reward_Get(float index)
    {
        runnerGroup.AddGroupReward(index);
    }

    // goal에 도달했을 때 (=탈출 성공) 실행하는 함수
    // * m_agent : ruby를 가진 agent인지 확인하기 위한 parameter
    public void Scored(GameObject m_agent)
    {
        bool flag = m_agent.GetComponent<ruby_runner>().hasruby;

        escapenum++;

        if (flag)
        {
            //rubygoal = true;
            runnerGroup.AddGroupReward(0.5f);
            runnerGroup.GroupEpisodeInterrupted();

            if (train != TrainBrain.DetectGoalBrain)
                chaserGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

    }

    // 스크립트에 삽입된 Sectors 중, 랜덤으로 ruby 위치 선정
    public void RandomPos_ruby()
    {
        int Randomindex = Random.Range(0, Sectors.Count);
        rubyTrigger.resetPlace(Randomindex);
    }

    // 스크립트에 삽입된 door들중, 랜덤으로 Goal로 활성화
    public void random_goal()
    {
        goalIndex = Random.Range(0, Doorlist.Count);
        Doorlist[goalIndex].GetComponent<ruby_goal>().select_finishGoal();
    }


    // Runner가 잡혔을 때 실행
    public void RunnerIsCatched()
    {
        catchedRunnerNum++;
        willCatchNum--;

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

    // Police Team Agent가 새 영역에 방문했을 때 실행
    public void NewAreaVisitReward()
    {
        chaserGroup.AddGroupReward(1.0f / visitCoinList.Length);
        coinNum++;
    }
}
