using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public partial class UIManager : SingletonManager<UIManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("UI Panels")]
    public Canvas mainCanvas;
    public GameObject pausePanel;
    public SkillLevelUpPanel levelupPanel;
    public PlayerSkillList skillList;

    [Header("Player Info")]
    [SerializeField] public PlayerUIManager playerUIManager;

    [Header("UI Bars")]
    [SerializeField] private Slider hpBarImage;
    [SerializeField] private Slider expBarImage;

    [Header("Boss Warning UI")]
    [SerializeField] private GameObject bossWarningPanel;
    [SerializeField] private float warningDuration = 3f;
    private Coroutine bossWarningCoroutine;

    private bool isPaused = false;
    private Coroutine levelCheckCoroutine;

    [Header("Main Menu UI")]
    [SerializeField] private GameObject mainMenuPrefab;
    [SerializeField] private GameObject loadingScreenPrefab;

    public MainMenuPanel MainMenuPanel => mainMenuPanel;
    private MainMenuPanel mainMenuPanel;
    private LoadingScreen loadingScreen;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverTimerText;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (!GameLoopManager.Instance.IsInitialized)
        {
            Debug.LogWarning("Waiting for GameLoopManager to initialize...");
            return;
        }

        try
        {
            Debug.Log("Initializing UIManager...");
            InitializeUI();
            IsInitialized = true;
            Debug.Log("UIManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing UIManager: {e.Message}");
            IsInitialized = false;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        CleanupUI();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAllCoroutines();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                SetupMainMenuUI();
                break;
            case "GameScene":
            case "TestScene":
                StartCoroutine(SetupGameSceneUI());
                break;
            case "BossStage":
                SetupBossStageUI();
                break;
        }
    }

    private void InitializeUI()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (levelupPanel) levelupPanel.gameObject.SetActive(false);
    }

    public void SetupMainMenuUI()
    {
        Debug.Log("Starting SetupMainMenuUI");

        if (mainCanvas == null)
        {
            Debug.LogError("Main Canvas is not assigned!");
            return;
        }

        // 기존 UI 정리 전에 로그
        Debug.Log("Cleaning up existing UI");
        CleanupUI();

        // UI 초기화 전에 로그
        Debug.Log("Initializing main menu UI");
        InitializeMainMenuUI();

        // UI 표시 전에 로그
        Debug.Log("Showing main menu");
        ShowMainMenu();

        // 다른 UI 비활성화
        if (pausePanel)
        {
            pausePanel.SetActive(false);
            Debug.Log("Pause panel deactivated");
        }
        if (levelupPanel)
        {
            levelupPanel.gameObject.SetActive(false);
            Debug.Log("Level up panel deactivated");
        }
        if (playerUIManager)
        {
            playerUIManager.gameObject.SetActive(false);
            Debug.Log("Player UI deactivated");
        }
        if (bossWarningPanel)
        {
            bossWarningPanel.SetActive(false);
            Debug.Log("Boss warning panel deactivated");
        }

        HideLoadingScreen();
        Time.timeScale = 1f;

        Debug.Log("Main menu UI setup completed");
    }

    private IEnumerator SetupGameSceneUI()
    {
        while (GameManager.Instance?.player == null)
        {
            yield return null;
        }

        // PlayerUIManager가 비활성화되어 있다면 활성화
        if (playerUIManager != null && !playerUIManager.gameObject.activeSelf)
        {
            playerUIManager.gameObject.SetActive(true);
        }

        // 플레이어 UI 초기화
        playerUIManager?.InitializePlayerUI(GameManager.Instance.player);
    }

    private void SetupBossStageUI()
    {
        // 보스 스테이지 UI 설정
    }

    private void Update()
    {
        CheckPause();
    }

    private void CheckPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (pausePanel) pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ShowLevelUpPanel()
    {
        if (levelupPanel != null && GameManager.Instance?.player != null)
        {
            Debug.Log("Opening level up panel");
            levelupPanel.gameObject.SetActive(true);
            Time.timeScale = 0f;
            levelupPanel.LevelUpPanelOpen(GameManager.Instance.player.skills, OnSkillSelected);
        }
    }

    private void OnSkillSelected(Skill skill)
    {
        try
        {
            if (skill != null)
            {
                Debug.Log($"Skill selected: {skill.SkillName}");
                skillList.skillListUpdate();
                Time.timeScale = 1f;
                levelupPanel.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No skill selected in level up panel");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnSkillSelected: {e.Message}");
            Time.timeScale = 1f;
            levelupPanel.gameObject.SetActive(false);
        }
    }

    // UI 요소 초기화/정리 메서드
    public void ClearUI()
    {
        StopAllCoroutines();
        if (pausePanel) pausePanel.SetActive(false);
        if (levelupPanel) levelupPanel.gameObject.SetActive(false);
        playerUIManager.Clear();
        Time.timeScale = 1f;
    }

    public void ShowBossWarning()
    {
        if (bossWarningCoroutine != null)
        {
            StopCoroutine(bossWarningCoroutine);
        }
        bossWarningCoroutine = StartCoroutine(ShowBossWarningCoroutine());
    }

    private IEnumerator ShowBossWarningCoroutine()
    {
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(true);
            yield return new WaitForSeconds(warningDuration);
            bossWarningPanel.SetActive(false);
        }
        bossWarningCoroutine = null;
    }

    private void InitializeMainMenuUI()
    {
        if (mainMenuPanel == null && mainMenuPrefab != null)
        {
            if (mainCanvas == null)
            {
                Debug.LogError("Main Canvas is null during menu initialization!");
                return;
            }

            Debug.Log($"Creating main menu UI from prefab: {mainMenuPrefab.name}");
            var menuObj = Instantiate(mainMenuPrefab, mainCanvas.transform);
            mainMenuPanel = menuObj.GetComponent<MainMenuPanel>();

            if (mainMenuPanel == null)
            {
                Debug.LogError("Failed to get MainMenuPanel component!");
            }
            else
            {
                Debug.Log("MainMenuPanel component found successfully");
            }
        }
        else
        {
            Debug.Log($"MainMenuPanel already exists or prefab is null. Panel: {mainMenuPanel}, Prefab: {mainMenuPrefab}");
        }
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel == null)
        {
            InitializeMainMenuUI();
        }
        mainMenuPanel?.gameObject.SetActive(true);
        mainMenuPanel?.UpdateButtons(GameManager.Instance.HasSaveData());
    }

    public void HideMainMenu()
    {
        mainMenuPanel?.gameObject.SetActive(false);
    }

    private void InitializeLoadingScreen()
    {
        if (loadingScreen == null && loadingScreenPrefab != null)
        {
            if (mainCanvas == null)
            {
                Debug.LogError("Main Canvas is null during loading screen initialization!");
                return;
            }

            Debug.Log($"Creating loading screen from prefab: {loadingScreenPrefab.name}");
            var loadingObj = Instantiate(loadingScreenPrefab, mainCanvas.transform);
            loadingScreen = loadingObj.GetComponent<LoadingScreen>();

            if (loadingScreen == null)
            {
                Debug.LogError("Failed to get LoadingScreen component!");
            }
            else
            {
                loadingScreen.gameObject.SetActive(false);
                Debug.Log("LoadingScreen component initialized successfully");
            }
        }
    }

    public void ShowLoadingScreen()
    {
        if (loadingScreen == null)
        {
            InitializeLoadingScreen();
        }

        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(true);
            loadingScreen.ResetProgress();  // 진행률 초기화
            Debug.Log("Loading screen shown");
        }
        else
        {
            Debug.LogError("Failed to show loading screen - loading screen is null");
        }
    }

    public void HideLoadingScreen()
    {
        loadingScreen?.gameObject.SetActive(false);
    }

    public void UpdateLoadingProgress(float progress)
    {
        loadingScreen?.UpdateProgress(progress);
    }

    public void OnStartNewGame()
    {
        GameManager.Instance.InitializeNewGame();
        StartCoroutine(LoadTownScene());
    }

    public void OnLoadGame()
    {
        if (!GameManager.Instance.HasSaveData()) return;
        GameManager.Instance.LoadGameData();
        StartCoroutine(LoadTownScene());
    }

    private IEnumerator LoadTownScene()
    {
        StageManager.Instance.LoadTownScene();
        yield break;
    }

    public void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CleanupUI()
    {
        if (mainMenuPanel != null)
        {
            Destroy(mainMenuPanel.gameObject);
            mainMenuPanel = null;
        }
        if (loadingScreen != null)
        {
            Destroy(loadingScreen.gameObject);
            loadingScreen = null;
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void HideGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void UpdateGameOverTimer(float time)
    {
        if (gameOverTimerText != null)
        {
            gameOverTimerText.text = $"타운으로 돌아가기: {time:F1}초";
        }
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void InitializePlayerUI(Player player)
    {
        if (playerUIManager != null)
        {
            playerUIManager.InitializePlayerUI(player);
            GameManager.Instance.StartLevelCheck();
        }
    }

    public bool IsMainMenuActive()
    {
        return mainMenuPanel != null && mainMenuPanel.gameObject.activeSelf;
    }

    public void SetupGameUI()
    {
        // PlayerUI 초기화
        if (playerUIManager != null)
        {
            playerUIManager.gameObject.SetActive(true);
            playerUIManager.PrepareUI();  // UI 컴포넌트들만 초기화
        }
    }

    public bool IsGameUIReady()
    {
        return playerUIManager != null &&
               playerUIManager.gameObject.activeSelf &&
               playerUIManager.IsUIReady();
    }
}
