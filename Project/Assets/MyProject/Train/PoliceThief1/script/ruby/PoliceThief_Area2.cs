using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class PoliceThief_Area2 : MonoBehaviour
{

    [System.Serializable]
    public class ThiefInfo
    {
        public thief_Agent agent;

        [HideInInspector]
        public Vector3 startPos;

        [HideInInspector]
        public Quaternion startRot;

        [HideInInspector]
        public Rigidbody rb;
    }


    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 3000; // 50 (per 1 second)

    [HideInInspector]
    public Bounds areaBounds;

    public GameObject ground;
    public GameObject rubby;

    public List<ThiefInfo> thiefList;

    private SimpleMultiAgentGroup thiefGroup;

    public float agentRunSpeed = 15f;

    [HideInInspector]
    public int m_ResetTimer;


    // Start is called before the first frame update
    void Start()
    {
        areaBounds = ground.GetComponent<Collider>().bounds;
        thiefGroup = new SimpleMultiAgentGroup();

        foreach (var item in thiefList)
        {
            item.startPos = item.agent.transform.position;
            item.startRot = item.agent.transform.rotation;
            item.rb = item.agent.GetComponent<Rigidbody>();
            thiefGroup.RegisterAgent(item.agent);
        }

        m_ResetTimer = 0;
        ResetScene();
    }

    private void FixedUpdate()
    {
        m_ResetTimer++;
        thiefGroup.AddGroupReward(-0.0001f * m_ResetTimer);
        if (m_ResetTimer > MaxEnvironmentSteps)
        {
            thiefGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public Vector3 GetRandomRubbyPos()
    {
        var foundNewLocation = false;
        var randomPos = Vector3.zero;

        while(foundNewLocation == false)
        {
            var x = Random.Range(-areaBounds.extents.x, areaBounds.extents.x);
            var z = Random.Range(-areaBounds.extents.z, areaBounds.extents.z);

            randomPos = ground.transform.position + new Vector3(x, 1f, z);
            if (Physics.CheckBox(randomPos, new Vector3(1.5f, 1.0f, 1.5f)) == false)
            {
                foundNewLocation = true;
            }
        }

        return randomPos;
    }

    public void FindRubby()
    {
        thiefGroup.AddGroupReward(1.0f);
        thiefGroup.EndGroupEpisode();
        ResetScene();
    }

    void ResetScene()
    {
        GetRandomRubbyPos();

        foreach (var item in thiefList)
        {
            if (!item.agent.gameObject.activeInHierarchy)
            {
                item.agent.gameObject.SetActive(true);
                thiefGroup.RegisterAgent(item.agent);
            }

            item.agent.transform.position = item.startPos;
            item.agent.transform.rotation = item.startRot;
            item.rb.velocity = Vector3.zero;
            item.rb.angularVelocity = Vector3.zero;
        }

        m_ResetTimer = 0;
    }
}
