using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InitializationManager : MonoBehaviour
{
    [System.Serializable]
    public class ManagerPrefabData
    {
        public string managerName;
        public GameObject prefab;
    }

    [SerializeField] private ManagerPrefabData[] managerPrefabs;
    [SerializeField] private GameObject eventSystemPrefab;

    private void Start()
    {
        // 1. EventSystem 초기화
        InitializeEventSystem();

        // 2. 매니저 오브젝트들만 생성 (초기화는 하지 않음)
        CreateManagerObjects();

        // 3. GameLoopManager를 통해 실제 초기화 시작
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.StartInitialization();
        }
    }

    private void CreateManagerObjects()
    {
        foreach (var managerData in managerPrefabs)
        {
            if (managerData.prefab != null)
            {
                var manager = Instantiate(managerData.prefab);
                manager.name = managerData.managerName;
                DontDestroyOnLoad(manager);

                // PathFindingManager는 초기에 비활성화
                if (managerData.managerName == "PathFindingManager")
                {
                    manager.SetActive(false);
                }
            }
        }
    }

    private void InitializeEventSystem()
    {
        var existingEventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (existingEventSystem == null && eventSystemPrefab != null)
        {
            var eventSystemObj = Instantiate(eventSystemPrefab);
            eventSystemObj.name = "EventSystem";
            DontDestroyOnLoad(eventSystemObj);
        }
    }
}