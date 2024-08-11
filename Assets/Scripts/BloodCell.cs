using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodCell : Monster
{
    public float hpValue;
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        
        if(collision.transform.tag == "Player")
        {
            playerTrans.GetComponent<PlayerController>().TakeDamage(-hpValue);
            gameObject.SetActive(false);
        }
    }
}
