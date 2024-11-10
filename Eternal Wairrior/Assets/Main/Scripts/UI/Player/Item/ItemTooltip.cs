using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemRarityText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private TextMeshProUGUI itemEffectsText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        ValidateComponents();
        Hide();
    }

    private void ValidateComponents()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetupTooltip(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogError("Attempted to setup tooltip with null ItemData");
            return;
        }

        Debug.Log($"Setting up tooltip for item: {itemData.name}");
        Debug.Log($"Item stats count: {itemData.stats?.Count ?? 0}");

        // 기본 정보 설정
        itemNameText.text = $"{GetRarityColor(itemData.rarity)}{itemData.name}</color>";
        itemTypeText.text = $"Type: {itemData.type}";
        itemRarityText.text = $"Rarity: {itemData.rarity}";

        // 아이콘 설정
        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.icon;
            itemIcon.enabled = itemData.icon != null;
        }

        // 스탯 정보 설정
        var statsBuilder = new System.Text.StringBuilder("Stats:\n");
        if (itemData.stats != null && itemData.stats.Any())
        {
            foreach (var stat in itemData.stats)
            {
                string valueStr = stat.amount >= 0 ? "+" + stat.amount : stat.amount.ToString();
                statsBuilder.AppendLine($"{stat.statType}: {valueStr}");
                Debug.Log($"Adding stat to tooltip: {stat.statType} = {valueStr}");
            }
        }
        else
        {
            statsBuilder.AppendLine("No stats");
            Debug.Log("No stats found for item");
        }
        itemStatsText.text = statsBuilder.ToString();

        // 효과 정보 설정
        var effectsBuilder = new System.Text.StringBuilder("Effects:\n");
        if (itemData.effects != null && itemData.effects.Any())
        {
            foreach (var effect in itemData.effects)
            {
                effectsBuilder.AppendLine($"{effect.effectName}");
                Debug.Log($"Adding effect to tooltip: {effect.effectName}");
            }
        }
        else
        {
            effectsBuilder.AppendLine("No effects");
            Debug.Log("No effects found for item");
        }
        itemEffectsText.text = effectsBuilder.ToString();

        Debug.Log($"Tooltip setup complete for {itemData.name}");
    }

    private string GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "<color=white>",
            ItemRarity.Uncommon => "<color=#00FF00>",  // 초록색
            ItemRarity.Rare => "<color=#0080FF>",      // 파란색
            ItemRarity.Epic => "<color=#CC33FF>",      // 보라색
            ItemRarity.Legendary => "<color=#FFD700>",  // 금색
            _ => "<color=white>"
        };
    }

    public void Show(Vector2 position)
    {
        transform.position = position;
        canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            Vector2 mousePos = Input.mousePosition;
            transform.position = mousePos + new Vector2(10f, -10f);
        }
    }
}