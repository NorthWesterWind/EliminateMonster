using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{
    
    //Boss武器
    public GameObject BossWeapon;
    //角色位置
    public Transform playerPosition;
    //释放技能的武器
    public GameObject SkillWeapon;

    public Transform attackTrans;

    private Animator animator;

    // Start is called before the first frame update

    private void Awake()
    {
        animator = GetComponent<Animator>();    
    }

    void Start()
    {
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform;
        if(SkillWeapon != null)
        {
            PoolManager.Instance.Init(SkillWeapon, 10);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //显示武器
    private void ShowWeapon()
    {
        BossWeapon.gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏武器
    /// </summary>
    private void HideWeapon()
    {
        BossWeapon.gameObject.SetActive(false);
    }



    //使用技能
    private void CreateSkill()
    {

        BossWeapon.gameObject.SetActive(false);
        GameObject ball = PoolManager.Instance.GetInstance<GameObject>(SkillWeapon);
        ball.gameObject.SetActive(true);
        ball.transform.SetParent(null);
        ball.transform.position = attackTrans.position;
        ball.GetComponent<BossWeapon>().targetPos = playerPosition.position;
   
    }

    private void FinishBorn()
    {
        animator.SetFloat("Born", -0.5f);
    }
}
