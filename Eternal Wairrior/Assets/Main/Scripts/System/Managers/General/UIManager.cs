using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UIManager : SingletonManager<UIManager>
{
    [Header("UI Panels")]
    public Canvas mainCanvas;
    public GameObject pausePanel;
    public SkillLevelUpPanel levelupPanel;
    public PlayerSkillList skillList;

    [Header("Player Info Texts")]
    [SerializeField] private TextMeshProUGUI playerDefText;
    [SerializeField] private TextMeshProUGUI playerAtkText;
    [SerializeField] private TextMeshProUGUI playerMSText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI expCollectRadText;
    [SerializeField] private TextMeshProUGUI playerHpRegenText;
    [SerializeField] private TextMeshProUGUI playerAttackRangeText;
    [SerializeField] private TextMeshProUGUI playerAttackSpeedText;

    [Header("UI Bars")]
    [SerializeField] private Slider hpBarImage;
    [SerializeField] private Slider expBarImage;

    private bool isPaused = false;
    private Coroutine playerInfoUpdateCoroutine;
    private Coroutine levelCheckCoroutine;

    protected override void Awake()
    {
        base.Awake();
        InitializeUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
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

    private void SetupMainMenuUI()
    {
        StopAllCoroutines();
        // 메인 메뉴 UI 설정
    }

    private IEnumerator SetupGameSceneUI()
    {
        // 게임 매니저 초기화 대기
        while (GameManager.Instance?.player == null)
        {
            yield return null;
        }

        // UI 업데이트 시작
        StartPlayerInfoUpdate();
        StartLevelCheck();
    }

    private void SetupBossStageUI()
    {
        // 보스 스테이지 UI 설정
    }

    private void StartPlayerInfoUpdate()
    {
        if (playerInfoUpdateCoroutine != null)
            StopCoroutine(playerInfoUpdateCoroutine);

        playerInfoUpdateCoroutine = StartCoroutine(PlayerInfoUpdate());
    }

    private void StartLevelCheck()
    {
        if (levelCheckCoroutine != null)
            StopCoroutine(levelCheckCoroutine);

        levelCheckCoroutine = StartCoroutine(CheckLevelUp());
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

    private IEnumerator PlayerInfoUpdate()
    {
        Player player = GameManager.Instance.player;
        if (player == null)
        {
            Debug.LogError("Player reference is null in UIManager");
            yield break;
        }

        while (true)
        {
            yield return new WaitForEndOfFrame();
            UpdatePlayerInfo(player);
        }
    }

    private void UpdatePlayerInfo(Player player)
    {
        try
        {
            PlayerStat playerStat = GameManager.Instance?.player.GetComponent<PlayerStat>();

            playerAtkText.text = $"ATK : {playerStat.GetStat(StatType.Damage):F1}";
            playerDefText.text = $"DEF : {playerStat.GetStat(StatType.Defense):F1}";
            levelText.text = $"LEVEL : {player.level}";
            playerMSText.text = $"MoveSpeed : {playerStat.GetStat(StatType.MoveSpeed):F1}";

            float currentHp = playerStat.GetStat(StatType.CurrentHp);
            float maxHp = playerStat.GetStat(StatType.MaxHp);
            hpText.text = $"{currentHp:F0} / {maxHp:F0}";
            hpBarImage.value = currentHp / maxHp;

            expCollectRadText.text = $"ExpRad : {playerStat.GetStat(StatType.ExpCollectionRadius):F1}";
            playerHpRegenText.text = $"HPRegen : {playerStat.GetStat(StatType.HpRegenRate):F1}/s";
            playerAttackRangeText.text = $"AR : {playerStat.GetStat(StatType.AttackRange):F1}";
            playerAttackSpeedText.text = $"AS : {playerStat.GetStat(StatType.AttackSpeed):F1}/s";

            if (player.level >= player._expList.Count)
            {
                expText.text = "MAX LEVEL";
                expBarImage.value = 1;
            }
            else
            {
                float currentExp = player.CurrentExp();
                float requiredExp = player.GetExpForNextLevel();

                expText.text = $"EXP : {currentExp:F0}/{requiredExp:F0}";
                expBarImage.value = currentExp / requiredExp;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating player info: {e.Message}\n{e.StackTrace}");
        }
    }

    private IEnumerator CheckLevelUp()
    {
        Player player = GameManager.Instance.player;
        if (player == null)
        {
            Debug.LogError("Player reference is null in UIManager");
            yield break;
        }

        int lastLevel = player.level;

        while (true)
        {
            if (player.level > lastLevel)
            {
                Debug.Log($"Level Up detected: {lastLevel} -> {player.level}");
                lastLevel = player.level;
                ShowLevelUpPanel();
                Time.timeScale = 0f;
            }

            if (levelupPanel.gameObject.activeSelf)
            {
                Time.timeScale = 0f;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ShowLevelUpPanel()
    {
        if (levelupPanel != null)
        {
            levelupPanel.gameObject.SetActive(true);
            Time.timeScale = 0f;
            levelupPanel.LevelUpPanelOpen(GameManager.Instance.player.skills, OnSkillSelected);
        }
    }

    private void OnSkillSelected(Skill skill)
    {
        if (skill != null)
        {
            skillList.skillListUpdate();
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
        Time.timeScale = 1f;
    }
}
