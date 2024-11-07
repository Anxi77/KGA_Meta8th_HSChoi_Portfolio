using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : SingletonManager<LobbyManager>
{
    private void Start()
    {
        // 로비 진입 시 초기화
        InitializeLobby();
    }

    private void InitializeLobby()
    {
        // 기존 데이터 초기화
        GameManager.Instance?.ClearGameData();
        StageTimeManager.Instance?.ResetTimer();

        // UI 초기화
        UpdateUI();
    }

    public void OnStartNewGame()
    {
        StartCoroutine(StartNewGameCoroutine());
    }

    public void OnLoadGame()
    {
        StartCoroutine(LoadGameCoroutine());
    }

    private IEnumerator StartNewGameCoroutine()
    {
        // 1. 새 게임 데이터 초기화
        GameManager.Instance.InitializeNewGame();

        // 2. 마을로 이동
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TownScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. 새 플레이어 초기화
        PlayerUnitManager.Instance.InitializeNewPlayer();

        // 4. 플레이어 시작 위치 설정
        GameManager.Instance.player.transform.position = GetTownStartPosition();
    }

    private IEnumerator LoadGameCoroutine()
    {
        // 1. 저장된 게임 데이터가 있는지 확인
        if (!GameManager.Instance.playerDataManager.HasSaveData("CurrentSave"))
        {
            Debug.LogWarning("No saved game found!");
            yield break;
        }

        // 2. 게임 데이터 로드
        GameManager.Instance.LoadGameData();

        // 3. 마지막 저장된 씬으로 이동 (기본값: 마을)
        string savedScene = GameManager.Instance.GetLastSavedScene();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(savedScene);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. 저장된 플레이어 데이터 로드
        PlayerUnitManager.Instance.LoadPlayerData();

        // 5. 플레이어 위치 복원
        Vector3 savedPosition = GameManager.Instance.GetLastSavedPosition();
        GameManager.Instance.player.transform.position = savedPosition;
    }

    private Vector3 GetTownStartPosition()
    {
        // 마을 시작 위치 반환
        return new Vector3(0, 0, 0); // 실제 시작 위치로 수정 필요
    }

    private void UpdateUI()
    {
        // 저장된 게임이 있는지 확인하여 UI 업데이트
        bool hasSaveData = GameManager.Instance.HasSaveData();
        UIManager.Instance.UpdateLobbyUI(hasSaveData);
    }

    public void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}