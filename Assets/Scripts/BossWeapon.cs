using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Boss�����ű�
/// </summary>
public class BossWeapon : MonoBehaviour
{
    //�˺�ֵ
    public float damageValue = 10;
    //�����Ľ�ɫ�ű�
    private PlayerController playerController;
    //�ɷ����������ƶ��ٶ�
    public float moveSpeed = 5;
  //  public Vector3 targetPos;
    //��������ʱ��
    private float destoryTime = 10;
    //��ʱ��
    private float timeVal;
    //�������ʱ���ŵ���Ч
    public ParticleSystem destoryEffect;

    //ֻ��ʹ��Զ������ʱ�����������ƶ�
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
        //���ܼ�ʱ��
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
