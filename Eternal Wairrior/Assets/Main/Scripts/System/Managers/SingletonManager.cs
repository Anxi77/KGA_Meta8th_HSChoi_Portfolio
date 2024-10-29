using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance { get { return instance; } } //Get만 가능한 퍼블릭 프로퍼티

    protected virtual void Awake()
    {
        if (instance == null)
        {
            //자식인 경우에 T로 타입을 캐스팅하였을때 monobehaviour를 상속했을때만 들어가게끔 제약조건을 설정해야한다.
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(this);
        }
    }
}
