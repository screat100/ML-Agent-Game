using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSetting : MonoBehaviour
{
    /* ===      Sub Classes     ===  */


    public enum TrainBrain
    {
        DetectGoalBrain,
        DetectRubyBrain,
        RunBrain,
        TotalBrain
    }



    /* ===      Variables     ===  */

    // For Managing Agent
    public GameObject[] policeAgents;
    public GameObject[] thiefAgents;      

    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    public List<GameObject> Sectors;
    public List<GameObject> Doorlist;
    public rubytrigger rubyTrigger;

    public float agentRunSpeed = 30f;
    public float runnerDetectRadius;                // 러너가 골을 감지할수있는 거리.

    [HideInInspector] public int willCatchNum;      // Chaser가 잡을수있는 러너 수

    int escapenum = 0;                              // 탈출 성공한 러너 수

    public TrainBrain train; // thief agent의 brain

    // For Maniging Stage
    [HideInInspector] public float timer;
    public float maxPlayTime = 300;
    public float maxLoadingTime = 3f;
    public float maxPolicesWatingTime = 7f;


    [System.NonSerialized] public int goalIndex;    // 현재 goal의 index
    [System.NonSerialized] public int key_player;   // 현재 runnerlist중 ruby를 가진 멤버의 인덱스
    [System.NonSerialized] public bool findruby;    // 루비를 러너가 먹었을 시 활성화.
    [System.NonSerialized] public bool DetectGoal;  // 러너들이 골의 위치를 봤는가?

    GameObject[] visitCoinList;                     // police agent가 방문해야 하는 trigger list
    int coinNum;                                    // 방문할 때마다 +1
    [HideInInspector] public int catchedRunnerNum;  // 잡힌 도둑의 수

    // Player
    public Player m_Player;


    // UI
    Text watingTime;
    Text playingTime;




    /* ===      Base Functions     ===  */

    void Start()
    {
        GameManager.phase = GameManager.Phase.waitLoading;

        /* ===      Initiate variables for police team    ===  */
        visitCoinList = GameObject.FindGameObjectsWithTag("areaDetector");
        coinNum = 0;
        catchedRunnerNum = 0;

        /* ===      Set Default Environment     ===  */
        timer = 0;
        willCatchNum = GameManager.thiefNum;
        ResetScene();
    }
    public void ResetScene()
    {
        chaserGroup = new SimpleMultiAgentGroup();
        runnerGroup = new SimpleMultiAgentGroup();

        // reset each agents group
        for(int i=0; i<3; i++)
        {
            if(GameManager.playersTeam == Player.Team.police)
            {
                if(i <GameManager.policeNum-1)
                {
                    policeAgents[i].SetActive(false);
                    policeAgents[i].SetActive(true);
                    chaserGroup.RegisterAgent(policeAgents[i].GetComponent<PoliceAgent>());
                }
                else 
                {
                    policeAgents[i].SetActive(false);
                }
            }

            else 
            {
                if(i <GameManager.policeNum)
                {
                    policeAgents[i].SetActive(false);
                    policeAgents[i].SetActive(true);
                    chaserGroup.RegisterAgent(policeAgents[i].GetComponent<PoliceAgent>());
                }
                else 
                {
                    policeAgents[i].SetActive(false);
                }
            }
        }

        for(int i=0; i<6; i++) 
        { 
            if(GameManager.playersTeam == Player.Team.thief)
            {
                if(i <GameManager.thiefNum -1)
                {
                    thiefAgents[i].SetActive(false);
                    thiefAgents[i].SetActive(true);
                    chaserGroup.RegisterAgent(policeAgents[i].GetComponent<TheifAgent>());
                }
                else 
                {
                    thiefAgents[i].SetActive(false);
                }
            }

            else 
            {
                if(i <GameManager.thiefNum)
                {
                    thiefAgents[i].SetActive(false);
                    thiefAgents[i].SetActive(true);
                    chaserGroup.RegisterAgent(policeAgents[i].GetComponent<TheifAgent>());
                }
                else 
                {
                    thiefAgents[i].SetActive(false);
                }
            }
        }

        // Initiate variables for police team 
        visitCoinList = GameObject.FindGameObjectsWithTag("areaDetector");
        coinNum = 0;
        catchedRunnerNum = 0;

        // Set Default Environment  
        willCatchNum = GameManager.thiefNum;

        
        findruby = false;
        DetectGoal = false;
        gameObject.GetComponent<CapsuleCollider>().isTrigger = false;
        escapenum = 0;

        RandomPos_ruby();
    }

    private void Update() 
    {
        ManageStageTime();

        // Thief 중 한 명이 RaySensor를 통해 Goal(탈출구)를 발견하면 CapsuleCollider를 true로 만듦
        if (gameObject.GetComponent<CapsuleCollider>().isTrigger == true && !DetectGoal)
        {
            runnerGroup.AddGroupReward(1f);
            DetectGoal = true;
        }

    }


    void ManageStageTime()
    {
        timer += Time.deltaTime;

        if(GameManager.phase == GameManager.Phase.waitLoading
        && timer >= maxLoadingTime)
        {
            timer = 0f;
            GameManager.phase = GameManager.Phase.policesWating;

            if(GameManager.playersTeam == Player.Team.thief)
            {
                m_Player.ActivatePlayersControll();
            }
        }

        else if(GameManager.phase == GameManager.Phase.policesWating
        && timer >= maxPolicesWatingTime)
        {
            timer = 0f;
            GameManager.phase = GameManager.Phase.play;
        }
        
        else if(GameManager.phase == GameManager.Phase.play
        && timer >= maxPlayTime)
        {
            timer = 0f;
            
            //POLICE TEAM WIN! (add plz)

            runnerGroup.GroupEpisodeInterrupted();
            chaserGroup.GroupEpisodeInterrupted();

            GameManager.phase = GameManager.Phase.play;
        }

    }



    /* ===      Externally executed functions      ===  */

    // 매치를 처음 시작할 때 한 번만 실행



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
    public void Reward_Get(float reward)
    {
        runnerGroup.AddGroupReward(reward);
    }

    // goal에 도달했을 때 (=탈출 성공) 실행하는 함수
    // * m_agent : ruby를 가진 agent인지 확인하기 위한 parameter
    public void ScoredThief(GameObject m_agent)
    {
        bool flag = m_agent.GetComponent<ruby_runner>().hasruby;

        escapenum++;

        if (flag)
        {
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
    public void GetReward_police()
    {
        catchedRunnerNum++;
        willCatchNum--;

        chaserGroup.AddGroupReward(2.0f / GameManager.thiefNum);
        runnerGroup.AddGroupReward(-2.0f / GameManager.thiefNum);

        // All runners are catched => chaser win!
        if (catchedRunnerNum >= GameManager.thiefNum)
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
