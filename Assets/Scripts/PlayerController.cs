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

    //��ɫ���������
    private CharacterController characterController;
    //��ɫ�ƶ��ٶ�
    private float moveSpeed = 5f;
    //��ɫ�����ٶ�
    private float runSpeed =8f;
    //��ɫ��Ծ�ٶ�
    private float jumpSpeed ;
    //��ɫ��Ծ״̬
    private bool isJump;
    private int jumpnum = 0;
   
    public float mouseSensitivity = 2.4f;

    private float angleY;
    private float rotateY;

   private float angleX;
    private float rotateX;
    //�ӽ������
    private Transform cameraTransform;
    //��ǰ����ID
    public int currentID = -1;
    //��ŵ�ǰӵ�е�����
    private Dictionary<int,Weapon> weaponDic ;
    //������
    public Dictionary<int, int> ammoInventory;
    //����λ��
    private Transform weaponPlaceTrans;
    //��ɫѪ��
    public float Hp;
    //��ɫ��ǰѪ��ֵ
    public float currentHp;

    private bool isDead;

    public Transform deadTransform;

    private CollisionFlags collisionFlags;

    public float actualSpeed;

    public float walkValue = 0;

    //����ģ�ⲻͬ�����������Խ�ɫ���ٵ�Ӱ��
    public float decreaseSpeed;


    public CameraShaker cameraShaker;
    // Start is called before the first frame update
    void Start()
    {
        width = HpImage.sizeDelta.x;
        height = HpImage.sizeDelta.y;
        //�õ�������ϵĽ�ɫ������
        characterController = GetComponent<CharacterController>();
        //��ȡ�������Transform
        cameraTransform = Camera.main.transform;

        angleY = transform.eulerAngles.y;
        angleX = cameraTransform.eulerAngles.x;

        //�������������
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
    /// ��ɫ�ƶ��߼�
    /// </summary>
    public void Move()
    {
        actualSpeed = Input.GetButton("Run") ? runSpeed - decreaseSpeed : moveSpeed - decreaseSpeed;
        Vector3 movedir = Vector3.zero;
        //�������ȼ�����ֵ��Ϊ�ƶ���������
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        movedir = new Vector3(h, 0,v);
        if (movedir == Vector3.zero)
        {
            walkValue = -1;
            return;
        }
         walkValue = 1;
        
       
        //������ֵ
         movedir.Normalize();
        movedir = movedir * actualSpeed * Time.deltaTime;
        //����������ת��Ϊ��������
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
    /// ��ɫ�ӽ���ת�߼�
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
    /// ��ɫ��Ծ�߼�
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
    /// ��������߼�
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
            //��ɫ����
            GameOver();
        }
        HpImage.sizeDelta = new Vector2(width* (currentHp / Hp), height);
    }
    /// <summary>
    /// ��ɫ�����߼�
    /// </summary>
    private  void GameOver()
    {
        Debug.Log("��ɫ����");
        isDead = true;

        cameraTransform.localPosition = deadTransform.localPosition;
        cameraTransform.eulerAngles = deadTransform.eulerAngles;
        weaponPlaceTrans.gameObject.SetActive(false);
    }


    /// <summary>
    /// �л������߼�
    /// </summary>
    /// <param name="id"></param>
    private  void ChangeWeapon(int id)
    {
        //����������������
        if (weaponDic.Count == 0 )
            return;

        //����idԽ��
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
            //������һ������
            weaponDic[currentID].gameObject.SetActive(false);
        }
       
        //��ʾ����
        weaponDic[id].gameObject.SetActive(true);
        weaponDic[id].Selected();
        currentID = id;
    }

    /// <summary>
    /// �������  �ı䵱ǰ����ID
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

            //���������ּ��л�����
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
    /// ��ɫʰȡ�����߼�
    /// </summary>
    /// <param name="weaponName"></param>
    public void PickUpWeapon(int weaponId)
    {
        if(weaponDic.ContainsKey(weaponId))
        {
            //�ѻ�ȡ�������� 
            //���䵯ҩ
            Weapon weapon = weaponDic[weaponId];
            ammoInventory[weapon.GetID()] = weapon.GetInitAmount();
            if (!weapon.HasBullet())
            {
                weapon.Reload();
            }
        }
        else
        {
            //��ӽ���������
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
    /// ��������ID��ȡ����������
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
    /// ��������������
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
