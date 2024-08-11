using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    public ParticleSystem particleSystem;

    private List<GameObject> monsterList = new List<GameObject>();

    private void OnEnable()
    {
        particleSystem.time = 0;
        particleSystem.Play();
        Invoke("HideSelf", 25);
    }

    private void HideSelf()
    {
        particleSystem.Stop();
        Invoke("HideGameObject" , 7f);
    }

    private void HideGameObject()
    {
        gameObject.SetActive(false);
    }

    //烟雾消失时
    private void OnDisable()
    {
        CancelInvoke();
        for(int i = 0; i < monsterList.Count; i++)
        {
            //对所有接触了烟雾的怪物进行处理
            Monster monster = monsterList[i].GetComponentInParent<Monster>();
            monster.RecoverAttackRangeValue();
            monster.SetSmokeCollierState(true);
        }
        StopAllCoroutines();
    }

    //烟雾接触怪物逻辑
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Monster")
        {
            if (!monsterList.Contains(other.gameObject))
            {
                monsterList.Add(other.gameObject);
                StartCoroutine(OutSmoke(other));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Monster")
        {
            if (!monsterList.Contains(other.gameObject))
            {
                monsterList.Remove(other.gameObject);
            }
        }
    }

    IEnumerator OutSmoke(Collider other)
    {
        //让怪物进入烟雾区域  两秒后再受到影响
        yield return new WaitForSeconds(2);
       Monster monster =  other.GetComponentInParent<Monster>();
        monster.SetRange(2);
        monster.SetSmokeCollierState(false);
        //延迟四秒自动恢复
        yield return new WaitForSeconds(6);
        monster.SetSmokeCollierState(true);
    }
}
