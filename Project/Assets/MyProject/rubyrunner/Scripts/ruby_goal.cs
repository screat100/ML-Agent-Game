using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ruby_goal : MonoBehaviour
{
    [SerializeField]
    ruby_AreaSetting m_areaSetting;

    [System.NonSerialized]
    public bool flagGoal;// Goal 이 활성화가 됐는가?
    
    Renderer m_render;
    public Material OnMaterial;
    public Material OffMaterial;
    void Start()
    {
        flagGoal=false;
        m_render=gameObject.GetComponent<MeshRenderer>();
        m_render.material=OffMaterial;
    }
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "runner"&&m_areaSetting.findruby&&flagGoal&other.gameObject.GetComponent<ruby_runner>().hasruby)
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
        gameObject.layer=6; //"goal"레이어 설정
    }
    public void Goal_reset()
    {
        flagGoal=false;
        m_render.material=OffMaterial;
        gameObject.tag="wall";
        gameObject.layer=0;
    }
}
