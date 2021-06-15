using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    /*      ===         Sub Classes         === */

    public enum Team
    {
        police = 0,
        thief = 1,
    }


    /*      ===         variables         === */


    Animator m_Animator;
    Rigidbody m_Rigidbody;
    GameObject m_Camera;
    StageSetting m_StageSetting;

    public Team team;
    public float movePower = 1.0f;
    public GameObject lightOfPolice;
    public GameObject lightOfThief;

    public StageSetting m_AreaSetting;
    public LayerMask m_Goallayermask = 0;
    // related control
    public bool controllActivate;
    float AngleX;
    float AngleY;
    [Range(0.1f, 10.0f)] public float mouseSensitive = 1.0f;

    bool ESCPressed = false;

    // sounds
    public AudioSource audio_Footstep;
    float heartbeatRange = 25f;
    public AudioSource audio_Hearbeat;

    public LayerMask m_PoliceLayer;

    [System.NonSerialized]
    public bool hasruby;
    /*      ===         Functions         === */

    void Start()
    {
        controllActivate = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        hasruby = false;
        m_Animator = gameObject.GetComponent<Animator>();
        m_Rigidbody = gameObject.GetComponent<Rigidbody>();
        m_Camera = transform.Find("Main Camera").gameObject;

        m_StageSetting = GameObject.Find("GameArea").GetComponent<StageSetting>();



        switch (GameManager.instance.playersTeam)
        {
            case Team.police:
                team = Team.police;
                gameObject.layer =7;
                break;
            case Team.thief:
                team = Team.thief;
                gameObject.layer = 0;
                break;
        }

        TurnOnSpotLight();

        if(GameManager.instance.playersTeam == Player.Team.thief)
        {
            gameObject.transform.position = GameObject.Find("GameArea").GetComponent<StageSetting>().thiefAgents[GameManager.instance.thiefNum-1].transform.position;
        }
        else
        {
            gameObject.transform.position = GameObject.Find("GameArea").GetComponent<StageSetting>().policeAgents[GameManager.instance.policeNum-1].transform.position;
        }

        SetFootStep();

    }
    void SetFootStep()
    {
        if (GameManager.instance.playersTeam == Player.Team.thief)
        {
            /* 역할별 FootStep */
            audio_Footstep.clip = GameObject.Find("Sounds").transform.Find("SFXs").transform.Find("Thief_footstep").gameObject.GetComponent<AudioSource>().clip;
        }
        else
        {
            /* 역할별 FootStep */
            audio_Footstep.clip = GameObject.Find("Sounds").transform.Find("SFXs").transform.Find("Police_footstep").gameObject.GetComponent<AudioSource>().clip;
        }
    }
    void Update()
    {
        PushESCKey();

        if(controllActivate)
        {
            KeyboardMove();
            MouseMove();
        }

        PlayHeartBeatSound();
        if (GameManager.instance.playersTeam == Player.Team.thief)
            GoalDetect();
    }
    void GoalDetect()
    {
        //감지거리에 Goal 이 있을 때 다른 러너와 위치 공유
        Collider[] Goals = Physics.OverlapSphere(transform.position, m_AreaSetting.runnerDetectRadius, m_Goallayermask);

        if (Goals.Length > 0)
        {
            Vector3 dir = (Goals[0].transform.position - transform.position).normalized;
            float t_angle = Vector3.Angle(dir, transform.forward);
            float m_SightAngle = 90f * 2f;
            //시야각에 잡히면
            if (t_angle < m_SightAngle * 0.5f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, dir, out hit))
                {
                    if (hit.collider.tag == "goal" && !m_AreaSetting.DetectGoal)
                    {
                        m_AreaSetting.DetectGoal = true;
                    }
                }
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (transform.tag == "thief" && collision.transform.tag == "police")
        {
            for (int i=0; i< m_StageSetting.thiefAgents.Length; i++)
            {
                if (m_StageSetting.thiefAgents[i].activeSelf == true)
                {
                    transform.position = m_StageSetting.thiefAgents[i].transform.position; // 활동 중인 thief 중 하나로 자리 교체
                    m_StageSetting.thiefAgents[i].SetActive(false); // 그 자리에 있던 thiefAgent 비활성화
                    //UI 틀기
                    change();
                    Invoke("change_exit", 2);
                    break;
                }
            }
        }

        if(transform.tag == "police" && collision.transform.tag == "thief")
        {
            GameManager.instance.policePlayerRecord++;
            collision.gameObject.SetActive(false);
            m_StageSetting.GetReward_police();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (transform.tag == "police" && other.tag == "areaDetector")
        {
            other.gameObject.SetActive(false);
            m_StageSetting.NewAreaVisitReward();
        }
    }

    private void change()
    {
        GameObject.Find("UI_Playing").transform.Find("change").gameObject.SetActive(true);
    }
    private void change_exit()
    {
        GameObject.Find("UI_Playing").transform.Find("change").gameObject.SetActive(false);
    }

    /*      ===         functions that run on Start()         === */



    /*      ===         functions that run on Update()         === */

    void KeyboardMove()
    {
        m_Rigidbody.velocity = Vector3.zero;

        float forwardMove = 0;
        float rightwardMove = 0;

        if(Input.GetKey(KeyCode.W))
        {
            forwardMove = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            forwardMove = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            rightwardMove = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rightwardMove = 1;
        }


        if(forwardMove !=0 && rightwardMove !=0)
        {
            forwardMove *= 1/Mathf.Sqrt(2);
            rightwardMove *= 1/Mathf.Sqrt(2);
        }
        
        //transform.position +=
        //    transform.forward * forwardMove * movePower
        //    + transform.right * rightwardMove * movePower;

        if(m_Rigidbody.velocity.magnitude <= 10) 
        {
            m_Rigidbody.AddForce(transform.forward * forwardMove * movePower * Time.deltaTime);
            m_Rigidbody.AddForce(transform.right * rightwardMove * movePower * Time.deltaTime);
        }

    }

    void MouseMove()
    {
        AngleX += Input.GetAxis("Mouse X") * mouseSensitive;
        AngleY -= Input.GetAxis("Mouse Y") * mouseSensitive;


        // y축을 상하 90도(=180도)로 제한
        if (AngleY >= 90)
            AngleY = 90;

        if (AngleY <= -90)
            AngleY = -90;


        gameObject.transform.eulerAngles = new Vector3(0, AngleX, 0.0f);
        m_Camera.transform.eulerAngles = new Vector3(AngleY, AngleX, 0.0f);
    }

    void PushESCKey()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !ESCPressed)
        {
            OnEditMode(true);

            GameObject.Find("Sounds").GetComponent<SoundManager>().PauseAudio("IngameBGM");
            GameObject.Find("Sounds").GetComponent<SoundManager>().PauseAudio("Countdown");
        }

        else if(Input.GetKeyDown(KeyCode.Escape) && ESCPressed)
        {
            OnEditMode(false);

            GameObject.Find("Sounds").GetComponent<SoundManager>().PlayAudio("IngameBGM");
            GameObject.Find("Sounds").GetComponent<SoundManager>().PlayAudio("Countdown");
        }
    }

    public void OnEditMode(bool isModeOn)
    {
        if(isModeOn)
        {
            ESCPressed = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            controllActivate = false;
        }
        else
        {
            ESCPressed = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            controllActivate = true;
        }
    }


    void PlayHeartBeatSound()
    {
        Collider[] sensedEnemise = Physics.OverlapSphere(transform.position, heartbeatRange, m_PoliceLayer);
        if(GameManager.instance.playersTeam == Player.Team.thief
        && sensedEnemise.Length > 0)
        {
            if(!audio_Hearbeat.isPlaying)
                audio_Hearbeat.Play();
        }
        else 
        {
            audio_Hearbeat.Pause();
        }
    }

    /*      ===         Externally executed functions         === */

    public void TurnOnSpotLight()
    {
        switch(team)
        {
        case Team.police:
            lightOfPolice.SetActive(true);
            lightOfThief.SetActive(false);
            break;
        
        case Team.thief:
            lightOfThief.SetActive(true);
            lightOfPolice.SetActive(false);
            break;
        }
    }

    public void ActivatePlayersControll()
    {
        controllActivate = true;
    }
}
