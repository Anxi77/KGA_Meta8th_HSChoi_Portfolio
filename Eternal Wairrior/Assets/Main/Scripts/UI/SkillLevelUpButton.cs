using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;

public class SkillLevelUpButton : MonoBehaviour
{
    [SerializeField] private Image skillIconImage;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statComparisonText;
    [SerializeField] private Image elementIcon;
    [SerializeField] private Button button;

    public void SetSkillSelectButton(SkillData skillData, Action onClick, string levelInfo = null, ISkillStat currentStats = null)
    {
        if (skillData == null)
        {
            SetDisabledButton("Invalid Skill Data");
            return;
        }

        SetupSkillIcon(skillData);
        SetupSkillText(skillData, levelInfo);
        SetupElementIcon(skillData);
        SetupButton(onClick);

        if (currentStats != null)
        {
            SetupStatComparison(skillData, currentStats);
        }
    }

    private void SetupSkillIcon(SkillData skillData)
    {
        if (skillIconImage != null)
        {
            skillIconImage.sprite = skillData.icon;
            skillIconImage.gameObject.SetActive(skillData.icon != null);
        }
    }

    private void SetupSkillText(SkillData skillData, string levelInfo)
    {
        if (skillNameText != null)
        {
            string nameText = skillData.metadata.Name;
            if (!string.IsNullOrEmpty(levelInfo))
            {
                nameText += $" ({levelInfo})";
            }
            skillNameText.text = nameText;
        }

        if (descriptionText != null)
        {
            Skill skillComponent = skillData.metadata.Prefab?.GetComponent<Skill>();
            string description = skillComponent != null ?
                skillComponent.GetDetailedDescription() :
                skillData.metadata.Description;

            string elementDesc = GetElementalDescription(skillData.metadata.Element);
            descriptionText.text = $"{description}\n{elementDesc}";
        }
    }

    private void SetupElementIcon(SkillData skillData)
    {
        if (elementIcon != null)
        {
            elementIcon.sprite = GetElementSprite(skillData.metadata.Element);
            elementIcon.gameObject.SetActive(skillData.metadata.Element != ElementType.None);
        }
    }

    private void SetupButton(Action onClick)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    public void SetDisabledButton(string message)
    {
        if (skillIconImage != null) skillIconImage.gameObject.SetActive(false);
        if (elementIcon != null) elementIcon.gameObject.SetActive(false);
        if (skillNameText != null) skillNameText.text = message;
        if (descriptionText != null) descriptionText.text = "";
        if (button != null) button.interactable = false;
    }

    private string GetElementalDescription(ElementType element)
    {
        return element switch
        {
            ElementType.Dark => "Dark: Reduces defense",
            ElementType.Water => "Water: Slows movement",
            ElementType.Fire => "Fire: Deals damage over time",
            ElementType.Earth => "Earth: Stuns targets",
            _ => ""
        };
    }

    private Sprite GetElementSprite(ElementType element)
    {
        string iconPath = element switch
        {
            ElementType.Dark => "Icons/DarkElement",
            ElementType.Water => "Icons/WaterElement",
            ElementType.Fire => "Icons/FireElement",
            ElementType.Earth => "Icons/EarthElement",
            _ => ""
        };
        return !string.IsNullOrEmpty(iconPath) ? Resources.Load<Sprite>(iconPath) : null;
    }

    private void SetupStatComparison(SkillData skillData, ISkillStat currentStats)
    {
        if (statComparisonText == null) return;

        StringBuilder comparison = new StringBuilder();
        comparison.AppendLine("Stat Difference:");

        var currentLevel = currentStats.baseStat.skillLevel;
        var nextLevelStats = SkillDataManager.Instance.GetSkillStatsForLevel(
            skillData.metadata.ID,
            currentLevel + 1,
            skillData.metadata.Type);

        if (nextLevelStats == null) return;

        CompareBasicStats(comparison, currentStats.baseStat, nextLevelStats.baseStat);

        // 스킬 타입별 스탯 비교
        switch (skillData.metadata.Type)
        {
            case SkillType.Projectile:
                CompareProjectileStats(comparison, currentStats as ProjectileSkillStat, nextLevelStats as ProjectileSkillStat);
                break;
            case SkillType.Area:
                CompareAreaStats(comparison, currentStats as AreaSkillStat, nextLevelStats as AreaSkillStat);
                break;
            case SkillType.Passive:
                ComparePassiveStats(comparison, currentStats as PassiveSkillStat, nextLevelStats as PassiveSkillStat);
                break;
        }

        statComparisonText.text = comparison.ToString();
    }

    private void CompareBasicStats(StringBuilder sb, BaseSkillStat current, BaseSkillStat next)
    {
        float damageDiff = next.damage - current.damage;
        if (damageDiff != 0)
        {
            sb.AppendLine($"Damage : {current.damage:F1} → {next.damage:F1} (<color={GetColorForDiff(damageDiff)}>{"+" + damageDiff:F1}</color>)");
        }

        float elementalPowerDiff = next.elementalPower - current.elementalPower;
        if (elementalPowerDiff != 0)
        {
            sb.AppendLine($"Elemental Power: {current.elementalPower:F1} → {next.elementalPower:F1} (<color={GetColorForDiff(elementalPowerDiff)}>{"+" + elementalPowerDiff:F1}</color>)");
        }
    }

