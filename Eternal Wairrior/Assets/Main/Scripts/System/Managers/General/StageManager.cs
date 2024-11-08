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

        // 로딩 화면 표시 및 게임 일시 정지
        UIManager.Instance.ShowLoadingScreen();
        Time.timeScale = 0f;

        // 초기 로딩 지연
        yield return new WaitForSecondsRealtime(2f);

        // 씬 전환 전 정리
        CleanupCurrentScene();

        // 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float progress = 0f;
        while (asyncLoad.progress < 0.9f)
        {
            progress = Mathf.MoveTowards(progress, asyncLoad.progress, Time.unscaledDeltaTime);
            UIManager.Instance.UpdateLoadingProgress(progress);
            yield return null;
        }

        // 추가 로딩 시간
        float artificialLoadingTime = 0f;
        while (artificialLoadingTime < 5f)
        {
            artificialLoadingTime += Time.unscaledDeltaTime;
            progress = Mathf.Lerp(0.9f, 1f, artificialLoadingTime / 5f);
            UIManager.Instance.UpdateLoadingProgress(progress);
            yield return null;
        }

        Debug.Log($"Scene {sceneName} loaded to 90%, activating...");
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log($"Scene {sceneName} loaded, initializing UI...");

        // UI 초기화
        switch (sceneType)
        {
            case SceneType.MainMenu:
                UIManager.Instance.SetupMainMenuUI();
                break;
            case SceneType.Town:
            case SceneType.Game:
                UIManager.Instance.SetupGameUI();
                break;
        }

        // UI가 준비될 때까지 대기
        yield return new WaitUntil(() => IsUIReady(sceneType));

        // GameLoopManager 상태 변경
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

        // 상태 핸들러의 초기화가 완료될 때까지 대기
        yield return new WaitUntil(() => IsSceneInitializationComplete(sceneType));

        // 최종 로딩 지연
        yield return new WaitForSecondsRealtime(1f);

        // 모든 초기화가 완료된 후에 로딩 화면을 숨기고 게임 재개
        UIManager.Instance.HideLoadingScreen();
        Time.timeScale = 1f;
        Debug.Log($"Scene {sceneName} initialization complete");
    }

    private bool IsSceneInitializationComplete(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Town:
                return GameManager.Instance?.player != null &&
                       CameraManager.Instance?.IsInitialized == true &&
                       UIManager.Instance?.playerUIManager?.gameObject.activeSelf == true;

            case SceneType.Game:
            case SceneType.Test:
                return GameManager.Instance?.player != null &&
                       CameraManager.Instance?.IsInitialized == true &&
                       UIManager.Instance?.playerUIManager?.gameObject.activeSelf == true &&
                       MonsterManager.Instance?.IsInitialized == true;

            case SceneType.MainMenu:
                return UIManager.Instance != null && UIManager.Instance.IsMainMenuActive();

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