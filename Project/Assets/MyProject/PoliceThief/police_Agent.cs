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
        Thief = 1
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

    //public override void CollectObservations(VectorSensor sensor)
    //{
    //    sensor.AddObservation(this.transform.localPosition);
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "thief")
        {
            collision.gameObject.SetActive(false);
            m_AreaSetting.ThiefIsCatched();
        }

        if (collision.transform.tag == "obstacle" || collision.transform.tag == "wall"){
            AddReward(-0.1f);
        }
    }

   // public override void OnActionReceived(ActionBuffers actionBuffers)
   //{
   //     Vector3 move = Vector3.zero;
   //     if (float.IsNaN(actionBuffers.ContinuousActions[0]) || float.IsNaN(actionBuffers.ContinuousActions[1]))
   //     {
   //         return;
   //     }
   //     move.x = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
   //     move.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
   //     //this.transform.Rotate(this.transform.up * Time.fixedDeltaTime * 200);
   //     m_AgentRb.AddForce(move * m_AreaSetting.agentRunSpeed, ForceMode.VelocityChange);
   //}

    
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
        m_AgentRb.AddForce(dirToGo * speed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
    }


}
