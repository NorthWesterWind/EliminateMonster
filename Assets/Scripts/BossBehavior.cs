using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{
    
    //Boss����
    public GameObject BossWeapon;
    //��ɫλ��
    public Transform playerPosition;
    //�ͷż��ܵ�����
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
    //��ʾ����
    private void ShowWeapon()
    {
        BossWeapon.gameObject.SetActive(true);
    }

    /// <summary>
    /// ��������
    /// </summary>
    private void HideWeapon()
    {
        BossWeapon.gameObject.SetActive(false);
    }



    //ʹ�ü���
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
