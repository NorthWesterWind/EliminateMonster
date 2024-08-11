using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// ��������
/// </summary>
public enum E_WEAPONTYPE
{
    RAYCAST,
    BULLET
}

/// <summary>
/// ����״̬
/// </summary>
public enum E_WEAPONSTAUS
{
    IDLE,
    FIRING,
    RELOADING
}

/// <summary>
/// ��������ģʽ
/// </summary>
public enum E_WEAPONMODE
{
    NORMAL,
    AUTO,
    ACCUMULATION
}

/// <summary>
/// ������β��Ϣ��
/// </summary>
public class ActiveTrail
{
    public LineRenderer line;
    public Vector3 direction;
    public float remainTime;
}

/// <summary>
/// ����������չ
/// </summary>
[Serializable]
public class AdvancedWeaponSetting
{   
    //�ӵ�ƫ�Ƴ̶�
    public float spreadAngle;
    //������ʱ��
    public float shakerTime;
    //�����𶯷���
    public float shakerStrength;
}

public class Weapon : MonoBehaviour
{
    //����ӵ����
    private PlayerController owner;
    //����ID
    public int itemID = -1;
    //������
    public int initAmount = -1;
    //ǹе����������
    private Animator animator;

    //ǹ��
    public LineRenderer rayTrailPrefab;

    //ǹ��
    public Transform shootPoint;
    
    public E_WEAPONTYPE weaponType;
    //������Ϣ������
    public List<ActiveTrail> activeTrails;
    //�ӵ��ű�Ԥ����
    public Bullet bulletPrefab;
    //���ӵ�ʩ�ӵ���
    public  float bulletforce = 600;
    //��������
    public int bulletSize ;
    //����ʣ���ӵ���
    public int remainSize;

    public AdvancedWeaponSetting advancedWeaponSetting;
    //����״̬
    private E_WEAPONSTAUS currentWeaponSatus;

    public E_WEAPONMODE e_WEAPONMODE;


    private int fireHashValue;
    private int reloadHashValue;

    //����ʱ��
    public float reloadtime;
    //����Ƶ��
    public float firetime;
    //CD
    private float cdtime ;
    //���𶯻���Ƭ
    public AnimationClip fireanimationClip;
    //����������Ƭ
    public AnimationClip reloadinganimationClip;

    //������
    public float atkValue;
    //���⽦����ЧԤ����
    public ParticleSystem raycastHitEffectPrefab;

    //��������
    public bool HasView;
    public TelesopicView telesopicView;

    //����ǹ�ӵ���С
    private float ShapebulletSize;
    //����ǹ��Ч
    public ParticleSystem ShapeViewEffect;
    //����ǹʵʱ������
    private float currentShapeAtkValue;

    public float decreaseSpeed;
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();    
        if(rayTrailPrefab != null)
        {
            //��ǰ���ɼ���Ԥ����
            PoolManager.Instance.Init(rayTrailPrefab,8);
        }
        
        activeTrails = new List<ActiveTrail>();

        currentWeaponSatus = E_WEAPONSTAUS.IDLE;

        fireHashValue = Animator.StringToHash("Fire");
        reloadHashValue = Animator.StringToHash("Reload");
        remainSize = bulletSize;

