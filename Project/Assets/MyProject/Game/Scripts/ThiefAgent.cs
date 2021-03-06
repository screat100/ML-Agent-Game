using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
using Unity.Barracuda;
using Unity.MLAgentsExamples;

public class ThiefAgent : Agent
{
    [SerializeField]
    private StageSetting m_AreaSetting;
    private Rigidbody m_AgentRb;

    AudioSource m_footstep;

    public NNModel DetectGoal;
    public NNModel DetectRuby;
    public NNModel RunModel;

    private float m_Detectradius;

    string m_DetectGoalBehaviorName = "rubyrun_DetectGoal";
    string m_DetectRubyBehaviorName = "rubyrun_DetectRuby";
    string m_RunModelBehaviorName = "rubyrun_RunModel";

    int m_Configuration;

    [System.NonSerialized]
    public bool hasruby;

    BehaviorParameters m_behaviorParameters;
    private bool SenseEnemy;

    [System.NonSerialized]
    public bool Detected;
    public enum Team
    {
        Chaser = 0,
        Runner = 1,
    }

    public Team team;
    [System.NonSerialized]

    public NavMeshAgent m_navagent;
    public LayerMask m_chaserlayermask=0;
    public LayerMask m_Goallayermask=0;

    //루비먹었을시 미니맵 표시
    [System.NonSerialized]
    public GameObject m_minimapThief;
    //도망 방향벡터
    Vector3 RunVector;
    //도망가야할 방향과 현재 내 각도의 차이
    float runAngle;
    float ClosestPoliceDist;
    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_navagent = GetComponent<NavMeshAgent>();
        m_behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        runAngle=0f;
        m_Detectradius = m_AreaSetting.runnerDetectRadius;

        m_minimapThief = transform.Find("Minimap_rubyThief").gameObject;
        m_minimapThief.SetActive(false);

        GameObject instant_footstep = GameObject.Instantiate(GameObject.Find("Sounds").transform.Find("SFXs").transform.Find("Thief_footstep")).gameObject;
        instant_footstep.transform.name = "Thief_footstep";
        instant_footstep.transform.SetParent(transform);
        instant_footstep.transform.localPosition = new Vector3(0, 0, 0);
        m_footstep = gameObject.transform.Find("Thief_footstep").GetComponent<AudioSource>();

        if (m_behaviorParameters.TeamId == (int)Team.Chaser)
        {
            team = Team.Chaser;
        }
        else if (m_behaviorParameters.TeamId == (int)Team.Runner)
        {
            team = Team.Runner;
        }

