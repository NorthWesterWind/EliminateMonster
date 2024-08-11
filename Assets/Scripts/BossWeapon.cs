using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Boss武器脚本
/// </summary>
public class BossWeapon : MonoBehaviour
{
    //伤害值
    public float damageValue = 10;
    //关联的角色脚本
    private PlayerController playerController;
    //可飞行武器的移动速度
    public float moveSpeed = 5;
  //  public Vector3 targetPos;
    //武器销毁时间
    private float destoryTime = 10;
    //计时器
    private float timeVal;
    //击中玩家时播放的特效
    public ParticleSystem destoryEffect;

    //只有使用远程武器时，武器才能移动
    public bool canMove;

    public Vector3 targetPos;


    private bool canTakeDamage;

    private float DamageTime;
    public float initDamageTime = 2;

    public bool isSmoke;

    

    // Start is called before the first frame update
    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
       
        if(destoryEffect != null)
        {
            PoolManager.Instance.Init(destoryEffect, 10);
        }
        DamageTime = initDamageTime;
        if (canMove)
        {
            transform.LookAt(targetPos);
        }
    }

    void Update()
    {
        //技能计时器
        if(DamageTime <= 0)
        {
            canTakeDamage = true;
            DamageTime = initDamageTime;

        }
        else
        {
            DamageTime -= Time.deltaTime;
        }


        if (canMove)
        {
            
            transform.Translate(transform.forward * 3 * moveSpeed * Time.deltaTime , Space.World);
            if (timeVal >= destoryTime)
            {
                gameObject.SetActive(false);
                gameObject.transform.SetParent(PoolManager.Instance.transform);
                timeVal = 0;
            }
            else
            {
                timeVal += Time.deltaTime;
            }
        }
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            playerController.TakeDamage(damageValue);
            if(destoryEffect != null)
            {
                ParticleSystem particleSystem = PoolManager.Instance.GetInstance<ParticleSystem>(destoryEffect);
                particleSystem.gameObject.SetActive(true);
                particleSystem.transform.position = transform.position;
                particleSystem.time = 0;
                particleSystem.Play();
            }
            if (canMove)
            {
                gameObject.SetActive(false);
                gameObject.transform.SetParent(PoolManager.Instance.transform);
            }
            if (isSmoke)
            {
                if (canTakeDamage)
                {
                    playerController.TakeDamage(damageValue);
                    canTakeDamage = false;
                    DamageTime = initDamageTime;
                }
            }
            else
            {
                playerController.TakeDamage(damageValue);
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (canTakeDamage && other.tag == "Player")
        {
            playerController.TakeDamage(damageValue);
            canTakeDamage = false;
            DamageTime = initDamageTime;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            canTakeDamage = true;
            DamageTime = initDamageTime;
        }    
    }
}
