using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject SpineRifle;

    public bool OpenOrClose;

    public RawImage blood;

    public Text txtbullet;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {

        SpineUIControl();
    }

    public void Open()
    {
       OpenOrClose = true;
    }
    public void Close()
    {
        OpenOrClose = false;
    }

    public void SpineUIControl()
    {
        if (OpenOrClose)
        {
            SpineRifle.SetActive(true);
        }
        else
        {
            SpineRifle.SetActive(false);
        }
    }

    public void ShowBlood()
    {
        blood.gameObject.SetActive(true);
        CancelInvoke();
        Invoke("HideBlood", 1.5f);
    }
    public void HideBlood()
    {
        blood.gameObject.SetActive(false);
    }

    public void TxtBullet(int size)
    {
        txtbullet.text = size.ToString();
    }
}
