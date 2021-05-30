using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayDetector : MonoBehaviour
{
    public CvR3_Agent agent;

    [Header("Ray Information")]

    [Range(0, 360)]
    public float RayAngleHorizontal;

    [Range(0, 45)]
    public float RayAngleVertical;

    [Range(3, 36)]
    public int RayNumHorizontal;

    [Range(2, 5)]
    public int RayNumVertical;

    [Range(5, 300)]
    public float RayMaxDistance;

    RaycastHit hit;


    void Update()
    {
    }

    private void FixedUpdate()
    {
        ShootRays();
    }

    Vector3 RotateHorizontal(Vector3 Origin, float angle)
    {
        Vector3 resultVector = Vector3.zero;

        resultVector.x = Origin.x * Mathf.Cos(angle*3.14f/ 180f) - Origin.z * Mathf.Sin(angle*3.14f/180f);
        resultVector.z = Origin.x * Mathf.Sin(angle * 3.14f / 180f) + Origin.z * Mathf.Cos(angle * 3.14f / 180f);
        resultVector.y = Origin.y;

        return resultVector;
    }

    Vector3 RotateVertical(Vector3 Origin, float angle)
    {
        Vector3 resultVector = Vector3.zero;

        float xz = Mathf.Sqrt(Origin.x * Origin.x + Origin.z * Origin.z);

        resultVector.x = Origin.x * (Mathf.Cos(angle * 3.14f / 180f) - Origin.y * Mathf.Sin(angle * 3.14f / 180f) / xz);
        resultVector.y = Origin.y * Mathf.Cos(angle * 3.14f / 180f) + xz * Mathf.Sin(angle * 3.14f / 180f);
        resultVector.z = Origin.z * (Mathf.Cos(angle * 3.14f / 180f) - Origin.y * Mathf.Sin(angle * 3.14f / 180f) / xz);

        return resultVector;
    }


    public void ShootRays()
    {
        float AngleTickHorizontal = RayAngleHorizontal / (RayNumHorizontal-1);
        float AngleTickVertical = RayAngleVertical / (RayNumVertical - 1);

        float initAngleHorizontal = RayAngleHorizontal / 2;
        float initAngleVertical = RayAngleVertical / 2;



        for (int i = 0; i < RayNumHorizontal; i++)
        {
            Vector3 RayDirection = RotateHorizontal(gameObject.transform.forward, initAngleHorizontal);
            RayDirection = RotateHorizontal(RayDirection, -AngleTickHorizontal * i);

            for(int j=0; j<RayNumVertical; j++)
            {
                Vector3 RayDirVertical = RotateVertical(RayDirection, initAngleVertical);
                RayDirVertical = RotateVertical(RayDirVertical, -AngleTickVertical * j);

                if (Physics.Raycast(transform.position, RayDirVertical, out hit, RayMaxDistance))
                {
                    if(agent.team == CvR3_Agent.Team.Chaser && hit.transform.tag == "runner")
                    {
                        //Debug.DrawRay(transform.position, RayDirVertical * RayMaxDistance, Color.blue, Time.fixedDeltaTime);

                        Vector3 hitPosition = new Vector3(hit.transform.position.x,
                            0,
                            hit.transform.position.z);

                        agent.isDetectRunner = true;
                        agent.runnersPosition = hitPosition;

                        return;
                    }
                    else if (agent.team == CvR3_Agent.Team.Runner && hit.transform.tag == "chaser")
                    {
                        //Debug.DrawRay(transform.position, RayDirVertical * RayMaxDistance, Color.red, Time.fixedDeltaTime);

                        Vector3 hitPosition = new Vector3(hit.transform.position.x,
                            0,
                            hit.transform.position.z);

                        agent.isDetectRunner = true;
                        agent.runnersPosition = hitPosition;

                        return;
                    }
                    else
                    {
                        //Debug.DrawRay(transform.position, RayDirVertical * RayMaxDistance, Color.white, Time.fixedDeltaTime);
                    }
                }
            }
        }

        agent.isDetectRunner = false;
        agent.runnersPosition = Vector3.zero;


    }
}
