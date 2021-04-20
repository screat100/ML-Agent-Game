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

    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    public float agentRunSpeed = 30f;
    [HideInInspector]
    public int m_ResetTimer;
    [HideInInspector]
    public int willCatchNum;

    public GameObject goal;
    bool rubygoal;
    int escapenum;

    public int key_player;
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

        
        runnerList[key_player].agent.current_distance=Vector3.Distance(goal.transform.position, runnerList[key_player].agent.transform.position);
        float start_dis=Vector3.Distance(runnerList[key_player].startPos,goal.transform.localPosition);

        if(!rubygoal){
            runnerGroup.AddGroupReward(-runnerList[key_player].agent.current_distance/(start_dis*1000f));
        }
        else{
            runnerGroup.AddGroupReward(-1/MaxEnvironmentSteps);
        }
        runnerList[key_player].agent.recently_distance=runnerList[key_player].agent.current_distance;

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


   public void Scored(GameObject m_agent,bool iscaught)
    {
        bool flag=m_agent.GetComponent<ruby_runner>().hasruby;

        if(iscaught){
            willCatchNum--;
            chaserGroup.AddGroupReward(1.0f);
            runnerGroup.AddGroupReward(-1.0f);
            //보석을 가지고있는 애가 잡히면 그룹처벌
            // if(flag)
            // {
            //    runnerGroup.AddGroupReward(-1.0f);
            // }
            // else{
            //     runnerGroup.AddGroupReward(-0.3f);
            // }

        }
        else{
            willCatchNum--;
            escapenum++;
            //chaserGroup.AddGroupReward(-1.0f);

            //보석을 가지고있는 애가 탈출하면 더 큰 보상
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

        //key player active
        key_player=Random.Range(0,runnerList.Count);
        runnerList[key_player].agent.hasruby=true;
        runnerList[key_player].agent.ruby.SetActive(true);
        m_ResetTimer = 0;
        willCatchNum = runnerList.Count;
        rubygoal=false;
        escapenum=0;
    }


}
