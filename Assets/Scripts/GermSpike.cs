using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GermSpike : Monster
{
    public GameObject explosionEffect;
    public float AtkValue;

    protected override void Start()
    {
        base.Start();
       
    }

    public override void Attack()
    {
        if(Time.time - lastAtkTime < actresttime)
            return;
        transform.LookAt(playerTrans);
        lastAtkTime = Time.time;
        PoolManager.Instance.Init(explosionEffect, 1);
        playerTrans.GetComponent<PlayerController>().TakeDamage(AtkValue);
        GameObject eff = PoolManager.Instance.GetInstance<GameObject>(explosionEffect);
        eff.transform.position = transform.position;
        eff.transform.localScale = Vector3.one * 3;
        eff.SetActive(true);
      
    }
   
}
