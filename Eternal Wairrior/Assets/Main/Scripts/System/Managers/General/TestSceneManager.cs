using UnityEngine;
using System.Collections;
using static InitializationManager;

public class TestSceneManager : MonoBehaviour
{
    [SerializeField] private ManagerPrefabData[] managerPrefabs;
    [SerializeField] private bool autoStartGameLoop = true;
    private void Start()
    {
        // 현재 씬이 InitScene을 통해 로드되었는지 확인
        if (!IsInitialized())
        {
            // InitScene이 없다면 InitScene을 로드
            Debug.Log("Loading InitScene for proper initialization...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("InitScene");
            return;
        }

        InitializeManagers();

        if (autoStartGameLoop && StageManager.Instance != null)
        {
            StageManager.Instance.LoadTestScene();
        }
    }

    private bool IsInitialized()
    {
        // 핵심 매니저들이 존재하는지 확인
        return GameManager.Instance != null &&
               UIManager.Instance != null &&
               GameLoopManager.Instance != null;
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
                DontDestroyOnLoad(manager);
                Debug.Log($"Initialized {managerData.managerName}");
            }
        }
    }
}