        var modelOverrider = GetComponent<ModelOverrider>();
        if (modelOverrider.HasOverrides)
        {
            DetectGoal = modelOverrider.GetModelForBehaviorName(m_DetectGoalBehaviorName);
            m_DetectGoalBehaviorName = ModelOverrider.GetOverrideBehaviorName(m_DetectGoalBehaviorName);

            DetectRuby = modelOverrider.GetModelForBehaviorName(m_DetectRubyBehaviorName);
            m_DetectRubyBehaviorName = ModelOverrider.GetOverrideBehaviorName(m_DetectRubyBehaviorName);

            RunModel = modelOverrider.GetModelForBehaviorName(m_RunModelBehaviorName);
            m_RunModelBehaviorName = ModelOverrider.GetOverrideBehaviorName(m_RunModelBehaviorName);
        }

    }
     private void FixedUpdate()
    {
         ConfigureAgent();

        if(Detected){
            transform.Find("Minimap_thief").gameObject.layer=14;
            //transform.Find("Minimap_thief").gameObject.layer=LayerMask.GetMask("detected_img");
            Detected=false;
        }
        else{
            transform.Find("Minimap_thief").gameObject.layer=9;
            //transform.Find("Minimap_thief").gameObject.layer=LayerMask.GetMask("thief_img");
        }

        Vector3 current_velocity = transform.InverseTransformDirection(m_AgentRb.velocity);
        if (m_AreaSetting.train == StageSetting.TrainBrain.RunBrain || m_AreaSetting.train == StageSetting.TrainBrain.TotalBrain)
        {
            runAngle = 0f;
            if (SenseEnemy)
            {
                runAngle = Vector3.Angle(transform.forward, RunVector);
                m_AreaSetting.Reward_Get(-1 / (m_AreaSetting.maxPlayTime*50)*runAngle);
            }
        }
        


        
        if(current_velocity.z > m_AreaSetting.agentRunSpeed*0.75f){
            m_AreaSetting.Reward_Get(+1f/(m_AreaSetting.maxPlayTime*50*(float)m_AreaSetting.thiefAgents.Length));
        }
        else{
            m_AreaSetting.Reward_Get(-1f/(m_AreaSetting.maxPlayTime*50*(float)m_AreaSetting.thiefAgents.Length));
        }

        if(m_AreaSetting.DetectGoal&&hasruby){
            m_navagent.isStopped=false;
            m_navagent.updatePosition=true;
            m_navagent.updateRotation=true;
            m_navagent.SetDestination(m_AreaSetting.Doorlist[m_AreaSetting.goalIndex].transform.position);
        }
        //Goal 찾기
        Detect();

         // animation
        if (m_AgentRb.velocity.magnitude > 0.05f)
        {
            gameObject.GetComponent<Animator>().SetBool("Run", true);
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("Run", false);
        }
            
            
        if(m_AreaSetting.agentRunSpeed<m_AgentRb.velocity.z)
        {
            m_AgentRb.velocity=new Vector3(m_AgentRb.velocity.x,0,m_AreaSetting.agentRunSpeed);
        }

        if(-m_AreaSetting.agentRunSpeed>m_AgentRb.velocity.z)
        {
            m_AgentRb.velocity=new Vector3(m_AgentRb.velocity.x,0,-m_AreaSetting.agentRunSpeed);
        }

        if(m_AreaSetting.agentRunSpeed<m_AgentRb.velocity.x)
        {
            m_AgentRb.velocity=new Vector3(m_AreaSetting.agentRunSpeed,0,m_AgentRb.velocity.z);
        }

        if(-m_AreaSetting.agentRunSpeed>m_AgentRb.velocity.x)
        {
            m_AgentRb.velocity=new Vector3(-m_AreaSetting.agentRunSpeed,0,m_AgentRb.velocity.z);
        }
    }

    void Detect()
    {
        //감지거리에 Goal 이 있을 때 다른 러너와 위치 공유
        Collider[] Goals=Physics.OverlapSphere(transform.position,m_AreaSetting.runnerDetectRadius,m_Goallayermask);

        if(Goals.Length>0){
                Vector3 dir=(Goals[0].transform.position-transform.position).normalized;
                float t_angle = Vector3.Angle(dir, transform.forward);
                float m_SightAngle=gameObject.GetComponent<RayPerceptionSensorComponent3D>().MaxRayDegrees*2f;
                //시야각에 잡히면
                if(t_angle <m_SightAngle * 0.5f){
                    RaycastHit hit;
                    if(Physics.Raycast(transform.position+Vector3.up, dir, out hit)){
                        if(hit.collider.tag=="goal"&&!m_AreaSetting.DetectGoal){
                            m_AreaSetting.DetectGoal=true;
                            m_AreaSetting.Reward_Get(0.5f);
                        }
                    }
                }
        }

        


        //주변 적들 감지
        Collider[] Enemys=Physics.OverlapSphere(transform.position, m_Detectradius, m_chaserlayermask);
        //도망 방향벡터
        RunVector= Vector3.zero;
        SenseEnemy = false;
        //감지된 가장가까운 적과의 거리
        ClosestPoliceDist= m_Detectradius + 1f;
        if(Enemys.Length>0){
            SenseEnemy=true;
            m_Detectradius = 30f;
            /* 발걸음 감지된 적을 미니맵에 표시 */
            foreach(var Enemy in Enemys){
                if(GameManager.instance.playersTeam == Player.Team.thief)
                { 
                    Enemy.GetComponent<PoliceAgent>().Detected=true;
                }
            }

            /* 주변에 위치한 적의 방향을 파악하고 도망갈 방향 지정 */
            foreach(var item in Enemys){
                RunVector+=transform.localPosition-item.transform.localPosition;
            }
            RunVector=RunVector.normalized;

            /* 가장 가까운 적과의 거리 */
            foreach(var item in Enemys){
                float dist=Vector3.Distance(transform.position,item.transform.position);
                if(ClosestPoliceDist>dist){
                    ClosestPoliceDist=dist;
                }
            }
        }
        else
        {
            resetRadiusdist();
        }
        
    }
    void resetRadiusdist()
    {
        m_Detectradius = 20f;
    }
    public void ConfigureAgent()
    {
        if (m_AreaSetting.train == StageSetting.TrainBrain.DetectGoalBrain)
        {
            SetModel(m_DetectGoalBehaviorName, DetectGoal);
        }
        else if (m_AreaSetting.train ==StageSetting.TrainBrain.DetectRubyBrain)
        {
            SetModel(m_DetectRubyBehaviorName, DetectRuby);
        }
        else if (m_AreaSetting.train == StageSetting.TrainBrain.RunBrain)
        {
            SetModel(m_RunModelBehaviorName, RunModel);
        }
        else if(m_AreaSetting.train==StageSetting.TrainBrain.TotalBrain){
            //적을 감지했다면 - 도망
            if(SenseEnemy){
                SetModel(m_RunModelBehaviorName, RunModel);
            }
            //루비를 찾지못했다면 - 루비를 찾는다
            else if(!m_AreaSetting.findruby){
                SetModel(m_DetectRubyBehaviorName, DetectRuby);
            }
            //루비를 찾았다면 - 탈출구를 찾는다.
            else{
                SetModel(m_DetectGoalBehaviorName, DetectGoal);
            }
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        if (!(m_AreaSetting.DetectGoal && hasruby))
        {
            m_AgentRb.AddForce(dirToGo * m_AreaSetting.agentRunSpeed *1.3f,
            ForceMode.VelocityChange);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if(m_AreaSetting.train==StageSetting.TrainBrain.DetectRubyBrain){
            sensor.AddObservation(hasruby); //필요없으므로 이후 SenseEnemy로 교체 또는 벡터 사이즈 변경 가능 여부 확인할예정
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
            sensor.AddObservation(m_AreaSetting.DetectGoal);
        }
        else if(m_AreaSetting.train==StageSetting.TrainBrain.DetectGoalBrain){
            sensor.AddObservation(hasruby);
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
            sensor.AddObservation(m_AreaSetting.DetectGoal);
        }
        else if(m_AreaSetting.train==StageSetting.TrainBrain.RunBrain){
            Vector3 cur_velocity=transform.InverseTransformDirection(m_AgentRb.velocity);
            sensor.AddObservation(SenseEnemy);
            sensor.AddObservation(cur_velocity.x);
            sensor.AddObservation(cur_velocity.z);
            sensor.AddObservation(ClosestPoliceDist);
            sensor.AddObservation(runAngle);
        }
        else if(m_AreaSetting.train==StageSetting.TrainBrain.TotalBrain){
            if(SenseEnemy){
                Vector3 cur_velocity=transform.InverseTransformDirection(m_AgentRb.velocity);
                sensor.AddObservation(SenseEnemy);
                sensor.AddObservation(cur_velocity.x);
                sensor.AddObservation(cur_velocity.z);
                sensor.AddObservation(ClosestPoliceDist);
                sensor.AddObservation(runAngle);
            }
            else if(m_AreaSetting.findruby){
                sensor.AddObservation(hasruby);
                sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
                sensor.AddObservation(m_AreaSetting.DetectGoal);
            }
            else{
                sensor.AddObservation(hasruby);
                sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
                sensor.AddObservation(m_AreaSetting.DetectGoal);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut.Clear();
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 3;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 4;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[0] = 5;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[0] = 6;
        }
    }
}
