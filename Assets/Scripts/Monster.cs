using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
/// <summary>
/// 怪物状态
/// </summary>
public enum E_MONSTERSATUS
{
    PATROL,
    CHASE,
    ATTACK,
    RETURN,
    IDLE,
    WARN,
    USESKILL
}
[Serializable]
public struct MonsterFunction
{
    public bool canRotateX;
    public bool canRotateY;
    public bool canRotateZ;

    public bool canPatrol;
    public bool canChase;
    public bool canAttack;
    public bool canReturn;
}

public class Monster : MonoBehaviour
{
    //血量
    public float initHealth = 10;
    //实时血量
    protected float currentHealth;

    public ParticleSystem effdestory;

    public E_MONSTERSATUS e_MONSTERSATUS = E_MONSTERSATUS.PATROL;

    public float moveSpeed;

    //记录怪物运动的时间间隔·
    protected float lastTime;
    public float actresttime;

    protected Quaternion targetRotation;
    //怪物初始位置
    protected Vector3 startpos;

    //对怪物进行单独移动方向控制
    public MonsterFunction monsterFunction;

    //玩家位置信息
    protected Transform playerTrans;

    public float attackRange;
    public float chaseRange;
    public float safeRange;

    //自身刚体组件
    protected Rigidbody rigidbody;

    protected float lastAtkTime;

    
    protected float initChaseRange;
    protected float initSafeRange;
    protected float initAttackRange;

    //烟雾检测器
    public GameObject SmokeCollider;

    protected virtual void Start()
    {
        currentHealth = initHealth;
        startpos = transform.position;

        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

        rigidbody = GetComponent<Rigidbody>();

        initChaseRange = chaseRange;
        initSafeRange = safeRange;
        initAttackRange = attackRange;
    }

   protected virtual  void Update()
    {
        MonsterAction();
        CheckDistance();
    }

    /// <summary>
    /// 怪物状态监测
    /// </summary>
    protected virtual void MonsterAction()
    {
        switch (e_MONSTERSATUS)
        {
            case E_MONSTERSATUS.PATROL:
                Move();
                if(Time.time- lastTime > actresttime)
                {
                    //改变怪物方向
                    targetRotation = Quaternion.Euler(GetRandomEuler());
                    lastTime = Time.time;
                }
                break;
            case E_MONSTERSATUS.CHASE:
                Chase();
                break;
            case E_MONSTERSATUS.ATTACK:
                Attack();
                break;
            case E_MONSTERSATUS.RETURN:
                ReturnStartpos();
                break;
                default:
                break;
        }
    }

    /// <summary>
    /// 警戒状态时移动逻辑
    /// </summary>
    public void Move()
    {
        transform.Translate(transform.forward * Time.deltaTime * moveSpeed, Space.World);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
    }

    /// <summary>
    /// 返回初始位置时移动逻辑
    /// </summary>
    protected virtual void ReturnStartpos()
    {
        transform.Translate(transform.forward * Time.deltaTime * moveSpeed, Space.World);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        if(Vector3.Distance(transform.position , startpos) <= safeRange)
        {
            e_MONSTERSATUS = E_MONSTERSATUS.PATROL;
        }
    }
    /// <summary>
    /// 怪物追逐逻辑
    /// </summary>
    public virtual  void Chase()
    {
        transform.LookAt(playerTrans);
        transform.Translate(transform.forward * Time.deltaTime * moveSpeed, Space.World);
    }

    /// <summary>
    /// 怪物攻击逻辑
    /// </summary>
    public virtual void Attack()
    {
       
    }



    /// <summary>
    /// 监测怪物位置
    /// </summary>
    protected  virtual void CheckDistance()
    {
        if(e_MONSTERSATUS == E_MONSTERSATUS.RETURN)
        {
            return;
        }
        if(Vector3.Distance(playerTrans.position, transform.position) <= attackRange && monsterFunction.canAttack)
        {
            e_MONSTERSATUS = E_MONSTERSATUS.ATTACK;
        }
        else if (Vector3.Distance(playerTrans.position, transform.position) <= chaseRange && monsterFunction.canChase)
        {
            e_MONSTERSATUS = E_MONSTERSATUS.CHASE;
        }
        else if(Vector3.Distance(playerTrans.position, transform.position) >= safeRange && monsterFunction.canReturn)
        {
            if(e_MONSTERSATUS == E_MONSTERSATUS.PATROL && Vector3.Distance(transform.position, startpos) >= safeRange)
            {
                e_MONSTERSATUS = E_MONSTERSATUS.RETURN;
                targetRotation = Quaternion.LookRotation(startpos - transform.position, transform.up);
            }
            else if(e_MONSTERSATUS == E_MONSTERSATUS.CHASE)
            {
                e_MONSTERSATUS = E_MONSTERSATUS.RETURN;
                targetRotation = Quaternion.LookRotation(startpos - transform.position, transform.up);
            }
        }
    }

    /// <summary>
    /// 用于返回一个随机的欧拉角
    /// </summary>
    /// <returns></returns>
    public  Vector3 GetRandomEuler()
    {
        float x = 0 , y = 0 , z = 0 ;
        if (monsterFunction.canRotateX)
        {
            x = Random.Range(1, 5) * 90;
        }
        if (monsterFunction.canRotateY)
        {
            y = Random.Range(1, 5) * 90;
        }
        if (monsterFunction.canRotateZ)
        {
           z = Random.Range(1, 5) * 90;
        }
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 怪物受伤
    /// </summary>wdw
    /// <param name="damageValue"></param>
    public  void Damage(float damageValue)
    {
        if (effdestory != null)
        {
            PoolManager.Instance.Init(effdestory, 4);
        }

        currentHealth -= damageValue;
        RecoverAttackRangeValue();
        if(currentHealth > 0)
            return;
        if (effdestory!=null)
        {
            ParticleSystem particle = PoolManager.Instance.GetInstance<ParticleSystem>(effdestory);
            particle.time = 0;
            particle.Play();
            particle.transform.position = transform.position;
        }
        gameObject.SetActive(false);
    }

   protected virtual  void OnCollisionEnter(Collision collision)
    {

        if(e_MONSTERSATUS == E_MONSTERSATUS.PATROL)
        {
            targetRotation = Quaternion.LookRotation(-transform.forward , transform.up);
            lastTime = Time.time;
        }
        if(e_MONSTERSATUS == E_MONSTERSATUS.CHASE || e_MONSTERSATUS == E_MONSTERSATUS.ATTACK || e_MONSTERSATUS == E_MONSTERSATUS.RETURN)
        {
            //当怪物处于追赶 、攻击 、返回 时
            //避免收到障碍阻挡
            rigidbody.isKinematic = true;

            //延迟1.5f执行
            Invoke("CloseIsKinematicState", 1.5f);
        }
    }

    /// <summary>
    /// 让怪物受到力的影响
    /// </summary>
    private void CloseIsKinematicState()
    {
        rigidbody.isKinematic=false;
    }

    public void SetRange(float range)
    {
        attackRange = chaseRange = range;
        e_MONSTERSATUS = E_MONSTERSATUS.IDLE;
    }

    public void RecoverAttackRangeValue()
    {
        attackRange = initAttackRange;
        chaseRange = initChaseRange;
    }

    public void SetSmokeCollierState(bool colliderState)
    {
        SmokeCollider.gameObject.SetActive(colliderState);
    }
}
