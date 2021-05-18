using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CVR5_Agent : Agent
{
    [SerializeField]
    private CVR5_Area m_AreaSetting;
    private Rigidbody m_AgentRb;
    BehaviorParameters m_behaviorParameters;



    public enum Team
    {
        Chaser = 0,
        Runner = 1,
    }

    public Team team;



    private void OnCollisionEnter(Collision collision)
    {
        if (transform.tag == "police" && collision.transform.tag == "thief")
        {
            collision.gameObject.SetActive(false);
            m_AreaSetting.RunnerIsCatched();

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(transform.tag == "police" && other.tag == "areaDetector")
        //{
        //    string areaName = other.gameObject.name;
        //    string[] splitString = areaName.Split(' ');
        //    int areaNumber = int.Parse(splitString[1]);
        //    if(!m_AreaSetting.visited[areaNumber])
        //    {
        //        m_AreaSetting.visited[areaNumber] = true;
        //        m_AreaSetting.NewAreaVisitReward();
        //    }
        //}
        if (transform.tag == "police" &&
            other.tag == "areaDetector")
        {
            other.gameObject.SetActive(false);
            m_AreaSetting.NewAreaVisitReward();
        }
    }

    private void FixedUpdate()
    {
        float velocity = m_AgentRb.velocity.magnitude;
        
        // animation
        if (velocity > 0.05f)
            gameObject.GetComponent<Animator>().SetBool("Run", true);
        else
            gameObject.GetComponent<Animator>().SetBool("Run", false);

        if (velocity < 0.75f)
            AddReward(-1.0f / m_AreaSetting.MaxEnvironmentSteps);
        else
            AddReward(1.0f / m_AreaSetting.MaxEnvironmentSteps);


    }

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
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
                dirToGo = transform.right * -0.75f;
                break;
            case 4:
                dirToGo = transform.right * 0.75f;
                break;
            case 5:
                rotateDir = transform.up * 1f;
                break;
            case 6:
                rotateDir = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);

        if (m_AgentRb.velocity.magnitude <= 10f)
        {
            m_AgentRb.AddForce(dirToGo * m_AreaSetting.agentRunSpeed);
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
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
