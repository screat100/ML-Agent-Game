using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PoliceAgent : Agent
{
    [SerializeField]
    private StageSetting m_AreaSetting;
    private Rigidbody m_AgentRb;
    BehaviorParameters m_behaviorParameters;

    public float AgentSpeed = 100f;



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "thief")
        {
            collision.gameObject.SetActive(false);
            m_AreaSetting.GetReward_police();

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "areaDetector")
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

        if (m_AgentRb.velocity.magnitude <= m_AreaSetting.agentRunSpeed * 0.95f)
        {
            m_AgentRb.AddForce(dirToGo * AgentSpeed);
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
