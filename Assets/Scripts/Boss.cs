using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor.Timeline;
using UnityEngine;

public class Boss : Monster
{

    //BOSS����������
    protected Animator animator;
    //boss����״̬
    protected bool isWaking;
    //boss����״̬
    protected bool isDead;

    private bool isBorn;
    private bool isreturn;
    private bool canRoar = true;

    public Light pointlight;
    //��ŭ״̬
    private bool isAngary;
    //�Ƿ�ʹ�ü���
    public bool hasSkill;

    

    //������Կ��
    public GameObject Key;

    //���˼�ʱ��
    public float takeDamageTime;
    private float takeDamageTimer;

    // Start is called before the first frame update
   protected override  void Start()
    {
        base.Start();
        animator = GetComponentInChildren<Animator>();
        animator.SetFloat("Born", -0.5f);

        initChaseRange = chaseRange;
        initSafeRange = safeRange;
        rigidbody = GetComponent<Rigidbody>();
        takeDamageTimer = takeDamageTime;
    }

    /// <summary>
    /// ���״̬
    /// </summary>
    protected override void MonsterAction()
    {
       // base.MonsterAction();
        if(isWaking || isDead)
            return;
        
        switch (e_MONSTERSATUS)
        {
            case E_MONSTERSATUS.PATROL:
                break;
            case E_MONSTERSATUS.CHASE:
                animator.SetBool("Move", true);
               // animator.ResetTrigger("Attack");
                if (isAngary)
                {
                    animator.SetFloat("Movespeed", 1);
                    moveSpeed = 4;
                }
                Chase();
                break;
            case E_MONSTERSATUS.RETURN:
                animator.SetBool("Move", true);
                animator.ResetTrigger("Hit");
                ReturnStartpos();
                isreturn = true;
                break;
            case E_MONSTERSATUS.ATTACK:
                animator.SetBool("Move" , false);
                animator.ResetTrigger("Hit");
                if (hasSkill)
                {
                    animator.SetTrigger("UseSkill");
                }
                else
                {
                    animator.SetTrigger("Attack");
                }
                
                Attack();
                break;
            case E_MONSTERSATUS.IDLE:
               
                animator.SetBool("Move", false);
      
                break;
            case E_MONSTERSATUS.WARN:
               
                break;
            case E_MONSTERSATUS.USESKILL:
                break;
                default:
                break;

        }
 
    }
   

    /// <summary>
    /// boss״̬�л�
    /// </summary>
    protected override void CheckDistance()
    {
        //�����ڷ���״̬ �� ����״̬ �� ����״̬
        if (e_MONSTERSATUS == E_MONSTERSATUS.RETURN || isWaking || isDead)
        {
            return;
        }
        //�õ�����ͽ�ɫ�ľ�����Ϣ
        float distance = Vector3.Distance(playerTrans.position, transform.position);
        if(distance < 2 * chaseRange && e_MONSTERSATUS != E_MONSTERSATUS.WARN &&
            e_MONSTERSATUS != E_MONSTERSATUS.ATTACK && e_MONSTERSATUS != E_MONSTERSATUS.CHASE )
        {
            Warn();
            e_MONSTERSATUS = E_MONSTERSATUS.WARN;

        }

        if (distance <= attackRange && monsterFunction.canAttack)
        {
            e_MONSTERSATUS = E_MONSTERSATUS.ATTACK;
        }
        else if (distance <= chaseRange && monsterFunction.canChase)
        {
            if (canRoar)
            {
                transform.LookAt(new Vector3(playerTrans.position.x, transform.position.y, playerTrans.position.z));
                animator.SetTrigger("Roar");
            }
            canRoar = false;
            e_MONSTERSATUS = E_MONSTERSATUS.CHASE;
        }
        else if (distance >= safeRange && monsterFunction.canReturn)
        {
           if(e_MONSTERSATUS == E_MONSTERSATUS.CHASE && monsterFunction.canChase)
            {
                e_MONSTERSATUS = E_MONSTERSATUS.RETURN;
            }
        }
    }
    /// <summary>
    /// ��ɫ����Bossʱ�����뾯��״̬
    /// </summary>
    protected void Warn()
    {
        float wakeValue = animator.GetFloat("Born");
        if (wakeValue < 0)
        {
            if(isBorn == false)
            {
                animator.SetFloat("Born", 0.5f);
            }
            
            isWaking = true;
            //ʹBossֻ��һ�������޵�״̬
            Invoke("FinishWaking", 6);
            isBorn = true;
        }
       
    }

    

    private void FinishWaking()
    {
        animator.ResetTrigger("Hit");
        isWaking = false;
    }

