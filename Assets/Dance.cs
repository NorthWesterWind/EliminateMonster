using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dance : MonoBehaviour
{
    public GameObject dance;


    private void OnTriggerEnter(Collider other)
    {
        dance.SetActive(true);
    }
}
