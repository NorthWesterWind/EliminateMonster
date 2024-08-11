using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject keyGo;



    private bool unLocked;


    private void Start()
    {
      
    }

    /// <summary>
    /// ´¥·¢¼ì²â
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            if (keyGo == null)
            {
                unLocked = true;
            }
        }
    }

    private void Update()
    {
        if (unLocked)
        {
            transform.Translate(-Vector3.up * Time.deltaTime * 3);
            if(transform.position.y <= -8)
            {
                Destroy(gameObject);
            }
        }
    }
}
