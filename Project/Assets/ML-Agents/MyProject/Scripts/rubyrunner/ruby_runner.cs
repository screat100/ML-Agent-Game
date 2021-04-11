using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ruby_runner : Agent
{
    [SerializeField]
    private ruby_AreaSetting m_AreaSetting;
    private Rigidbody m_AgentRb;

    [System.NonSerialized]
    public bool hasruby;
    
    [System.NonSerialized]
    public float recently_distance;
    [System.NonSerialized]
    public float current_distance;

    BehaviorParameters m_behaviorParameters;
    public GameObject ruby;
    public enum Team
    {
        Chaser = 0,
        Runner = 1,
    }

    public Team team;
    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        recently_distance=Vector3.Distance(m_AreaSetting.goal.transform.position, transform.position);
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
        sensor.AddObservation(m_AreaSetting.goal.transform.localPosition.x);
        sensor.AddObservation(m_AreaSetting.goal.transform.localPosition.z);
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(m_AreaSetting.runnerList[m_AreaSetting.key_player].agent.transform.localPosition.x);
        sensor.AddObservation(m_AreaSetting.runnerList[m_AreaSetting.key_player].agent.transform.localPosition.z);
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
