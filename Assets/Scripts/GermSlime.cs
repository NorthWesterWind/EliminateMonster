using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GermSlime : Monster
{
    public float AtkValue;

    public override void Attack()
    {
        if(Time.time - lastAtkTime < actresttime)
            return;
        transform.LookAt(playerTrans);
        lastAtkTime = Time.time;
        playerTrans.GetComponent<PlayerController>().TakeDamage(AtkValue);
    }
}
