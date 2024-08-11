using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池管理类
/// </summary>
public  class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    /// <summary>
    /// 对象池
    /// </summary>
    private Dictionary<Object , Queue<Object>> poolDic;
    private void Awake()
    {
        Instance = this;
        poolDic = new Dictionary<Object, Queue<Object>>();
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 对象池初始化
    /// </summary>
    public void Init(Object prefabs , int size)
    {
 
        if(poolDic.ContainsKey(prefabs) || prefabs == null)
        {

            return;

        }
 
        Queue<Object> queue = new Queue<Object>();
        for(int i = 0; i < size; i++)
        {
            Object go = Instantiate(prefabs);
            CreateGameObjectAndSetActive(go, false);
            queue.Enqueue(go);
        }
        //加入对象池容器
        poolDic.Add(prefabs, queue);

    }

    /// <summary>
    /// 创建对象逻辑
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="active"></param>
    private void CreateGameObjectAndSetActive(Object obj , bool active)
    {
        GameObject item = null;
        if(obj is Component)
        {
            Component component = obj as Component;
            item = component.gameObject;
        }
        else
        {
            item = obj as GameObject;
        }
        item.SetActive(active);
    }

    /// <summary>
    /// 从对象池子中获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public T GetInstance<T>(Object prefab) where T : Object
    {
        Queue<Object> q = new Queue<Object>();
        if(poolDic.TryGetValue(prefab, out q))
        {
            Object obj;
            if(q.Count > 0)
            {
                obj = q.Dequeue();
            }
            else
            {
                obj = Instantiate(prefab);
            }
            CreateGameObjectAndSetActive(obj, true);    
            q.Enqueue(obj);
            return obj as T;    
        }
        return null;
    }
}
