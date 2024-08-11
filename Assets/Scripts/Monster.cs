using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
/// <summary>
/// ����״̬
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
    //Ѫ��
    public float initHealth = 10;
    //ʵʱѪ��
    protected float currentHealth;

    public ParticleSystem effdestory;

    public E_MONSTERSATUS e_MONSTERSATUS = E_MONSTERSATUS.PATROL;

    public float moveSpeed;

    //��¼�����˶���ʱ������
    protected float lastTime;
    public float actresttime;

    protected Quaternion targetRotation;
    //�����ʼλ��
    protected Vector3 startpos;

    //�Թ�����е����ƶ��������
    public MonsterFunction monsterFunction;

    //���λ����Ϣ
    protected Transform playerTrans;

    public float attackRange;
    public float chaseRange;
    public float safeRange;

    //����������
    protected Rigidbody rigidbody;

    protected float lastAtkTime;

    
    protected float initChaseRange;
    protected float initSafeRange;
    protected float initAttackRange;

    //��������
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
    /// ����״̬���
    /// </summary>
    protected virtual void MonsterAction()
    {
        switch (e_MONSTERSATUS)
        {
            case E_MONSTERSATUS.PATROL:
                Move();
                if(Time.time- lastTime > actresttime)
                {
                    //�ı���﷽��
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
    /// ����״̬ʱ�ƶ��߼�
    /// </summary>
    public void Move()
    {
        transform.Translate(transform.forward * Time.deltaTime * moveSpeed, Space.World);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
    }

    /// <summary>
    /// ���س�ʼλ��ʱ�ƶ��߼�
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
    /// ����׷���߼�
    /// </summary>
    public virtual  void Chase()
    {
        transform.LookAt(playerTrans);
        transform.Translate(transform.forward * Time.deltaTime * moveSpeed, Space.World);
    }

    /// <summary>
    /// ���﹥���߼�
    /// </summary>
    public virtual void Attack()
    {
       
    }



    /// <summary>
    /// ������λ��
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
    /// ���ڷ���һ�������ŷ����
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
    /// ��������
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
            //�����ﴦ��׷�� ������ ������ ʱ
            //�����յ��ϰ��赲
            rigidbody.isKinematic = true;

            //�ӳ�1.5fִ��
            Invoke("CloseIsKinematicState", 1.5f);
        }
    }

    /// <summary>
    /// �ù����ܵ�����Ӱ��
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
