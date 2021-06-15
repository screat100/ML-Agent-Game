using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rubytrigger : MonoBehaviour
{
    public StageSetting m_areaSetting;
    private Vector3 dropSector;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision other)
    /*
        루비에 충돌 시, 해당 러너 루비 흭득
    */
    {
        if(other.gameObject.tag=="thief")
        {
            if (other.gameObject.name == "Player")
                other.gameObject.GetComponent<Player>().hasruby = true;
            else
                other.gameObject.GetComponent<ThiefAgent>().hasruby = true;

            m_areaSetting.findruby=true;
            for(int i=0;i<m_areaSetting.thiefAgents.Length;i++){
                if(m_areaSetting.thiefAgents[i]==other.gameObject){
                    m_areaSetting.key_player=i;
                }
            }
            gameObject.SetActive(false);
            m_areaSetting.random_goal(); //랜덤한 위치에 Goal 활성화
            m_areaSetting.Reward_RubyGet(); //루비흭득에 따른 보상
        }
    }
    public void resetPlace(int spawnSector){
        gameObject.SetActive(true);

        var spawnTransform = m_areaSetting.Ruby_Spawn[spawnSector].transform;
        var xRange = spawnTransform.localScale.x / 2.1f;
        var zRange = spawnTransform.localScale.z / 2.1f;

        transform.position = new Vector3(Random.Range(-xRange, xRange), 0.5f, Random.Range(-zRange, zRange))
            + spawnTransform.position;
        dropSector = transform.position;
    }
    public void droptheRuby()
    {
        gameObject.SetActive(true);
        transform.position = dropSector;
    }
}