    private void CompareProjectileStats(StringBuilder sb, ProjectileSkillStat current, ProjectileSkillStat next)
    {
        if (current == null || next == null) return;

        CompareFloatValue(sb, "Projectile Speed", current.projectileSpeed, next.projectileSpeed);
        CompareFloatValue(sb, "Scale", current.projectileScale, next.projectileScale, false, "x");
        CompareFloatValue(sb, "Shot Interval", current.shotInterval, next.shotInterval, true, "s");
        CompareIntValue(sb, "Pierce Count", current.pierceCount, next.pierceCount);
        CompareFloatValue(sb, "Attack Range", current.attackRange, next.attackRange);
        CompareFloatValue(sb, "Homing Range", current.homingRange, next.homingRange);
        CompareIntValue(sb, "Projectile Count", current.projectileCount, next.projectileCount);
        CompareFloatValue(sb, "Inner Interval", current.innerInterval, next.innerInterval, true, "s");
        CompareFloatValue(sb, "Explosion Radius", current.explosionRad, next.explosionRad);
    }

    private void CompareAreaStats(StringBuilder sb, AreaSkillStat current, AreaSkillStat next)
    {
        if (current == null || next == null) return;

        CompareFloatValue(sb, "Radius", current.radius, next.radius);
        CompareFloatValue(sb, "Duration", current.duration, next.duration, false, "s");
        CompareFloatValue(sb, "Tick Rate", current.tickRate, next.tickRate, true, "s");
        CompareFloatValue(sb, "Move Speed", current.moveSpeed, next.moveSpeed);
        CompareBoolValue(sb, "Persistence", current.isPersistent, next.isPersistent);
    }

    private void ComparePassiveStats(StringBuilder sb, PassiveSkillStat current, PassiveSkillStat next)
    {
        if (current == null || next == null) return;

        // Base stats
        CompareFloatValue(sb, "Effect Duration", current.effectDuration, next.effectDuration);
        CompareFloatValue(sb, "Cooldown", current.cooldown, next.cooldown, true);
        CompareFloatValue(sb, "Trigger Chance", current.triggerChance, next.triggerChance);
        CompareFloatValue(sb, "Damage Increase", current.damageIncrease, next.damageIncrease, false, "%");
        CompareFloatValue(sb, "Defense Increase", current.defenseIncrease, next.defenseIncrease, false, "%");
        CompareFloatValue(sb, "EXP Range Increase", current.expAreaIncrease, next.expAreaIncrease, false, "%");
        CompareFloatValue(sb, "HP Increase", current.hpIncrease, next.hpIncrease, false, "%");

        // Additional stats
        CompareFloatValue(sb, "Move Speed Increase", current.moveSpeedIncrease, next.moveSpeedIncrease, false, "%");
        CompareFloatValue(sb, "Attack Speed Increase", current.attackSpeedIncrease, next.attackSpeedIncrease, false, "%");
        CompareFloatValue(sb, "Attack Range Increase", current.attackRangeIncrease, next.attackRangeIncrease, false, "%");
        CompareFloatValue(sb, "HP Regen Increase", current.hpRegenIncrease, next.hpRegenIncrease, false, "%");
    }

    private void CompareFloatValue(StringBuilder sb, string statName, float current, float next, bool isNegativeBetter = false, string unit = "")
    {
        float diff = next - current;
        if (diff != 0)
        {
            string color = isNegativeBetter ? GetColorForDiff(-diff) : GetColorForDiff(diff);
            string diffText = diff > 0 ? $"+{diff:F1}" : $"{diff:F1}";
            sb.AppendLine($"{statName}: {current:F1}{unit} → {next:F1}{unit} (<color={color}>{diffText}{unit}</color>)");
        }
    }

    private void CompareIntValue(StringBuilder sb, string statName, int current, int next, bool isNegativeBetter = false)
    {
        int diff = next - current;
        if (diff != 0)
        {
            string color = isNegativeBetter ? GetColorForDiff(-diff) : GetColorForDiff(diff);
            sb.AppendLine($"{statName}: {current} → {next} (<color={color}>{(diff > 0 ? "+" : "")}{diff}</color>)");
        }
    }

    private string GetColorForDiff(float diff)
    {
        return diff > 0 ? "#00FF00" : "#FF0000"; 
    }

    private void CompareBoolValue(StringBuilder sb, string statName, bool current, bool next)
    {
        if (current != next)
        {
            string color = next ? "#00FF00" : "#FF0000";
            sb.AppendLine($"{statName}: {current} → {next} (<color={color}>{(next ? "Activated" : "Deactivated")}</color>)");
        }
    }
}