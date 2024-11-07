using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerUIManager : MonoBehaviour
{
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

    public void Initialize(Player player)
    {
        this.player = player;
        this.playerStat = player.GetComponent<PlayerStat>();
        StartUIUpdate();
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
            playerAtkText.text = $"ATK : {playerStat.GetStat(StatType.Damage):F1}";
            playerDefText.text = $"DEF : {playerStat.GetStat(StatType.Defense):F1}";
            levelText.text = $"LEVEL : {player.level}";
            playerMSText.text = $"MoveSpeed : {playerStat.GetStat(StatType.MoveSpeed):F1}";

            UpdateHealthUI();
            UpdateExpUI();
            UpdateOtherStats();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating player info: {e.Message}");
        }
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
        }
    }

    private void OnDisable()
    {
        Clear();
    }
}