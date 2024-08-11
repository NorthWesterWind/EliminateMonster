using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    private float rotateSpeed = 50;
    public int id;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //ʹ�����ڵط�ʱ��ת
        transform.eulerAngles += new Vector3(0 , rotateSpeed * Time.deltaTime, 0);  
    }


    /// <summary>
    /// �Ӵ�����ɫʱ��ʰȡ
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.PickUpWeapon(id);
            player.ChangeCurrentWeapon(true);
            Destroy(this.gameObject);
        }
    }
}
