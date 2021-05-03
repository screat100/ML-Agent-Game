using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
public class ruby_runner : Agent
{
    [SerializeField]
    private ruby_AreaSetting m_AreaSetting;
    private Rigidbody m_AgentRb;

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
    }
     private void FixedUpdate()
    {
        if(SenseEnemy){
            
        }

        if(m_AreaSetting.DetectGoal&&hasruby){
            m_navagent.isStopped=false;
            m_navagent.updatePosition=true;
            m_navagent.updateRotation=true;
            m_navagent.SetDestination(m_AreaSetting.Doorlist[m_AreaSetting.goalIndex].transform.position+new Vector3(2.5f,0,0));
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
        m_AgentRb.AddForce(dirToGo * m_AreaSetting.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(hasruby);
        //sensor.AddObservation(SenseEnemy);
        //sensor.AddObservation(m_AreaSetting.findruby);
        sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
        sensor.AddObservation(m_AreaSetting.DetectGoal);
        // sensor.AddObservation(transform.localPosition.x);
        // sensor.AddObservation(transform.localPosition.z);
        // sensor.AddObservation(m_AreaSetting.Doorlist[m_AreaSetting.goalIndex].transform.localPosition.x);
        // sensor.AddObservation(m_AreaSetting.Doorlist[m_AreaSetting.goalIndex].transform.localPosition.z);

        // if(m_AreaSetting.DetectGoal){
        //      sensor.AddObservation(m_AreaSetting.Doorlist[m_AreaSetting.goalIndex].transform.localPosition.x);
        //      sensor.AddObservation(m_AreaSetting.Doorlist[m_AreaSetting.goalIndex].transform.localPosition.z);
        // }
        // else if(m_AreaSetting.RunBrain){
            
        // }
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
