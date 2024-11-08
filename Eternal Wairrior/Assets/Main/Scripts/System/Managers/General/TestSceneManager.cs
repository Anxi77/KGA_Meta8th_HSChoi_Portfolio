using UnityEngine;
using System.Collections;
using static InitializationManager;

public class TestSceneManager : MonoBehaviour
{
    [SerializeField] private ManagerPrefabData[] managerPrefabs;

    private void Start()
    {
        InitializeForTest();
    }

    private void InitializeForTest()
    {
        // 필요한 매니저들 초기화
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        foreach (var managerData in managerPrefabs)
        {
            if (managerData.prefab != null &&
                GameObject.Find(managerData.managerName) == null)
            {
                var manager = Instantiate(managerData.prefab);
                manager.name = managerData.managerName;
                Debug.Log($"Initialized {managerData.managerName} for test");
            }
        }
    }
}