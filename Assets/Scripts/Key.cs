using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    //Ĭ��ԭ����ת�ٶ�
    public float rotateSpeed;

    
    void Start()
    {
        rotateSpeed = 50;
    }


    void Update()
    {
        transform.eulerAngles += new Vector3(0 , rotateSpeed * Time.deltaTime, 0);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        //�������ʱ �Զ�����
       if(other.tag == "Player")
        {
           
            GameObject.Destroy(gameObject);
        }
    }

}
