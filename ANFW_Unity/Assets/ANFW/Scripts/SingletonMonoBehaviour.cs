using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null) {
                _instance = (T)FindObjectOfType(typeof(T));

                if (_instance == null) {
                    SetupInstance();
                } else {
                    string typeName = typeof(T).Name;
                    Debug.Log("[Singleton]" + typeName + " instance already created: " + _instance.gameObject.name);
                }
            }
            return _instance;
        }
    }

    public virtual void Awake()
    {
        RemoveDuplicates();
    }

    /// <summary>
    /// シングルトン初期化
    /// </summary>
    private static void SetupInstance()
    {
        _instance = (T)FindObjectOfType(typeof(T));

        if (_instance == null) {
            GameObject gameObject = new GameObject();
            gameObject.name = typeof(T).Name;

            _instance = gameObject.AddComponent<T>();
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 重複チェック
    /// </summary>
    private void RemoveDuplicates()
    {
        if (_instance == null) {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
