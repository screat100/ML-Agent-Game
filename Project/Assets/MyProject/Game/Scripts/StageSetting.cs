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


    public TrainBrain train; // thief agent의 brain

    // For Maniging Stage
    [HideInInspector] public float timer;
    public float maxPlayTime = 300;
    public float maxLoadingTime = 3.5f;
    public float maxPolicesWatingTime = 7.5f;


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
    public Text text_watingTime;
    public Text text_playingTime;




    /* ===      Base Functions     ===  */

    void Start()
    {
        GameManager.instance.phase = GameManager.Phase.waitLoading;
        m_Player.transform.tag = GameManager.instance.playersTeam.ToString();

        GameManager.instance.round++;

        /* ===      Initiate variables for police team    ===  */
        visitCoinList = GameObject.FindGameObjectsWithTag("areaDetector");
        coinNum = 0;
        catchedRunnerNum = 0;

        /* ===      UI     ===  */
        text_watingTime.gameObject.SetActive(true);
        text_playingTime.gameObject.SetActive(false);

        /* ===      sound   === */
        GameObject.Find("Sounds").GetComponent<SoundManager>().PlayAudio("IngameBGM");
        GameObject.Find("Sounds").GetComponent<SoundManager>().PlayAudio("Countdown");

        /* ===      Set Default Environment     ===  */
        timer = 0;
        willCatchNum = GameManager.instance.thiefNum;
        ResetScene();

        
    }

    public void ResetScene()
    {
        chaserGroup = new SimpleMultiAgentGroup();
        runnerGroup = new SimpleMultiAgentGroup();

        // reset each agents group
        for(int i=0; i<3; i++)
        {
            if(GameManager.instance.playersTeam == Player.Team.police)
            {
                if(i <GameManager.instance.policeNum-1)
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
                if(i <GameManager.instance.policeNum)
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
            if(GameManager.instance.playersTeam == Player.Team.thief)
            {
                if(i <GameManager.instance.thiefNum -1)
                {
                    thiefAgents[i].SetActive(false);
                    thiefAgents[i].SetActive(true);
                    runnerGroup.RegisterAgent(thiefAgents[i].GetComponent<ThiefAgent>());
                }
                else 
                {
                    thiefAgents[i].SetActive(false);
                }
            }

            else 
            {
                if(i <GameManager.instance.thiefNum)
                {
                    thiefAgents[i].SetActive(false);
                    thiefAgents[i].SetActive(true);
                    runnerGroup.RegisterAgent(thiefAgents[i].GetComponent<ThiefAgent>());
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
        willCatchNum = GameManager.instance.thiefNum;

        
        findruby = false;
        DetectGoal = false;
        gameObject.GetComponent<CapsuleCollider>().isTrigger = false;

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

        // ============= UI ============= 

        switch(GameManager.instance.phase)
        {
        case GameManager.Phase.waitLoading:
            text_watingTime.text = ((int)(maxLoadingTime - timer)).ToString();
            break;

        case GameManager.Phase.policesWating:
            text_watingTime.text = ((int)(maxPolicesWatingTime - timer)).ToString();
            break;

        case GameManager.Phase.play :
            int min = (int)timer / 60;
            int sec = (int)timer - (min*60);
            string sec_text = (sec < 10) ? "0"+sec.ToString() : sec.ToString();
            text_playingTime.text = $"0{min.ToString()}:{sec_text}";
            break;
        }

        // change phase : loading -> thief move only
        if(GameManager.instance.phase == GameManager.Phase.waitLoading
        && timer >= maxLoadingTime)
        {
            timer = 0f;
            GameManager.instance.phase = GameManager.Phase.policesWating;

            if(GameManager.instance.playersTeam == Player.Team.thief)
            {
                m_Player.ActivatePlayersControll();
            }
        }

        // change phase : thief move only --> play
        else if(GameManager.instance.phase == GameManager.Phase.policesWating
        && timer >= maxPolicesWatingTime)
        {
            timer = 0f;
            GameManager.instance.phase = GameManager.Phase.play;
            
            text_watingTime.gameObject.SetActive(false);
            text_playingTime.gameObject.SetActive(true);

            m_Player.ActivatePlayersControll();
        }
        
        // change phase : time over -> police win! 
        else if(GameManager.instance.phase == GameManager.Phase.play
        && timer >= maxPlayTime)
        {
            timer = 0f;
            
            EndGame(true);

            runnerGroup.GroupEpisodeInterrupted();
            chaserGroup.GroupEpisodeInterrupted();

            GameManager.instance.phase = GameManager.Phase.play;
        }

    }

    /* ===                        finishing functions                          ===  */

    public void EndGame(bool isPoliceWinner)
    {
        // police win
        if(isPoliceWinner)
        {
            GameManager.instance.winNum_Police++;
            GameManager.instance.recentWinner = Player.Team.police;
            ChangePhaseToResult();
        }

        // thief win
        else
        {
            GameManager.instance.winNum_Thief++;
            GameManager.instance.recentWinner = Player.Team.thief;
            ChangePhaseToResult();
        }
    }

    public void ChangePhaseToResult()
    {
        m_Player.OnEditMode(false);
        SceneManager.LoadScene("Result");
    }

    /* ===                        Externally executed functions                          ===  */

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


    // thief 팀이 특정한 보상 조건을 달성한 상황일 때, thief 팀에게 group reward를 주는 함수
    public void Reward_Get(float reward)
    {
        runnerGroup.AddGroupReward(reward);
    }

    // goal에 도달했을 때 (=탈출 성공) 실행하는 함수
    // * m_agent : ruby를 가진 agent인지 확인하기 위한 parameter
    public void ScoredThief(GameObject m_agent)
    {
        bool flag = m_agent.GetComponent<ruby_runner>().hasruby;


        if (flag)
        {
            runnerGroup.AddGroupReward(0.5f);
            runnerGroup.GroupEpisodeInterrupted();

            if (train != TrainBrain.DetectGoalBrain) 
            {
                chaserGroup.GroupEpisodeInterrupted();
                EndGame(false);
            }

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
        
        chaserGroup.AddGroupReward(2.0f / GameManager.instance.thiefNum);
        runnerGroup.AddGroupReward(-2.0f / GameManager.instance.thiefNum);

        // All runners are catched => chaser win!
        if (catchedRunnerNum >= GameManager.instance.thiefNum)
        {
            runnerGroup.GroupEpisodeInterrupted();
            chaserGroup.GroupEpisodeInterrupted();
            ResetScene();
            EndGame(true);
        }

    }

    // Police Team Agent가 새 영역에 방문했을 때 실행
    public void NewAreaVisitReward()
    {
        chaserGroup.AddGroupReward(1.0f / visitCoinList.Length);
        coinNum++;
    }








}
