using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerUIManager : SingletonManager<PlayerUIManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

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

    private Player player;
    private PlayerStat playerStat;
    private Coroutine updateCoroutine;

    private bool isUIReady = false;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    public void Initialize()
    {
        if (!UIManager.Instance.IsInitialized)
        {
            Debug.LogWarning("Waiting for UIManager to initialize...");
            return;
        }

        try
        {
            Debug.Log("Initializing PlayerUIManager...");
            IsInitialized = true;
            Debug.Log("PlayerUIManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing PlayerUIManager: {e.Message}");
            IsInitialized = false;
        }
    }

    public void PrepareUI()
    {
        if (playerDefText) playerDefText.text = "DEF : 0";
        if (playerAtkText) playerAtkText.text = "ATK : 0";
        if (playerMSText) playerMSText.text = "MoveSpeed : 0";
        if (levelText) levelText.text = "LEVEL : 1";
        if (expText) expText.text = "EXP : 0/0";
        if (hpText) hpText.text = "0 / 0";
        if (expCollectRadText) expCollectRadText.text = "ExpRad : 0";
        if (playerHpRegenText) playerHpRegenText.text = "HPRegen : 0/s";
        if (playerAttackRangeText) playerAttackRangeText.text = "AR : 0";
        if (playerAttackSpeedText) playerAttackSpeedText.text = "AS : 0/s";

        if (hpBarImage) hpBarImage.value = 0;
        if (expBarImage) expBarImage.value = 0;

        isUIReady = true;
        Debug.Log("PlayerUI prepared");
    }

    public bool IsUIReady()
    {
        return isUIReady &&
               playerDefText != null &&
               playerAtkText != null &&
               playerMSText != null &&
               levelText != null &&
               expText != null &&
               hpText != null;
    }

    public void InitializePlayerUI(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Cannot initialize PlayerUI with null player");
            return;
        }

        this.player = player;
        this.playerStat = player.GetComponent<PlayerStat>();
        StartUIUpdate();
        isUIReady = true;
        Debug.Log("PlayerUI fully initialized with player reference");
    }

    private void StartUIUpdate()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
        updateCoroutine = StartCoroutine(PlayerInfoUpdate());
    }

    private IEnumerator PlayerInfoUpdate()
    {
        while (true)
        {
            if (player != null && playerStat != null)
            {
                UpdatePlayerInfo();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void UpdatePlayerInfo()
    {
        try
        {
            UpdateCombatStats();
            UpdateHealthUI();
            UpdateExpUI();
            UpdateOtherStats();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating player info: {e.Message}");
        }
    }

    private void UpdateCombatStats()
    {
        playerAtkText.text = $"ATK : {playerStat.GetStat(StatType.Damage):F1}";
        playerDefText.text = $"DEF : {playerStat.GetStat(StatType.Defense):F1}";
        levelText.text = $"LEVEL : {player.level}";
        playerMSText.text = $"MoveSpeed : {playerStat.GetStat(StatType.MoveSpeed):F1}";
    }

    private void UpdateHealthUI()
    {
        float currentHp = playerStat.GetStat(StatType.CurrentHp);
        float maxHp = playerStat.GetStat(StatType.MaxHp);
        hpText.text = $"{currentHp:F0} / {maxHp:F0}";
        hpBarImage.value = currentHp / maxHp;
    }

    private void UpdateExpUI()
    {
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

    private void UpdateOtherStats()
    {
        expCollectRadText.text = $"ExpRad : {playerStat.GetStat(StatType.ExpCollectionRadius):F1}";
        playerHpRegenText.text = $"HPRegen : {playerStat.GetStat(StatType.HpRegenRate):F1}/s";
        playerAttackRangeText.text = $"AR : {playerStat.GetStat(StatType.AttackRange):F1}";
        playerAttackSpeedText.text = $"AS : {playerStat.GetStat(StatType.AttackSpeed):F1}/s";
    }

    public void Clear()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        player = null;
        playerStat = null;
        isUIReady = false;
    }

    private void OnDisable()
    {
        Clear();
    }
}