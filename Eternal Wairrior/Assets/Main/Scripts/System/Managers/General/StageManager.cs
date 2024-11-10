using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : SingletonManager<StageManager>
{

    public enum SceneType
    {
        MainMenu,
        Town,
        Game,
        Test
    }


    [Header("Portal Settings")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Vector3 townPortalPosition = new Vector3(10, 0, 0);
    [SerializeField] private Vector3 gameStagePortalPosition = new Vector3(-10, 0, 0);

    #region Scene Loading
    public void LoadMainMenu()
    {
        Debug.Log("StageManager: Starting to load main menu...");
        StartCoroutine(LoadSceneCoroutine("MainMenu", SceneType.MainMenu));
    }

    public void LoadTownScene()
    {
        StartCoroutine(LoadSceneCoroutine("TownScene", SceneType.Town));
    }

    public void LoadGameScene()
    {
        StartCoroutine(LoadSceneCoroutine("GameScene", SceneType.Game));
    }

    public void LoadTestScene()
    {
        StartCoroutine(LoadSceneCoroutine("TestScene", SceneType.Test));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, SceneType sceneType)
    {
        Debug.Log($"Starting to load scene: {sceneName}");

        UIManager.Instance.ShowLoadingScreen();
        UIManager.Instance.UpdateLoadingProgress(0f);
        Time.timeScale = 0f;

        // 초기 로딩 (0% - 10%)
        float progress = 0f;
        while (progress < 10f)
        {
            progress += Time.unscaledDeltaTime * 50f;
            UIManager.Instance.UpdateLoadingProgress(progress);
            yield return null;
        }

        CleanupCurrentScene();

        // TestScene인 경우와 일반 씬 로딩을 구분
        if (sceneName.Contains("Test"))
        {
            // TestScene은 비동기 로딩 없이 바로 초기화 (10% - 70%)
            progress = 10f;
            while (progress < 70f)
            {
                progress += Time.unscaledDeltaTime * 100f;
                UIManager.Instance.UpdateLoadingProgress(progress);
                yield return null;
            }

            // 씬 전환
            SceneManager.LoadScene(sceneName);

            // 씬 로드 후 초기화가 완료될 때까지 잠시 대기
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // 일반 씬 비동기 로딩 (10% - 70%)
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                progress = Mathf.Lerp(10f, 70f, asyncLoad.progress / 0.9f);
                UIManager.Instance.UpdateLoadingProgress(progress);
                yield return null;
            }

            asyncLoad.allowSceneActivation = true;
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        // UI 초기화 (70% - 80%)
        switch (sceneType)
        {
            case SceneType.MainMenu:
                UIManager.Instance.SetupMainMenuUI();
                break;
            default:
                UIManager.Instance.SetupGameUI();
                break;
        }

        // 게임 상태 변경
        switch (sceneType)
        {
            case SceneType.MainMenu:
                GameLoopManager.Instance.ChangeState(GameLoopManager.GameState.MainMenu);
                break;
            case SceneType.Town:
                GameLoopManager.Instance.ChangeState(GameLoopManager.GameState.Town);
                break;
            case SceneType.Game:
            case SceneType.Test:
                GameLoopManager.Instance.ChangeState(GameLoopManager.GameState.Stage);
                break;
        }

        // 씬 초기화 완료 대기 (80% - 95%)
        while (!IsSceneReady(sceneType))
        {
            progress = Mathf.Lerp(80f, 95f, Time.unscaledDeltaTime);
            UIManager.Instance.UpdateLoadingProgress(progress);
            yield return null;
        }

        // 최종 마무리 (95% - 100%)
        while (progress < 100f)
        {
            progress += Time.unscaledDeltaTime * 50f;
            UIManager.Instance.UpdateLoadingProgress(Mathf.Min(100f, progress));
            yield return null;
        }

        UIManager.Instance.HideLoadingScreen();
        Time.timeScale = 1f;
        Debug.Log($"Scene {sceneName} initialization complete");
    }

    private bool IsSceneReady(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.MainMenu:
                return UIManager.Instance != null &&
                       UIManager.Instance.IsMainMenuActive();

            case SceneType.Town:
                return GameManager.Instance?.player != null &&
                       CameraManager.Instance?.IsInitialized == true &&
                       UIManager.Instance?.playerUIPanel != null &&
                       UIManager.Instance.IsGameUIReady();

            case SceneType.Game:
            case SceneType.Test:
                bool isReady = GameManager.Instance?.player != null &&
                              CameraManager.Instance?.IsInitialized == true &&
                              UIManager.Instance?.playerUIPanel != null &&
                              UIManager.Instance.IsGameUIReady() &&
                              MonsterManager.Instance?.IsInitialized == true;

                if (!isReady)
                {
                    Debug.Log($"Test Scene not ready: Player={GameManager.Instance?.player != null}, " +
                             $"Camera={CameraManager.Instance?.IsInitialized}, " +
                             $"UI={UIManager.Instance?.playerUIPanel != null}, " +
                             $"GameUI={UIManager.Instance?.IsGameUIReady()}, " +
                             $"Monster={MonsterManager.Instance?.IsInitialized}");
                }

                return isReady;

            default:
                return true;
        }
    }

    private void CleanupCurrentScene()
    {
        var existingPortals = FindObjectsOfType<Portal>();
        foreach (var portal in existingPortals)
        {
            Destroy(portal.gameObject);
        }
        PoolManager.Instance?.ClearAllPools();
    }

    private bool IsUIReady(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.MainMenu:
                return UIManager.Instance.IsMainMenuActive();
            case SceneType.Town:
            case SceneType.Game:
                return UIManager.Instance.IsGameUIReady();
            case SceneType.Test:
                return UIManager.Instance.IsGameUIReady();
            default:
                return true;
        }
    }
    #endregion

    #region Portal Management
    public void SpawnGameStagePortal()
    {
        SpawnPortal(townPortalPosition, SceneType.Game);
    }

    public void SpawnTownPortal(Vector3 position)
    {
        SpawnPortal(position, SceneType.Town);
    }

    private void SpawnPortal(Vector3 position, SceneType destinationType)
    {
        if (portalPrefab != null)
        {
            GameObject portalObj = Instantiate(portalPrefab, position, Quaternion.identity);
            DontDestroyOnLoad(portalObj);

            if (portalObj.TryGetComponent<Portal>(out var portal))
            {
                portal.Initialize(destinationType);
            }
        }
    }

    public void OnPortalEnter(SceneType destinationType)
    {
        switch (destinationType)
        {
            case SceneType.Town:
                PlayerUnitManager.Instance.SaveGameState();
                LoadTownScene();
                break;
            case SceneType.Game:
                LoadGameScene();
                break;
        }
    }
    #endregion
}