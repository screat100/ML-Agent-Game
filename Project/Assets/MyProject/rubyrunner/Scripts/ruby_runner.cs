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

public class ruby_runner : Agent
{
    [SerializeField]
    private ruby_AreaSetting m_AreaSetting;
    private Rigidbody m_AgentRb;

    public NNModel DetectGoal;
    public NNModel DetectRuby;
    public NNModel RunModel;

    string m_DetectGoalBehaviorName = "DetectGoalBrain";
    string m_DetectRubyBehaviorName = "rubyrun";
    string m_RunModelBehaviorName = "RunBrain";

    int m_Configuration;

    [System.NonSerialized]
    public bool hasruby;

    BehaviorParameters m_behaviorParameters;
    private bool SenseEnemy;
    public GameObject ruby;
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
    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_navagent = GetComponent<NavMeshAgent>();
        m_behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
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

        if (SenseEnemy){
            m_AreaSetting.Reward_Get(-1f/m_AreaSetting.MaxEnvironmentSteps);
        }
        
        Vector3 current_velocity=transform.InverseTransformDirection(m_AgentRb.velocity);
        if(current_velocity.z > m_AreaSetting.agentRunSpeed*0.5f){
            AddReward(+1f/m_AreaSetting.MaxEnvironmentSteps);
        }
        else if(current_velocity.z < 0.0f){
            AddReward(-1f/m_AreaSetting.MaxEnvironmentSteps);
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
            gameObject.GetComponent<Animator>().SetBool("Run", true);
        else
            gameObject.GetComponent<Animator>().SetBool("Run", false);
            
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
        Collider[] Goals=Physics.OverlapSphere(transform.position,m_AreaSetting.RunnerDetectRadius,m_Goallayermask);

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
                            //m_AreaSetting.Reward_Get(0.5f);
                        }
                    }
                }
        }

        //주변에 Chaser가 있을 때 감지
        SenseEnemy=false;
        

        Collider[] Enemys=Physics.OverlapSphere(transform.position,m_AreaSetting.RunnerDetectRadius, m_chaserlayermask);

        if(Enemys.Length>0){
            SenseEnemy=true;
            // RaycastHit hit;
            // Vector3 dir=Enemys[0].transform.position-transform.position;
            // if(Physics.Raycast(transform.position+Vector3.up, dir, out hit)){
            //     if(hit.collider.tag=="chaser"){
            //         SenseEnemy=true;
            //     }
            // }
        }

    }

    public void ConfigureAgent()
    {
        if (m_AreaSetting.train == ruby_AreaSetting.TrainBrain.DetectGoalBrain)
        {
            SetModel(m_DetectGoalBehaviorName, DetectGoal);
        }
        else if (m_AreaSetting.train == ruby_AreaSetting.TrainBrain.DetectRubyBrain)
        {
            SetModel(m_DetectRubyBehaviorName, DetectRuby);
        }
        else if (m_AreaSetting.train == ruby_AreaSetting.TrainBrain.RunBrain)
        {
            SetModel(m_RunModelBehaviorName, RunModel);
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
            m_AgentRb.AddForce(dirToGo * m_AreaSetting.agentRunSpeed,
            ForceMode.VelocityChange);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if(m_AreaSetting.train==ruby_AreaSetting.TrainBrain.DetectRubyBrain){
            sensor.AddObservation(hasruby); //필요없으므로 이후 SenseEnemy로 교체 또는 벡터 사이즈 변경 가능 여부 확인할예정
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
            sensor.AddObservation(m_AreaSetting.DetectGoal);
        }
        else if(m_AreaSetting.train==ruby_AreaSetting.TrainBrain.DetectGoalBrain){
            sensor.AddObservation(hasruby);
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
            sensor.AddObservation(m_AreaSetting.DetectGoal);
        }
        else if(m_AreaSetting.train==ruby_AreaSetting.TrainBrain.RunBrain){
            sensor.AddObservation(SenseEnemy);
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
            sensor.AddObservation(m_AreaSetting.DetectGoal); //필요없으므로 이후 벡터 사이즈 변경 가능 여부 확인
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
