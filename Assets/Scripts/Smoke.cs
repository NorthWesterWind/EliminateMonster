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

    //������ʧʱ
    private void OnDisable()
    {
        CancelInvoke();
        for(int i = 0; i < monsterList.Count; i++)
        {
            //�����нӴ�������Ĺ�����д���
            Monster monster = monsterList[i].GetComponentInParent<Monster>();
            monster.RecoverAttackRangeValue();
            monster.SetSmokeCollierState(true);
        }
        StopAllCoroutines();
    }

    //����Ӵ������߼�
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
        //�ù��������������  ��������ܵ�Ӱ��
        yield return new WaitForSeconds(2);
       Monster monster =  other.GetComponentInParent<Monster>();
        monster.SetRange(2);
        monster.SetSmokeCollierState(false);
        //�ӳ������Զ��ָ�
        yield return new WaitForSeconds(6);
        monster.SetSmokeCollierState(true);
    }
}
