using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    /*      ===         Functions         === */

    void Start()
    {
        controllActivate = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        m_Animator = gameObject.GetComponent<Animator>();
        m_Rigidbody = gameObject.GetComponent<Rigidbody>();
        m_Camera = transform.Find("Main Camera").gameObject;

        m_StageSetting = GameObject.Find("GameArea").GetComponent<StageSetting>();

        switch(GameManager.playersTeam)
        {
            case Team.police:
                team = Team.police;
                break;
            case Team.thief:
                team = Team.thief;
                break;
        }

        TurnOnSpotLight();

        if(GameManager.playersTeam == Player.Team.thief)
        {
            gameObject.transform.position = GameObject.Find("GameArea").GetComponent<StageSetting>().thiefAgents[GameManager.thiefNum-1].transform.position;
        }
        else
        {
            gameObject.transform.position = GameObject.Find("GameArea").GetComponent<StageSetting>().policeAgents[GameManager.policeNum-1].transform.position;
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
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (transform.tag == "police" && collision.transform.tag == "thief")
        {
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
        if(GameManager.playersTeam == Player.Team.thief
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