    #region ׷�𡢹���������

    /// <summary>
    /// Boss׷��
    /// </summary>
    public override void Chase()
    {
      if(!CanMove())
            return;
        transform.LookAt( new Vector3( playerTrans.position.x , transform.position.y , playerTrans .position.z ));
        transform.Translate(transform.forward * Time.deltaTime * moveSpeed, Space.World);
    }

    /// <summary>
    /// ��д��������
    /// </summary>
    public override void Attack()
    {
       
        if (Time.time - lastAtkTime < actresttime)
        { 
            return;
        }
        else
        {
            lastAtkTime = Time.time;
            transform.LookAt(new Vector3(playerTrans.position.x , transform.position.y , playerTrans .position.z ));
        }
    }

    /// <summary>
    /// Boss���س�����
    /// </summary>
    protected override void ReturnStartpos()
    {
        currentHealth = initHealth;
        animator.SetFloat("Movespeed", 0);
        moveSpeed /= 1.5f;
        if (Vector3.Distance(transform.position , startpos) <= 3)
        {
            e_MONSTERSATUS = E_MONSTERSATUS.IDLE;
            isAngary = false;
            transform.eulerAngles = new Vector3(0, 180, 0);
            
            
            chaseRange = initChaseRange;
            safeRange = initSafeRange;
            canRoar = true;
            return;
        }
        transform.Translate(transform.forward * Time.deltaTime * moveSpeed , Space.World);
        transform.LookAt(new Vector3(startpos.x, transform.position.y, startpos.z));
       
    }
    #endregion


    #region  ����

    /// <summary>
    /// Boss�����߼�
    /// </summary>
    /// <param name="damageValue"></param>
    public void Damage(float damageValue , Vector3 hitpos)
    {
        if (isreturn || isWaking || isDead)
            return;
        currentHealth -= damageValue;
        base.RecoverAttackRangeValue();
        if (currentHealth <= initHealth / 3)
            isAngary = true;

        if(e_MONSTERSATUS == E_MONSTERSATUS.IDLE || e_MONSTERSATUS == E_MONSTERSATUS.WARN)
        {
            chaseRange = initChaseRange * 2;
            safeRange = initSafeRange * 2;
        }

        if(takeDamageTimer <= 0)
        {
            #region  Hit
            animator.SetTrigger("Hit");
            animator.SetFloat("HitX", 0);
            animator.SetFloat("HitY", 0);
            float y = Vector3.Dot(transform.forward, hitpos - transform.position);
            float x = Vector3.Dot(transform.forward, hitpos);
            if (ForwardBehindOrLeftRight(hitpos))
            {
                if (y > 0)
                    animator.SetFloat("HitY", 1);
                else
                    animator.SetFloat("HitY", -1);
            }
            else
            {
                if (x > 0)
                    animator.SetFloat("HitX", 1);
                else
                    animator.SetFloat("HitX", -1);
            }
            #endregion
            takeDamageTimer = takeDamageTime;
        }



        if (currentHealth > 0)
        {
            return;
        }
        animator.SetTrigger("Die");
        isDead = true;
        Key.SetActive(true);
        Key.transform.position = transform.position + new Vector3(0,2,0);
        rigidbody.isKinematic = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }
    
    /// <summary>
    /// �����ж�BOSS�ܵ��ĸ�����Ĺ���
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    private bool ForwardBehindOrLeftRight(Vector3 targetPos)
    {
        float Z = Mathf.Abs(transform.position.z - targetPos.z);
        float X = Mathf.Abs(transform.position.x - targetPos.x);
        if(Z > X)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    #endregion

    // Update is called once per frame
    protected override void Update()
    {
        if (isDead)
        {
            //boss����  ��ʬ�建���³�
            transform.Translate(-Vector3.up * Time.deltaTime  * 0.3f);
            pointlight.intensity -= Time.deltaTime * 2;
              if (transform.position.y <= -10)
            {
                Destroy(gameObject);
            }
            return;
        }

        takeDamageTimer -= Time.deltaTime;

        MonsterAction();
        CheckDistance();

    }

    protected bool CanMove()
    {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
            Animator.StringToHash("Attack") &&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
            Animator.StringToHash("Hit") &&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
            Animator.StringToHash("Roar")&&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
           Animator.StringToHash("UseSkill");
    }


    protected override void OnCollisionEnter(Collision collision)
    {
        if (e_MONSTERSATUS == E_MONSTERSATUS.CHASE || e_MONSTERSATUS == E_MONSTERSATUS.ATTACK || e_MONSTERSATUS == E_MONSTERSATUS.RETURN)
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
        rigidbody.isKinematic = false;
    }
}
