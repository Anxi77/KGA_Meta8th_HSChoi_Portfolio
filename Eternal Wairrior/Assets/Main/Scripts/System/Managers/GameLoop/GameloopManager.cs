using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoopManager : SingletonManager<GameLoopManager>, IInitializable
{
    public enum GameState
    {
        MainMenu,
        Town,
        Stage,
        Paused,
        GameOver
    }

    public enum InitializationState
    {
        None,
        DataManagers,    // PlayerDataManager, ItemDataManager, SkillDataManager
        CoreManagers,    // GameManager, UIManager, PoolManager
        GameplayManagers,// PlayerUnitManager, MonsterManager, etc.
        Complete
    }

    private GameState currentState = GameState.MainMenu;
    public GameState CurrentState => currentState;
    public bool IsInitialized { get; private set; }

    private Dictionary<GameState, IGameStateHandler> stateHandlers;
    private InitializationState currentInitState = InitializationState.None;

    public void Initialize()
    {
        if (!IsInitialized)
        {
            StartInitialization();
        }
    }

    public void StartInitialization()
    {
        if (!IsInitialized)
        {
            StartCoroutine(InitializationSequence());
        }
    }

    private IEnumerator InitializationSequence()
    {
        Debug.Log("Starting manager initialization sequence...");

        // 1. Data Managers 초기화
        currentInitState = InitializationState.DataManagers;
        bool dataManagersInitialized = false;
        yield return StartCoroutine(InitializeDataManagers(success => dataManagersInitialized = success));

        if (!dataManagersInitialized)
        {
            Debug.LogError("Data Managers initialization failed");
            yield break;
        }

        // 2. Core Managers 초기화
        currentInitState = InitializationState.CoreManagers;
        bool coreManagersInitialized = false;
        yield return StartCoroutine(InitializeCoreManagers(success => coreManagersInitialized = success));

        if (!coreManagersInitialized)
        {
            Debug.LogError("Core Managers initialization failed");
            yield break;
        }

        // 3. Gameplay Managers 초기화
        currentInitState = InitializationState.GameplayManagers;
        bool gameplayManagersInitialized = false;
        yield return StartCoroutine(InitializeGameplayManagers(success => gameplayManagersInitialized = success));

        if (!gameplayManagersInitialized)
        {
            Debug.LogError("Gameplay Managers initialization failed");
            yield break;
        }

        // 4. State Handlers 생성 및 초기화
        if (!CreateStateHandlers())
        {
            Debug.LogError("State Handlers creation failed");
            yield break;
        }

        currentInitState = InitializationState.Complete;
        IsInitialized = true;

        Debug.Log("All managers initialized successfully");

        // StageManager를 통해 메인 메뉴 씬 로드
        if (StageManager.Instance != null)
        {
            Debug.Log("Loading main menu scene...");
            StageManager.Instance.LoadMainMenu();

            // 씬 로드가 완료될 때까지 대기
            yield return new WaitForSeconds(0.5f);

            // 상태 변경
            ChangeState(GameState.MainMenu);
            Debug.Log("Changed state to MainMenu");
        }
        else
        {
            Debug.LogError("StageManager is null, cannot load main menu!");
        }
    }

    private IEnumerator InitializeDataManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Data Managers...");

        // PlayerDataManager 초기화
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.Initialize();
            while (!PlayerDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PlayerDataManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PlayerDataManager initialized");
        }

        // ItemDataManager 초기화
        if (ItemDataManager.Instance != null)
        {
            ItemDataManager.Instance.Initialize();
            while (!ItemDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(ItemDataManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("ItemDataManager initialized");
        }

        // SkillDataManager 초기화
        if (SkillDataManager.Instance != null)
        {
            SkillDataManager.Instance.Initialize();
            while (!SkillDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(SkillDataManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("SkillDataManager initialized");
        }

        Debug.Log("All Data Managers initialized");
        onComplete?.Invoke(true);
    }

    private bool CheckInitializationError(IInitializable manager)
    {
        // 여기서 초기화 중 발생할 수 있는 오류 상태를 체크
        // 예: 타임아웃, 특정 조건 실패 등
        return false; // 오류가 없으면 false 반환
    }

    private IEnumerator InitializeCoreManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Core Managers...");

        // PoolManager 초기화
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Initialize();
            while (!PoolManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PoolManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PoolManager initialized");
        }

        // GameManager 초기화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Initialize();
            while (!GameManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(GameManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("GameManager initialized");
        }

        // CameraManager 초기화
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.Initialize();
            while (!CameraManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(CameraManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("CameraManager initialized");
        }

        // UIManager는 마지막에 초기화
        if (UIManager.Instance != null)
        {
            // GameLoopManager가 이미 초기화되어 있음을 보장
            IsInitialized = true;

            UIManager.Instance.Initialize();
            while (!UIManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(UIManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("UIManager initialized");
        }

        Debug.Log("All Core Managers initialized");
        onComplete?.Invoke(true);
    }

    private IEnumerator InitializeGameplayManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Gameplay Managers...");

        // SkillManager 초기화
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.Initialize();
            while (!SkillManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(SkillManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("SkillManager initialized");
        }

        // PlayerUnitManager 초기화
        if (PlayerUnitManager.Instance != null)
        {
            PlayerUnitManager.Instance.Initialize();
            while (!PlayerUnitManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PlayerUnitManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PlayerUnitManager initialized");
        }

        // MonsterManager 초기화
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.Initialize();
            while (!MonsterManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(MonsterManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("MonsterManager initialized");
        }

        // StageTimeManager 초기화
        if (StageTimeManager.Instance != null)
        {
            StageTimeManager.Instance.Initialize();
            while (!StageTimeManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(StageTimeManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("StageTimeManager initialized");
        }

        // PlayerUIManager 초기화
        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.Initialize();
            while (!PlayerUIManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PlayerUIManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PlayerUIManager initialized");
        }

        // ItemManager 초기화
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.Initialize();
            while (!ItemManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(ItemManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("ItemManager initialized");
        }

        Debug.Log("All Gameplay Managers initialized");
        onComplete?.Invoke(true);
    }

    private bool CreateStateHandlers()
    {
        Debug.Log("Creating state handlers...");
        stateHandlers = new Dictionary<GameState, IGameStateHandler>();

        try
        {
            // 각 StateHandler 인스턴스 생성 및 초기화
            stateHandlers[GameState.MainMenu] = new MainMenuStateHandler();
            stateHandlers[GameState.Town] = new TownStateHandler();
            stateHandlers[GameState.Stage] = new StageStateHandler();
            stateHandlers[GameState.Paused] = new PausedStateHandler();
            stateHandlers[GameState.GameOver] = new GameOverStateHandler();

            Debug.Log("All state handlers created successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating state handlers: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState || !IsInitialized || stateHandlers == null)
            return;

        Debug.Log($"Changing state from {currentState} to {newState}");

        try
        {
            // 현재 상태 종료
            if (stateHandlers.ContainsKey(currentState))
            {
                stateHandlers[currentState].OnExit();
            }

            // 새로운 상태로 전환
            currentState = newState;

            // 새로운 상태 시작
            if (stateHandlers.ContainsKey(currentState))
            {
                stateHandlers[currentState].OnEnter();
            }

            Debug.Log($"Successfully changed to state: {newState}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during state change: {e.Message}\n{e.StackTrace}");
        }
    }

    private void Update()
    {
        if (!IsInitialized || stateHandlers == null) return;

        try
        {
            stateHandlers[currentState]?.OnUpdate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in state update: {e.Message}");
        }
    }

    private void FixedUpdate()
    {
        if (!IsInitialized || stateHandlers == null) return;

        try
        {
            stateHandlers[currentState]?.OnFixedUpdate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in state fixed update: {e.Message}");
        }
    }

    public T GetCurrentHandler<T>() where T : class, IGameStateHandler
    {
        if (!IsInitialized || stateHandlers == null) return null;

        if (stateHandlers.TryGetValue(currentState, out var handler))
        {
            return handler as T;
        }
        return null;
    }

    private void OnDestroy()
    {
        IsInitialized = false;
        stateHandlers?.Clear();
        StopAllCoroutines();
    }
}

// 게임 상태 핸들러 인터페이스
public interface IGameStateHandler
{
    void OnEnter();
    void OnUpdate();
    void OnFixedUpdate();
    void OnExit();
}
