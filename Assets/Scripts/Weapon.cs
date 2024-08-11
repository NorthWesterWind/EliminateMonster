using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 武器类型
/// </summary>
public enum E_WEAPONTYPE
{
    RAYCAST,
    BULLET
}

/// <summary>
/// 武器状态
/// </summary>
public enum E_WEAPONSTAUS
{
    IDLE,
    FIRING,
    RELOADING
}

/// <summary>
/// 武器攻击模式
/// </summary>
public enum E_WEAPONMODE
{
    NORMAL,
    AUTO,
    ACCUMULATION
}

/// <summary>
/// 激光拖尾信息类
/// </summary>
public class ActiveTrail
{
    public LineRenderer line;
    public Vector3 direction;
    public float remainTime;
}

/// <summary>
/// 武器属性拓展
/// </summary>
[Serializable]
public class AdvancedWeaponSetting
{   
    //子弹偏移程度
    public float spreadAngle;
    //武器震动时间
    public float shakerTime;
    //武器震动幅度
    public float shakerStrength;
}

public class Weapon : MonoBehaviour
{
    //武器拥有者
    private PlayerController owner;
    //武器ID
    public int itemID = -1;
    //备弹数
    public int initAmount = -1;
    //枪械动画控制器
    private Animator animator;

    //枪线
    public LineRenderer rayTrailPrefab;

    //枪口
    public Transform shootPoint;
    
    public E_WEAPONTYPE weaponType;
    //激光信息类容器
    public List<ActiveTrail> activeTrails;
    //子弹脚本预设体
    public Bullet bulletPrefab;
    //给子弹施加的力
    public  float bulletforce = 600;
    //弹夹容量
    public int bulletSize ;
    //弹夹剩余子弹量
    public int remainSize;

    public AdvancedWeaponSetting advancedWeaponSetting;
    //武器状态
    private E_WEAPONSTAUS currentWeaponSatus;

    public E_WEAPONMODE e_WEAPONMODE;


    private int fireHashValue;
    private int reloadHashValue;

    //换弹时间
    public float reloadtime;
    //攻击频率
    public float firetime;
    //CD
    private float cdtime ;
    //开火动画切片
    public AnimationClip fireanimationClip;
    //换弹动画切片
    public AnimationClip reloadinganimationClip;

    //攻击力
    public float atkValue;
    //激光溅射特效预设体
    public ParticleSystem raycastHitEffectPrefab;

    //倍镜功能
    public bool HasView;
    public TelesopicView telesopicView;

    //聚能枪子弹大小
    private float ShapebulletSize;
    //聚能枪特效
    public ParticleSystem ShapeViewEffect;
    //聚能枪实时攻击力
    private float currentShapeAtkValue;

    public float decreaseSpeed;
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();    
        if(rayTrailPrefab != null)
        {
            //提前生成激光预设体
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
    /// 更新武器状态控制器
    /// </summary>
    private void UpdateController()
    {
      animator.SetFloat("moveSpeed", owner.walkValue);
     AnimatorStateInfo animatorStateInfo =   animator.GetCurrentAnimatorStateInfo(0);
        E_WEAPONSTAUS e_WEAPONSTAUS;
        if(animatorStateInfo.shortNameHash == fireHashValue)
        {
            //转换为开火状态
            e_WEAPONSTAUS = E_WEAPONSTAUS.FIRING;
        }else if(animatorStateInfo.shortNameHash == reloadHashValue)
        {
            //转换为换弹状态
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
    /// 获取备弹数
    /// </summary>
    /// <returns></returns>
    public int GetInitAmount()
    {
        return initAmount;
    }

    /// <summary>
    /// 获取武器ID
    /// </summary>
    /// <returns></returns>
    public int GetID()
    {
        return itemID;
    }

    /// <summary>
    /// 播放选中动画
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

        //将武器的重量传递给角色
        owner.decreaseSpeed = decreaseSpeed;
    }
    /// <summary>
    /// 捡起武器时，关联当前武器拥有者
    /// </summary>
    /// <param name="playerController"></param>
    public void PickUp(PlayerController playerController)
    {
        owner = playerController;
    }

    /// <summary>
    /// 开火动画
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
    /// 聚能枪  蓄能逻辑
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
    /// 激光攻击逻辑
    /// </summary>
    private void RayCastShot()
    {
        //发散比例
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

            //当碰到的物体为怪物层时  ，调用怪物身上挂载的脚本中的受伤方法
            if(hit.collider.gameObject.layer == 6)
            {
                Monster mon = hit.collider.GetComponent<Monster>();
                mon.Damage(atkValue);
            }
            else if (hit.collider.gameObject.layer == 7)
            {
                //射中BOSS时，调用Boss中的受伤方法
                Boss mon = hit.collider.GetComponentInParent<Boss>();
                mon.Damage(atkValue ,transform.position);
            }

            if (rayTrailPrefab != null)
            {
                //画出枪线
                LineRenderer lineRenderer = PoolManager.Instance.GetInstance<LineRenderer>(rayTrailPrefab);
                Vector3[] pos = new Vector3[] { shootPoint.position , hit.point };
                lineRenderer.SetPositions(pos);
                lineRenderer.gameObject.SetActive(true);
                activeTrails.Add(new ActiveTrail() { line = lineRenderer ,direction = (pos[1] - pos[0]).normalized , remainTime = 0.2f }) ;

            }
        }

    }
    /// <summary>
    /// 模拟激光拖尾效果
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
    /// 子弹攻击逻辑
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
    /// 换弹夹
    /// </summary>
    public void Reload()
    {
        
        //弹夹是满的
        if (remainSize == bulletSize  || currentWeaponSatus != E_WEAPONSTAUS.IDLE)
        {
            return;
        }
        currentWeaponSatus = E_WEAPONSTAUS.RELOADING;
        //获取备弹量
        int remainitem = owner.GetAmmoAmount(itemID);
        if(remainitem == 0)
            return;
        int chargeInClip = Mathf.Min(remainitem ,bulletSize - remainSize);
        remainSize += chargeInClip;
       //更新武器备弹数
        owner.UpdateAmmoAmount(itemID, -chargeInClip);
        animator.SetTrigger("Reload");

        if(currentWeaponSatus == E_WEAPONSTAUS.RELOADING &&  ShapeViewEffect != null)
        {
            ShapeViewEffect.gameObject.SetActive(false);
            ShapeViewEffect.startSize = 0;
        }
    }
    
    /// <summary>
    /// 判断弹夹内是否还有子弹
    /// </summary>
    /// <returns></returns>
    public bool HasBullet()
    {
        return remainSize > 0;
    }

}
