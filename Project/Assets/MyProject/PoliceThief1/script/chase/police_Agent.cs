using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;

public class police_Agent : Agent
{
    [SerializeField]
    private PoliceThief_Area m_AreaSetting;
    private Rigidbody m_AgentRb;

    float speed;

    BehaviorParameters m_behaviorParameters;

    public enum Team
    {
        Police = 0,
        Thief = 1,
    }

    public Team team;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        speed=m_AreaSetting.agentRunSpeed*0.75f;
        m_behaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        team = Team.Police;
    }

    private void FixedUpdate()
    {
        // animation
        //if (m_AgentRb.velocity.magnitude > 0.05f)
        //    gameObject.GetComponent<Animator>().SetBool("Run", true);
        //else
        //    gameObject.GetComponent<Animator>().SetBool("Run", false);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "thief")
        {
            collision.gameObject.SetActive(false);
            m_AreaSetting.ThiefIsCatched();
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var dirToGoSideAction = act[2];

        if (dirToGoForwardAction == 1)
            dirToGo = 1f * transform.forward;
        else if (dirToGoForwardAction == 2)
            dirToGo = -1f * transform.forward;
        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;
        if (dirToGoSideAction == 1)
            dirToGo = -0.6f * transform.right;
        else if (dirToGoSideAction == 2)
            dirToGo = 0.6f * transform.right;

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * speed,
            ForceMode.VelocityChange);
    }

}
