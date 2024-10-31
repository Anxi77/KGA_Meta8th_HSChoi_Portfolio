using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    #region Members
   
    private static GameManager instance;

    public static GameManager Instance => instance;

    internal List<Enemy> enemies = new List<Enemy>(); //씬에 존재하는 전체 적 List

    internal Player player;

    #endregion
    
    #region Unity Message Methods
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Update()
    {
        //PlayerStatusCheck();
    }

    #endregion

    #region Unit Related Methods
    private void PlayerStatusCheck()
    {
        switch (player.playerStatus)
        {
            case Player.Status.Alive:
                break;
            case Player.Status.Dead:
                Time.timeScale = 0;
                break;
        }
    }

    public void Resume() 
    {
        Time.timeScale = 1;
    }

    #endregion

}

#region Tutorials

/*MyClass myClass = MyClass.GetMyClass();//객체 생성
        //기본 생성자가 private이므로 GetMyClass로만 인스턴스에 접근할수 있다.                                
        //필연적으로 싱글톤을 사용할때는 스태틱 변수를 하나 만들어 데이터영역에 두어 참조를 잃지 않도록
        //만약 myClass가 필요 없어져서 null을 대입하는 등 참조를 잃으면
        //GC에 의해 객체가 삭제된다.
*/

/*전형적인 C#의 싱글톤 객체 형식
public class DefaultSingleton
{
    //현재 프로세스 내에 단일 책임을 진 인스턴스를 저장할 변수
    private static DefaultSingleton instance;

    private DefaultSingleton() { } //외부에서 생성자를 호출할 수 없도록 기본 생성자 접근을 막는다.

    //외부에서는 단일 생성된 인스턴스에 접근하여 값을 가져올 수만 있음(다른 값으로 대입 불가)
    public static DefaultSingleton Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DefaultSingleton();
                return instance;
            }
            return instance;
        }
    }
    /*
    //현재 프로세스 내에 단일 책임을 진 인스턴스를 저장할 변수
    private static DefaultSingleton _Instance;

    private DefaultSingleton() {}

    public DefaultSingleton Instance 
    { 
        get 
        { 
            if (_Instance == null) 
            { 
                _Instance = new DefaultSingleton(); 
                return _Instance;
            }
            return _Instance;
        }
    }
}
    */

/*기본적인 객체지향적 언어에서 싱글톤 객체를 만드는 방법
public class MyClass
{
    private static MyClass nonCollectableMyClass; //참조를 잃으면 안되는 myclass 인스턴스를 저장.

    private MyClass() { }

    public int processCount;//전역변수(non-static)

    public static MyClass GetMyClass()
    {
        if (nonCollectableMyClass == null)//GetMyClass가 최초 호출됬을 경우에만 True 
        {
            nonCollectableMyClass = new MyClass();
            return nonCollectableMyClass;
        }
        else
        {
            return nonCollectableMyClass;
        }
    }
}
*/

#region EventHandler
//private void HandleBombExploded()
//{
//    Bomb.OnBombExploded += HandleBombExploded;
//    Enemy.OnEnemyKilled += HandleEnemyKilled;
//    if (player != null && enemies != null)
//    {
//        foreach (Enemy enemy in enemies)
//        {
//            Destroy(enemy.gameObject);
//        }
//        enemies.Clear();
//    }
//}

//private void HandleEnemyKilled(Enemy enemy, float exp) 
//{
//    player.GainExperience(exp);
//    player.killCount++;
//    enemies.Remove(enemy);
//    Destroy(enemy.gameObject);
//}
#endregion

#endregion
