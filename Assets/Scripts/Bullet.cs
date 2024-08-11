using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    //子弹击中特效
    public ParticleSystem explosioneffect;
    //子弹拖尾特效
    public ParticleSystem bulletTrailEff;
    public float destoryTime;  //销毁时间
    private float passTime;    //子弹或手雷发射了多少时间

    public bool isDestory = false;

    //手雷爆炸伤害半径
    public float radio;
    //爆炸时检测到的物体
    public Collider[] colliders;

    public float atkvalue = 4;

    //子弹是粒子且需要变化大小
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
                    //作为普通子弹时
                    BulletOnHitDestory();
                }
                else
                {
                    //特殊作用时
                    BulletDestory();
                }
            }
        
        
    }


    /// <summary>
    /// 子弹发射
    /// </summary>
    /// <param name="launcher">发射子弹的武器</param>
    /// <param name="direction">发射的方向</param>
    /// <param name="force">给子弹施加的力</param>
    public void Launch(Weapon launcher , Vector3 direction , float force )
    {
        transform.position = launcher.GetPoint().position;
        transform.forward = launcher.GetPoint().forward;
        rb.AddForce(direction * force);
    }


    /// <summary>
    /// 销毁逻辑
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
    /// 设置聚能枪子弹大小
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
