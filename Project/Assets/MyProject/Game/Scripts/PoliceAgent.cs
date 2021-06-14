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
    AudioSource m_footstep;
    public float AgentSpeed = 100f;
    [System.NonSerialized]
    public bool Detected;
    public LayerMask m_thiefLayermask=0;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "thief")
        {
            //collision.gameObject.SetActive(false);
            GameObject.Find("Sounds").GetComponent<SoundManager>().PlayAudio("Catch", transform.position);
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
        /* 정면 시야에 적이 감지되었을때 미니맵에 표시 */
        Detect();
        /* 내가 적에게 감지되었을때 미니맵에 표시 */
        if (Detected)
        {
            //transform.Find("Minimap_police").gameObject.layer = LayerMask.GetMask("detected_img");
            transform.Find("Minimap_police").gameObject.layer = 14;
            Detected = false;
        }
        else
        {
            transform.Find("Minimap_police").gameObject.layer = 10;
            //transform.Find("Minimap_police").gameObject.layer = LayerMask.GetMask("police_img");
        }

        float velocity = m_AgentRb.velocity.magnitude;

        // animation
        if (velocity > 0.05f)
        {
            if (!m_footstep.isPlaying)
                m_footstep.Play();
            gameObject.GetComponent<Animator>().SetBool("Run", true);
        }
        else
        {
            m_footstep.Pause();
            gameObject.GetComponent<Animator>().SetBool("Run", false);
        }

        if (GameManager.instance.phase == GameManager.Phase.play &&
        velocity < 0.75f)
            AddReward(-1.0f / m_AreaSetting.maxPlayTime * 50);
        else if (GameManager.instance.phase == GameManager.Phase.play)
            AddReward(1.0f / m_AreaSetting.maxPlayTime * 50);

    }
    private void Detect()
    {
        Collider[] Enemys = Physics.OverlapSphere(transform.position, m_AreaSetting.runnerDetectRadius, m_thiefLayermask);
        if (Enemys.Length > 0)
        {
            foreach (var Enemy in Enemys)
            {
                Vector3 dir=(Enemy.transform.position-transform.position).normalized;
                float t_angle = Vector3.Angle(dir, transform.forward);
                float m_SightAngle=gameObject.GetComponent<RayPerceptionSensorComponent3D>().MaxRayDegrees*2f;
                //시야각에 잡히면
                if(t_angle <m_SightAngle * 0.5f){
                    RaycastHit hit;
                    if(Physics.Raycast(transform.position+Vector3.up, dir, out hit)){
                        if(hit.collider.tag=="police"){
                            Enemy.GetComponent<ThiefAgent>().Detected=true;
                        }
                    }
                }
            }
        }
    }
    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        GameObject instant_footstep = GameObject.Instantiate(GameObject.Find("Sounds").transform.Find("SFXs").transform.Find("Police_footstep")).gameObject;
        instant_footstep.transform.name = "Police_footstep";
        instant_footstep.transform.SetParent(transform);
        instant_footstep.transform.localPosition = new Vector3(0, 0, 0);
        m_footstep = gameObject.transform.Find("Police_footstep").GetComponent<AudioSource>();
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

        if (GameManager.instance.phase == GameManager.Phase.play &&
            m_AgentRb.velocity.magnitude <= m_AreaSetting.agentRunSpeed * 0.95f)
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
