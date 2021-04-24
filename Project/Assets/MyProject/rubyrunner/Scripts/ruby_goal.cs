using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ruby_goal : MonoBehaviour
{
    [SerializeField]
    ruby_AreaSetting m_areaSetting;

    [System.NonSerialized]
    public bool flagGoal;
    
    Renderer m_render;
    public Material OnMaterial;
    public Material OffMaterial;
    void Start()
    {
        flagGoal=false;
        m_render=gameObject.GetComponent<MeshRenderer>();
        m_render.material=OffMaterial;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "runner"&&m_areaSetting.findruby&&flagGoal)
        {
            other.gameObject.SetActive(false);
            m_areaSetting.Scored(other.gameObject, false); 
        }
    }
    public void select_finishGoal()
    {
        flagGoal=true;
        m_render.material=OnMaterial;
        gameObject.tag="goal";
    }
    public void Goal_reset()
    {
        flagGoal=false;
        m_render.material=OffMaterial;
        gameObject.tag="wall";
    }
}