        if (bulletPrefab != null)
        {
            PoolManager.Instance.Init(bulletPrefab, 4);
        }
        if (raycastHitEffectPrefab != null)
        {
            PoolManager.Instance.Init(raycastHitEffectPrefab, 8);
        }
        if (HasView)
        {
            telesopicView = Camera.main.GetComponent<TelesopicView>();
        }

    }

    private void Start()
    {
        
    }
    void Update()
    {
        UpdateController();
        if (weaponType == E_WEAPONTYPE.RAYCAST)
        {
            UpdateTrailState();
        }
     if(cdtime > 0)
        {
            cdtime -= Time.deltaTime;
            if(cdtime == 0)
                cdtime = 0;
        }


        InputFire();
        if (Input.GetKeyDown(KeyCode.R))
            Reload();

    }
    /// <summary>
    /// ��������״̬������
    /// </summary>
    private void UpdateController()
    {
      animator.SetFloat("moveSpeed", owner.walkValue);
     AnimatorStateInfo animatorStateInfo =   animator.GetCurrentAnimatorStateInfo(0);
        E_WEAPONSTAUS e_WEAPONSTAUS;
        if(animatorStateInfo.shortNameHash == fireHashValue)
        {
            //ת��Ϊ����״̬
            e_WEAPONSTAUS = E_WEAPONSTAUS.FIRING;
        }else if(animatorStateInfo.shortNameHash == reloadHashValue)
        {
            //ת��Ϊ����״̬
            e_WEAPONSTAUS = E_WEAPONSTAUS.RELOADING;
        }
        else
        {
            e_WEAPONSTAUS = E_WEAPONSTAUS.IDLE;
        }
        if(e_WEAPONSTAUS != currentWeaponSatus)
        {
           E_WEAPONSTAUS lastSatus = currentWeaponSatus;
            currentWeaponSatus = e_WEAPONSTAUS;
            if( remainSize == 0 && initAmount >0)
            {
                Reload();
            }
        }
    }

    /// <summary>
    /// ��ȡ������
    /// </summary>
    /// <returns></returns>
    public int GetInitAmount()
    {
        return initAmount;
    }

    /// <summary>
    /// ��ȡ����ID
    /// </summary>
    /// <returns></returns>
    public int GetID()
    {
        return itemID;
    }

    /// <summary>
    /// ����ѡ�ж���
    /// </summary>
    public void Selected()
    {
        animator.SetTrigger("Selected");
        if(fireanimationClip != null)
        {
            animator.SetFloat("firetime", fireanimationClip.length / firetime);
        }
        if(reloadinganimationClip != null)
        {
            animator.SetFloat("reloadtime" , reloadinganimationClip.length / reloadtime);
        }
        currentWeaponSatus = E_WEAPONSTAUS.IDLE;

        //���������������ݸ���ɫ
        owner.decreaseSpeed = decreaseSpeed;
    }
    /// <summary>
    /// ��������ʱ��������ǰ����ӵ����
    /// </summary>
    /// <param name="playerController"></param>
    public void PickUp(PlayerController playerController)
    {
        owner = playerController;
    }

    /// <summary>
    /// ���𶯻�
    /// </summary>
    public void Fire()
    {

        if (currentWeaponSatus != E_WEAPONSTAUS.IDLE || remainSize == 0 || cdtime > 0 || currentWeaponSatus == E_WEAPONSTAUS.RELOADING || owner.currentID == -1)
        {
            return;
        }
        cdtime = firetime;
        animator.SetTrigger("Fire");
        owner.cameraShaker.Shaker(advancedWeaponSetting.shakerTime, 0.05f * advancedWeaponSetting.shakerStrength);
        remainSize--;
        currentWeaponSatus = E_WEAPONSTAUS.FIRING;
        switch (weaponType)
        {
            case E_WEAPONTYPE.RAYCAST:
                RayCastShot();
                break;
            case E_WEAPONTYPE.BULLET:
                BulletShot();
                break;
            default:
                break;
        }
    }

    private void InputFire()
    {
        switch (e_WEAPONMODE)
        {
            case E_WEAPONMODE.NORMAL:
                if (Input.GetMouseButtonUp(0))
                {
                    Fire();
                }
                if (Input.GetMouseButton(1)  && HasView)
                {

                    telesopicView.OpenViewControl(true);
                }
                else if ( HasView)
                {

                    telesopicView.OpenViewControl(false);
                }
                break;
            case E_WEAPONMODE.AUTO:
                if (Input.GetMouseButton(0))
                    Fire();
                break;
            case E_WEAPONMODE.ACCUMULATION:
                if (Input.GetMouseButtonUp(0))
                {
                    Fire();
                    ShapeViewEffect.gameObject.SetActive(false);
                    ShapeViewEffect.startSize = 0;
                }
                else if(Input.GetMouseButton(0) && currentWeaponSatus!= E_WEAPONSTAUS.RELOADING && remainSize > 0)
                {
                   AccumulateEnergy();
                }
                break;
                
        }
    }
    /// <summary>
    /// ����ǹ  �����߼�
    /// </summary>
    public void AccumulateEnergy()
    {
        ShapeViewEffect.gameObject.SetActive(true);

        if(ShapeViewEffect.startSize <= 0.3f)
        {
            ShapeViewEffect.startSize += Time.deltaTime;
        }
        if(currentShapeAtkValue <= 3*atkValue)
        {
            currentShapeAtkValue += Time.deltaTime;
        }
    }



    /// <summary>
    /// ���⹥���߼�
    /// </summary>
    private void RayCastShot()
    {
        //��ɢ����
        float spreadRatio = advancedWeaponSetting.spreadAngle / Camera.main.fieldOfView;
        Vector2 spread = spreadRatio * Random.insideUnitCircle;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2 , Screen.height/2 , 0) + (Vector3)spread);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 1000, ~(1 << 8), QueryTriggerInteraction.Ignore))
        {
            ParticleSystem ps = PoolManager.Instance.GetInstance<ParticleSystem>(raycastHitEffectPrefab);
            ps.transform.position = hit.point;
            ps.transform.forward = hit.normal;
            ps.Play();

            //������������Ϊ�����ʱ  �����ù������Ϲ��صĽű��е����˷���
            if(hit.collider.gameObject.layer == 6)
            {
                Monster mon = hit.collider.GetComponent<Monster>();
                mon.Damage(atkValue);
            }
            else if (hit.collider.gameObject.layer == 7)
            {
                //����BOSSʱ������Boss�е����˷���
                Boss mon = hit.collider.GetComponentInParent<Boss>();
                mon.Damage(atkValue ,transform.position);
            }

            if (rayTrailPrefab != null)
            {
                //����ǹ��
                LineRenderer lineRenderer = PoolManager.Instance.GetInstance<LineRenderer>(rayTrailPrefab);
                Vector3[] pos = new Vector3[] { shootPoint.position , hit.point };
                lineRenderer.SetPositions(pos);
                lineRenderer.gameObject.SetActive(true);
                activeTrails.Add(new ActiveTrail() { line = lineRenderer ,direction = (pos[1] - pos[0]).normalized , remainTime = 0.2f }) ;

            }
        }

    }
    /// <summary>
    /// ģ�⼤����βЧ��
    /// </summary>
    private void UpdateTrailState()
    {
        Vector3[] pos = new Vector3[2];
        for(int i = 0; i < activeTrails.Count; i++)
        {
            ActiveTrail activeTrail = activeTrails[i];

            activeTrails[i].line.GetPositions(pos);
            activeTrails[i].remainTime -= Time.deltaTime;
            pos[0] += activeTrails[i].direction * 60 * Time.deltaTime;
          //  pos[1] += activeTrails[i].direction * 60 * Time.deltaTime;
            activeTrails[i].line.SetPositions(pos) ;
            if(activeTrail.remainTime <= 0 || Vector3.Distance(pos[0], pos[1])<0.5f )
            {
               activeTrail.line.gameObject.SetActive(false);
                activeTrails.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// �ӵ������߼�
    /// </summary>
    private void BulletShot()
    {
        
        Bullet bullet = PoolManager.Instance.GetInstance<Bullet>(bulletPrefab);
        bullet.gameObject.SetActive(true);
       if(e_WEAPONMODE == E_WEAPONMODE.ACCUMULATION)
        {
            bullet.SetAtkValue(currentShapeAtkValue);
            bullet.SetBulletSize(ShapebulletSize);
        }
      
        Vector2 angleDir = Random.insideUnitCircle * Mathf.Sin(advancedWeaponSetting.spreadAngle*Mathf.Deg2Rad);
        Vector3 dir = shootPoint.forward + (Vector3)angleDir;
        dir.Normalize();    
        bullet.Launch(this, dir, bulletforce);
    }

   public Transform GetPoint()
    {
        return shootPoint;
    }

    /// <summary>
    /// ������
    /// </summary>
    public void Reload()
    {
        
        //����������
        if (remainSize == bulletSize  || currentWeaponSatus != E_WEAPONSTAUS.IDLE)
        {
            return;
        }
        currentWeaponSatus = E_WEAPONSTAUS.RELOADING;
        //��ȡ������
        int remainitem = owner.GetAmmoAmount(itemID);
        if(remainitem == 0)
            return;
        int chargeInClip = Mathf.Min(remainitem ,bulletSize - remainSize);
        remainSize += chargeInClip;
       //��������������
        owner.UpdateAmmoAmount(itemID, -chargeInClip);
        animator.SetTrigger("Reload");

        if(currentWeaponSatus == E_WEAPONSTAUS.RELOADING &&  ShapeViewEffect != null)
        {
            ShapeViewEffect.gameObject.SetActive(false);
            ShapeViewEffect.startSize = 0;
        }
    }
    
    /// <summary>
    /// �жϵ������Ƿ����ӵ�
    /// </summary>
    /// <returns></returns>
    public bool HasBullet()
    {
        return remainSize > 0;
    }

}
