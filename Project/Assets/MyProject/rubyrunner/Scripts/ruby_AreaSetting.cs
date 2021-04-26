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




    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 15000; // 50 (per 1 second)

    public List<ChaserInfo> chaserList;
    public List<RunnerInfo> runnerList;

    public List<GameObject> Sectors;

    public List<GameObject> Doorlist;
    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    public float agentRunSpeed = 30f;
    [HideInInspector]
    public int m_ResetTimer;
    [HideInInspector]
    public int willCatchNum; //Chaser가 잡을수있는 러너 수

    bool rubygoal;  //루비를 든 러너 탈출 확인
    int escapenum; //탈출 성공한 러너 수
    int goalIndex; //현재 goal의 index
    [System.NonSerialized]
    public int key_player; //현재 runnerlist중 ruby를 가진 멤버의 인덱스

    [System.NonSerialized]
    public bool findruby; //루비를 러너가 먹었을 시 활성화.

    public rubytrigger ruby;
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
        runnerGroup.AddGroupReward(-1.5f/MaxEnvironmentSteps);
   
        
        if(m_ResetTimer > MaxEnvironmentSteps)
        {
            chaserGroup.AddGroupReward(-escapenum);
            if(rubygoal){
                runnerGroup.AddGroupReward(escapenum);
            }
            else{
                runnerGroup.AddGroupReward(escapenum*0.5f);
            }
            runnerGroup.GroupEpisodeInterrupted();
            chaserGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

    }
    public void rubyGet(){
        runnerGroup.AddGroupReward(1.5f);
    }

   public void Scored(GameObject m_agent,bool iscaught)
    {
        bool flag=m_agent.GetComponent<ruby_runner>().hasruby;

        //잡혔을때
        if(iscaught){
            willCatchNum--;
            chaserGroup.AddGroupReward(1.0f);
            runnerGroup.AddGroupReward(-1.0f);

        }
        //탈출했을때
        else{
            willCatchNum--;
            escapenum++;

            if(flag)
            {
                rubygoal=true;
            }
        }

         if (willCatchNum <=0)
            {
                if(rubygoal){
                    runnerGroup.AddGroupReward(escapenum*2f);
                }
                else{
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
        }

        //루비 위치 랜덤으로 활성화
        int hidden_rubySector=Random.Range(0,Sectors.Count);
        ruby.resetPlace(hidden_rubySector);

        m_ResetTimer = 0;
        willCatchNum = runnerList.Count;
        rubygoal=false;
        Doorlist[goalIndex].GetComponent<ruby_goal>().Goal_reset();
        escapenum=0;
    }
    public void random_goal()
    {
        goalIndex=Random.Range(0,Doorlist.Count);
        Doorlist[goalIndex].GetComponent<ruby_goal>().select_finishGoal();
    }

}
