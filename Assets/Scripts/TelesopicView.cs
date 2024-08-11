using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelesopicView : MonoBehaviour
{
    public float zoomLevel = 2;
    public float zoomSpeed = 100;
    private float initFOV = 60;
    private bool OpenView;
 
    // Start is called before the first frame update
    void Awake()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (OpenView)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
    public void Open()
    {
      
        //if (Camera.main.fieldOfView != initFOV / zoomLevel)
        //{
        //    if(Mathf.Abs(Camera.main.farClipPlane - initFOV / zoomLevel) <10)
        //    {
        //        Camera.main.fieldOfView = initFOV / zoomLevel;
        //    }
        //    else
        //    {
        //        Camera.main.fieldOfView -= Time.deltaTime * zoomSpeed;
        //    }
        //}
        Camera.main.fieldOfView = initFOV / zoomLevel;
        UIManager.Instance.Open();
    }
    public void Close()
    {

        //if (Camera.main.fieldOfView != initFOV)
        //{
        //    if (Mathf.Abs(Camera.main.farClipPlane - initFOV) <10)
        //    {
        //        Camera.main.fieldOfView = initFOV ;
        //    }
        //    else
        //    {
        //        Camera.main.fieldOfView += Time.deltaTime * zoomSpeed;
        //    }
        //}
        Camera.main.fieldOfView = initFOV;
        UIManager.Instance.Close();
 
    }

    public void OpenViewControl(bool b = true)
    {
        OpenView = b;
    }
}
