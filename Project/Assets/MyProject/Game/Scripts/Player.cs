using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /*      ===         Sub Classes         === */

    public enum Team
    {
        police,
        thief,
    }


    /*      ===         variables         === */

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    GameObject m_Camera;

    public Team team;
    public float movePower = 1.0f;

    float AngleX;
    float AngleY;
    [Range(0.1f, 10.0f)] public float mouseSensitive = 1.0f;



    /*      ===         Functions         === */

    void Start()
    {
        m_Animator = gameObject.GetComponent<Animator>();
        m_Rigidbody = gameObject.GetComponent<Rigidbody>();
        m_Camera = transform.Find("Main Camera").gameObject;

        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        KeyboardMove();
        MouseMove();
    }


    /*      ===         functions that run on Start()         === */



    /*      ===         functions that run on Update()         === */

    void KeyboardMove()
    {
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
        
        else if (forwardMove == 0 && rightwardMove == 0)
        {
            m_Rigidbody.velocity = Vector3.zero;
        }

        //transform.position +=
        //    transform.forward * forwardMove * movePower
        //    + transform.right * rightwardMove * movePower;

        m_Rigidbody.AddForce(transform.forward * forwardMove * movePower * Time.deltaTime);
        m_Rigidbody.AddForce(transform.right * rightwardMove * movePower * Time.deltaTime);

    }

    void MouseMove()
    {
        AngleX += Input.GetAxis("Mouse X") * mouseSensitive;
        AngleY += Input.GetAxis("Mouse Y") * mouseSensitive;


        // y축을 상하 90도(=180도)로 제한
        if (AngleY >= 90)
            AngleY = 90;

        if (AngleY <= -90)
            AngleY = -90;


        gameObject.transform.eulerAngles = new Vector3(0, AngleX, 0.0f);
        m_Camera.transform.eulerAngles = new Vector3(AngleY, AngleX, 0.0f);
    }



}
