using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
   // public Image HpImage;
    public RectTransform HpImage;
    private float width;
    private float height;

    //角色控制器组件
    private CharacterController characterController;
    //角色移动速度
    private float moveSpeed = 5f;
    //角色奔跑速度
    private float runSpeed =8f;
    //角色跳跃速度
    private float jumpSpeed ;
    //角色跳跃状态
    private bool isJump;
    private int jumpnum = 0;
   
    public float mouseSensitivity = 2.4f;

    private float angleY;
    private float rotateY;

   private float angleX;
    private float rotateX;
    //视角摄像机
    private Transform cameraTransform;
    //当前武器ID
    public int currentID = -1;
    //存放当前拥有的武器
    private Dictionary<int,Weapon> weaponDic ;
    //武器库
    public Dictionary<int, int> ammoInventory;
    //武器位置
    private Transform weaponPlaceTrans;
    //角色血量
    public float Hp;
    //角色当前血量值
    public float currentHp;

    private bool isDead;

    public Transform deadTransform;

    private CollisionFlags collisionFlags;

    public float actualSpeed;

    public float walkValue = 0;

    //用来模拟不同武器的重量对角色移速的影响
    public float decreaseSpeed;


    public CameraShaker cameraShaker;
    // Start is called before the first frame update
    void Start()
    {
        width = HpImage.sizeDelta.x;
        height = HpImage.sizeDelta.y;
        //得到玩家身上的角色控制器
        characterController = GetComponent<CharacterController>();
        //获取摄像机的Transform
        cameraTransform = Camera.main.transform;

        angleY = transform.eulerAngles.y;
        angleX = cameraTransform.eulerAngles.x;

        //鼠标锁定和隐藏
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        weaponPlaceTrans = cameraTransform.Find("WeaponPos");

        weaponDic = new Dictionary<int, Weapon>();   
        ammoInventory = new Dictionary<int, int>();

        currentHp = Hp;
    }

    // Update is called once per frame
    void Update()
    {
        if(isDead)
            return;
       
        Move();
        Rotate();
        Jump();
        ChangeCurrentWeapon();
        if(currentID != -1)
        {
            UIManager.Instance.TxtBullet(weaponDic[currentID].remainSize);
        }
 

    }

    /// <summary>
    /// 角色移动逻辑
    /// </summary>
    public void Move()
    {
        actualSpeed = Input.GetButton("Run") ? runSpeed - decreaseSpeed : moveSpeed - decreaseSpeed;
        Vector3 movedir = Vector3.zero;
        //将键盘热键输入值作为移动方向向量
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        movedir = new Vector3(h, 0,v);
        if (movedir == Vector3.zero)
        {
            walkValue = -1;
            return;
        }
         walkValue = 1;
        
       
        //有输入值
         movedir.Normalize();
        movedir = movedir * actualSpeed * Time.deltaTime;
        //将本地向量转换为世界向量
        movedir = transform.TransformDirection(movedir);
        collisionFlags =  characterController.Move(movedir);
            if(collisionFlags != CollisionFlags.Below)
            {
                isJump = true;
            }


        if (h <= 0.1f && v <= 0.1f)
            actualSpeed = 0;
     
    }

    /// <summary>
    /// 角色视角旋转逻辑
    /// </summary>
   public void Rotate()
    {
        rotateY = Input.GetAxis("Mouse X") * mouseSensitivity;
        angleY += rotateY;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, angleY, transform.eulerAngles.z);
        rotateX = -Input.GetAxis("Mouse Y") * mouseSensitivity;
        angleX += rotateX;
        angleX = Mathf.Clamp(angleX, -40, 40);
        cameraTransform.eulerAngles = new Vector3(angleX, angleY, cameraTransform.eulerAngles.z);
    }

    /// <summary>
    /// 角色跳跃逻辑
    /// </summary>
    private void Jump()
    {
        if(Input.GetButton("Jump") && !isJump && jumpnum < 1)
        {
            isJump = true;
            jumpSpeed = 8f;
            jumpnum++;
        }
        if (isJump)
        {
            jumpSpeed = jumpSpeed - 20 * Time.deltaTime;
            Vector3 jump = new Vector3(0,jumpSpeed * Time.deltaTime, 0);
             collisionFlags  = characterController.Move(jump);
            if(collisionFlags == CollisionFlags.Below)
            {
                isJump = false;
                jumpSpeed = 0;
                jumpnum--;
                if(jumpnum < 0) 
                    jumpSpeed = 0;
            }
        }
       
    }
    
    /// <summary>
    /// 玩家受伤逻辑
    /// </summary>
    public void TakeDamage(float value)
    {
        if(isDead)
            return;
        currentHp -= value;
        UIManager.Instance.ShowBlood();
        cameraShaker.Shaker(0.2f, 0.5f);
        if(currentHp > Hp)
            currentHp = Hp;
        if(currentHp <= 0)
        { 
            currentHp = 0;
            //角色死亡
            GameOver();
        }
        HpImage.sizeDelta = new Vector2(width* (currentHp / Hp), height);
    }
    /// <summary>
    /// 角色死亡逻辑
    /// </summary>
    private  void GameOver()
    {
        Debug.Log("角色死亡");
        isDead = true;

        cameraTransform.localPosition = deadTransform.localPosition;
        cameraTransform.eulerAngles = deadTransform.eulerAngles;
        weaponPlaceTrans.gameObject.SetActive(false);
    }


    /// <summary>
    /// 切换武器逻辑
    /// </summary>
    /// <param name="id"></param>
    private  void ChangeWeapon(int id)
    {
        //武器容器中无武器
        if (weaponDic.Count == 0 )
            return;

        //处理id越界
        if (id > weaponDic.Keys.Max())
        {
         id = weaponDic.Keys.Min();
        }
        else if (id < weaponDic.Keys.Min())
        {
            id = weaponDic.Keys.Max();
        }
        if (id == currentID)
            return;

        while (!weaponDic.ContainsKey(id))
        {
            if(id > currentID)
            {
                id++;
            }
            else
            {
                id--;
            }
        }

        if(currentID != -1)
        {
            //隐藏上一把武器
            weaponDic[currentID].gameObject.SetActive(false);
        }
       
        //显示武器
        weaponDic[id].gameObject.SetActive(true);
        weaponDic[id].Selected();
        currentID = id;
    }

    /// <summary>
    /// 监测输入  改变当前武器ID
    /// </summary>
    public void ChangeCurrentWeapon( bool isAuto = false)
    {
        if(isAuto)
        {
            ChangeWeapon(weaponDic.Keys.Last());
        }
        else
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                ChangeWeapon(currentID + 1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                ChangeWeapon(currentID - 1);
            }

            //监测键盘数字键切换武器
            for (int i = 0; i < 10; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    int num = 0;
                    if (i == 0)
                    {
                        num = 10;
                    }
                    else
                    {
                        num = i - 1;
                    }
                    if ( weaponDic.ContainsKey(num))
                    {
                        ChangeWeapon(num);
                    }
                }
            }
        }
        
    }

    /// <summary>
    /// 角色拾取武器逻辑
    /// </summary>
    /// <param name="weaponName"></param>
    public void PickUpWeapon(int weaponId)
    {
        if(weaponDic.ContainsKey(weaponId))
        {
            //已获取过该武器 
            //补充弹药
            Weapon weapon = weaponDic[weaponId];
            ammoInventory[weapon.GetID()] = weapon.GetInitAmount();
            if (!weapon.HasBullet())
            {
                weapon.Reload();
            }
        }
        else
        {
            //添加进武器容器
           GameObject item = Instantiate(Resources.Load<GameObject>("Prefabs/Weapons/" +weaponId));
            item.transform.SetParent(weaponPlaceTrans);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.SetActive(false);
            Weapon weapon = item.GetComponent<Weapon>();
            weapon.PickUp(this);
            weaponDic.Add(weaponId, weapon);
            ammoInventory.Add(weapon.GetID(), weapon.GetInitAmount());
        }
    }

    /// <summary>
    /// 根据武器ID获取武器备弹数
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int GetAmmoAmount(int id)
    {
        int value = 0;
        ammoInventory.TryGetValue(id, out value);
        return value;
    }

    /// <summary>
    /// 更新武器备弹数
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    public void UpdateAmmoAmount(int id , int value)
    {
        if (ammoInventory.ContainsKey(id))
        {
            ammoInventory[id] += value;
        }
    }
}
