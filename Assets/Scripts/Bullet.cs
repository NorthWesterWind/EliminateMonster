using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    //�ӵ�������Ч
    public ParticleSystem explosioneffect;
    //�ӵ���β��Ч
    public ParticleSystem bulletTrailEff;
    public float destoryTime;  //����ʱ��
    private float passTime;    //�ӵ������׷����˶���ʱ��

    public bool isDestory = false;

    //���ױ�ը�˺��뾶
    public float radio;
    //��ըʱ��⵽������
    public Collider[] colliders;

    public float atkvalue = 4;

    //�ӵ�����������Ҫ�仯��С
    public ParticleSystem bulletSizeParticle;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        destoryTime = 2.5f;


        colliders = new Collider[10];
        if (bulletTrailEff != null)
        {
            bulletTrailEff.time = 0;
            bulletTrailEff.Play();
        }
    }

    private void Start()
    {
        if(explosioneffect != null)
        {
            PoolManager.Instance.Init(explosioneffect, 4);
        }
       
      

    }
    // Update is called once per frame
    void Update()
    {
            passTime += Time.deltaTime;
            if (passTime >= destoryTime)
            {
                if (isDestory)
                {
                    //��Ϊ��ͨ�ӵ�ʱ
                    BulletOnHitDestory();
                }
                else
                {
                    //��������ʱ
                    BulletDestory();
                }
            }
        
        
    }


    /// <summary>
    /// �ӵ�����
    /// </summary>
    /// <param name="launcher">�����ӵ�������</param>
    /// <param name="direction">����ķ���</param>
    /// <param name="force">���ӵ�ʩ�ӵ���</param>
    public void Launch(Weapon launcher , Vector3 direction , float force )
    {
        transform.position = launcher.GetPoint().position;
        transform.forward = launcher.GetPoint().forward;
        rb.AddForce(direction * force);
    }


    /// <summary>
    /// �����߼�
    /// </summary>
    private void BulletDestory()
    {
        if(explosioneffect != null)
        {
            ParticleSystem effobj = PoolManager.Instance.GetInstance<ParticleSystem>(explosioneffect);
            effobj.transform.position = transform.position;
            effobj.gameObject.SetActive(true);
            effobj.time = 0;
            effobj.Play();
        }
      
        gameObject.SetActive(false);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        passTime = 0;
        int count = Physics.OverlapSphereNonAlloc(transform.position, radio, colliders, 1 << LayerMask.NameToLayer("Monster"));
        for (int i = 0; i < count; i++)
        {
           Monster monster = colliders[i].GetComponent<Monster>();
            monster.Damage(atkvalue);
        }
        int Bosscount = Physics.OverlapSphereNonAlloc(transform.position, radio, colliders, 1 << LayerMask.NameToLayer("Boss"));
        if( Bosscount > 0)
        {
            Boss monster = colliders[0].GetComponentInParent<Boss>();
            monster.Damage(atkvalue, transform.position);
        }
       
    }
    private void BulletOnHitDestory( GameObject m = null)
    {
        if(explosioneffect != null)
        {
            ParticleSystem effobj = PoolManager.Instance.GetInstance<ParticleSystem>(explosioneffect);
            effobj.transform.position = transform.position;
            effobj.gameObject.SetActive(true);
            effobj.time = 0;
            effobj.Play();
        }
      
        gameObject.SetActive(false);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        passTime = 0;
        TakeDamage(m ,gameObject.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestory)
        {
            BulletOnHitDestory(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDestory)
        {
            BulletOnHitDestory(other.gameObject);
        }
    }


    public void SetAtkValue(float value)
    {
        atkvalue = value;
    }

    /// <summary>
    /// ���þ���ǹ�ӵ���С
    /// </summary>
    public void SetBulletSize(float size)
    {
        bulletSizeParticle.startSize = size;
    }

    public void TakeDamage(GameObject m , Vector3 position)
    {
        if(m != null)
        {
            if(m.layer == 6)
            {
                 m.GetComponent<Monster>().Damage(atkvalue);
            }
            else if(m.layer == 7)
            {
                 m.GetComponentInParent<Boss>().Damage(atkvalue , position);
            }
        }
    }
}
