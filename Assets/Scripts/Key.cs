using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    //默认原地旋转速度
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
        //碰到玩家时 自动销毁
       if(other.tag == "Player")
        {
           
            GameObject.Destroy(gameObject);
        }
    }

}
