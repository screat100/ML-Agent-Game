    using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class ruby_AreaSetting : MonoBehaviour
{
    [System.Serializable]
    public class ChaserInfo
    {
        public ruby_chaser agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }

    [System.Serializable]
    public class RunnerInfo
    {
        public ruby_runner agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }




    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 30000; // 50 (per 1 second)

    public List<ChaserInfo> chaserList;
    public List<RunnerInfo> runnerList;

    public List<GameObject> Sectors;

    public List<GameObject> Doorlist;
    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    public float agentRunSpeed = 30f;
    public float RunnerDetectRadius; //러너가 골을 감지할수있는 거리.
    [HideInInspector]
    public int m_ResetTimer;
    [HideInInspector]
    public int willCatchNum; //Chaser가 잡을수있는 러너 수

    bool rubygoal;  //루비를 든 러너 탈출 확인
    int escapenum; //탈출 성공한 러너 수
    [System.NonSerialized]
    public int goalIndex; //현재 goal의 index
    [System.NonSerialized]
    public int key_player; //현재 runnerlist중 ruby를 가진 멤버의 인덱스

    [System.NonSerialized]
    public bool findruby; //루비를 러너가 먹었을 시 활성화.

    [System.NonSerialized]
    public bool DetectGoal; //러너들이 골의 위치를 봤는가?
    public rubytrigger ruby;

    public enum TrainBrain{
        DetectGoalBrain,
        DetectRubyBrain,
        RunBrain
    }
    public TrainBrain train;
    //루비만 찾는 브레인 학습

    // Start is called before the first frame update
    void Start()
    {
        chaserGroup = new SimpleMultiAgentGroup();
        runnerGroup = new SimpleMultiAgentGroup();

        foreach(var item in chaserList)
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
        willCatchNum = runnerList.Count;
        ResetScene();
    }

    private void FixedUpdate()
    {
        m_ResetTimer++;

        //골 태그를 raysensor 발견했다는것을 확인
        if(gameObject.GetComponent<CapsuleCollider>().isTrigger==true&&!DetectGoal)
        {
            runnerGroup.AddGroupReward(1f);
            DetectGoal=true;
        }

        if(TrainBrain.RunBrain==train){
            runnerGroup.AddGroupReward(1f/MaxEnvironmentSteps);
        }
        else if(TrainBrain.DetectGoalBrain==train||TrainBrain.DetectRubyBrain==train){
             runnerGroup.AddGroupReward(-1f/MaxEnvironmentSteps);
        }

        if(m_ResetTimer > MaxEnvironmentSteps)
        {
            chaserGroup.AddGroupReward(-willCatchNum);
            //runnerGroup.AddGroupReward(willCatchNum);
            runnerGroup.GroupEpisodeInterrupted();
            if(train!=TrainBrain.DetectGoalBrain&&train!=TrainBrain.DetectRubyBrain)
                chaserGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

    }

    public void Reward_RubyGet(){
        runnerGroup.AddGroupReward(1.0f);
        if(TrainBrain.DetectRubyBrain==train){
            runnerGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
        
    }
    public void Reward_Get(float index){
        runnerGroup.AddGroupReward(index);
    }

   public void Scored(GameObject m_agent,bool iscaught)
    {
        bool flag=m_agent.GetComponent<ruby_runner>().hasruby;

        //잡혔을때
        if(iscaught){
            willCatchNum--;
            chaserGroup.AddGroupReward(1.0f);

            //도망자 브레인을 학습할때는 모두가 잡혀야 끝
            if(TrainBrain.RunBrain==train){
                runnerGroup.AddGroupReward(-1f/runnerList.Count);
            }
            else{
                if(flag)
                {
                    runnerGroup.AddGroupReward(-0.5f);
                    runnerGroup.GroupEpisodeInterrupted();
                    chaserGroup.GroupEpisodeInterrupted();
                    ResetScene();
                }
                else{
                    runnerGroup.AddGroupReward(-0.2f);
                }
            }
        }
        //탈출했을때
        else{
            willCatchNum--;
            escapenum++;
            if(flag)
            {
                rubygoal=true;
                runnerGroup.AddGroupReward(0.5f);
                runnerGroup.GroupEpisodeInterrupted();
                if(train!=TrainBrain.DetectGoalBrain)
                    chaserGroup.GroupEpisodeInterrupted();
                ResetScene();
            }
        }

        
         if (willCatchNum <=0)
            {
                if(rubygoal){
                    runnerGroup.AddGroupReward(escapenum);
                }

                runnerGroup.GroupEpisodeInterrupted();
                chaserGroup.GroupEpisodeInterrupted();
                ResetScene();
            }
    }

    void ResetScene()
    {
        foreach(var item in chaserList)
        {
            item.agent.transform.localPosition = item.startPos;
            item.agent.transform.localRotation = item.startRot;
            item.rb.velocity = Vector3.zero;
            item.rb.angularVelocity = Vector3.zero;
        }

        foreach (var item in runnerList)
        {
            if(!item.agent.gameObject.activeInHierarchy)
            {
                item.agent.gameObject.SetActive(true);
                runnerGroup.RegisterAgent(item.agent);
            }
            item.agent.transform.localPosition = item.startPos;
            item.agent.transform.localRotation = item.startRot;
            item.rb.velocity = Vector3.zero;
            item.rb.angularVelocity = Vector3.zero;
            item.agent.hasruby=false;
            item.agent.ruby.SetActive(false);
            item.agent.m_navagent.ResetPath();
        }


        m_ResetTimer = 0;
        willCatchNum = runnerList.Count;
        rubygoal=false;
        DetectGoal=false;
        gameObject.GetComponent<CapsuleCollider>().isTrigger=false;
        Doorlist[goalIndex].GetComponent<ruby_goal>().Goal_reset();
        escapenum=0;

        if(train==TrainBrain.RunBrain){

        }
        else if(train==TrainBrain.DetectGoalBrain)
        {
            RandomPlayerGet_ruby(); //랜덤한 플레이어 루비 흭득(Rubyrunner2 브레인용)
            random_goal();
        }
        else if(train==TrainBrain.DetectRubyBrain)
        {
            RandomPos_ruby(); //루비 위치 랜덤으로 활성화
        }
        RandomPos_player(); //플레이어 랜덤 배치
        
    }
    public void RandomPos_ruby()
    /*  
        스크립트에 삽입된 Sector들중, 랜덤으로 ruby위치 선정
    */
    {
        int Randomindex=Random.Range(0,Sectors.Count);
        ruby.resetPlace(Randomindex);
    }

    public void RandomPos_player()
    {

        foreach(var item in chaserList)
        {
            int spawnSector=Random.Range(0,Sectors.Count);
            var spawnTransform = Sectors[spawnSector].transform;
            var xRange = spawnTransform.localScale.x / 2.1f;
            var zRange = spawnTransform.localScale.z / 2.1f;
            item.agent.transform.position= new Vector3(Random.Range(-xRange, xRange), 0.5f, Random.Range(-zRange, zRange))
            + spawnTransform.position;
        }

        foreach (var item in runnerList)
        {
            int spawnSector=Random.Range(0,Sectors.Count);
            var spawnTransform = Sectors[spawnSector].transform;
            var xRange = spawnTransform.localScale.x / 2.1f;
            var zRange = spawnTransform.localScale.z / 2.1f;
            item.agent.transform.position= new Vector3(Random.Range(-xRange, xRange), 0.5f, Random.Range(-zRange, zRange))
            + spawnTransform.position;

            item.agent.m_navagent.isStopped =true;
            item.agent.m_navagent.velocity = Vector3.zero;
        }
    }
    public void RandomPlayerGet_ruby()
    /*  
        랜덤한 플레이어 루비 흭득(Rubyrunner2 브레인용)
        *rubyrunner2 : 탈출브레인
    */
    {
        int Randomindex=Random.Range(0,runnerList.Count);
        key_player=Randomindex;
        findruby=true;
        
        runnerList[Randomindex].agent.GetComponent<ruby_runner>().hasruby=true;
        runnerList[Randomindex].agent.GetComponent<ruby_runner>().ruby.SetActive(true);
    }
    public void random_goal()
    /*  
        스크립트에 삽입된 door들중, 랜덤으로 Goal로 활성화
    */
    {
        goalIndex=Random.Range(0,Doorlist.Count);
        Doorlist[goalIndex].GetComponent<ruby_goal>().select_finishGoal();
    }

}
