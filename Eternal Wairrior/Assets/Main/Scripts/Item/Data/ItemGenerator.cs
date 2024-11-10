using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    private Dictionary<string, ItemData> itemDatabase;
    private System.Random random = new System.Random();

    public ItemGenerator(Dictionary<string, ItemData> database)
    {
        itemDatabase = database;
    }

    public ItemData GenerateItem(string itemId, ItemRarity? targetRarity = null)
    {
        if (!itemDatabase.TryGetValue(itemId, out var baseItem))
        {
            Debug.LogError($"Item not found in database: {itemId}");
            return null;
        }

        var newItem = baseItem.Clone();

        // 레어리티 설정
        if (targetRarity.HasValue)
        {
            newItem.rarity = targetRarity.Value;
        }

        Debug.Log($"Generating item: {newItem.name} with rarity: {newItem.rarity}");

        // 스탯 생성
        GenerateStats(newItem);

        // 이펙트 생성
        GenerateEffects(newItem);

        return newItem;
    }

    private void GenerateStats(ItemData item)
    {
        if (item.statRanges == null || item.statRanges.possibleStats == null)
        {
            Debug.LogWarning($"No stat ranges defined for item: {item.id}");
            return;
        }

        item.stats.Clear();

        // 레어리티에 따른 추가 스탯 수 계산
        int additionalStats = item.statRanges.additionalStatsByRarity.GetValueOrDefault(item.rarity, 0);
        int statCount = random.Next(
            item.statRanges.minStatCount,
            Mathf.Min(item.statRanges.maxStatCount + additionalStats + 1,
                     item.statRanges.possibleStats.Count)
        );

        Debug.Log($"Generating {statCount} stats for item {item.id}");

        // 가중치 기반 스탯 선택
        var availableStats = item.statRanges.possibleStats
            .Where(stat => stat.minRarity <= item.rarity)
            .ToList();

        for (int i = 0; i < statCount && availableStats.Any(); i++)
        {
            var selectedStat = SelectStatByWeight(availableStats);
            if (selectedStat != null)
            {
                float value = GenerateStatValue(selectedStat, item.rarity);
                item.AddStat(new StatContainer
                {
                    statType = selectedStat.statType,
                    amount = value,
                    sourceType = selectedStat.sourceType
                });

                Debug.Log($"Added stat: {selectedStat.statType} = {value}");
                availableStats.Remove(selectedStat);
            }
        }
    }

    private void GenerateEffects(ItemData item)
    {
        if (item.effectRanges == null || item.effectRanges.possibleEffects == null)
        {
            Debug.LogWarning($"No effect ranges defined for item: {item.id}");
            return;
        }

        item.effects.Clear();

        // 레어리티에 따른 추가 이펙트 수 계산
        int additionalEffects = item.effectRanges.additionalEffectsByRarity.GetValueOrDefault(item.rarity, 0);
        int effectCount = random.Next(
            item.effectRanges.minEffectCount,
            Mathf.Min(item.effectRanges.maxEffectCount + additionalEffects + 1,
                     item.effectRanges.possibleEffects.Count)
        );

        Debug.Log($"Generating {effectCount} effects for item {item.id}");

        // 가중치 기반 이펙트 선택
        var availableEffects = item.effectRanges.possibleEffects
            .Where(effect => effect.minRarity <= item.rarity)
            .ToList();

        for (int i = 0; i < effectCount && availableEffects.Any(); i++)
        {
            var selectedEffect = SelectEffectByWeight(availableEffects);
            if (selectedEffect != null)
            {
                float value = GenerateEffectValue(selectedEffect, item.rarity);
                var effectData = new ItemEffectData
                {
                    effectId = selectedEffect.effectId,
                    effectName = selectedEffect.effectName,
                    effectType = selectedEffect.effectType,
                    value = value,
                    applicableTypes = selectedEffect.applicableTypes,
                    applicableSkills = selectedEffect.applicableSkills,
                    applicableElements = selectedEffect.applicableElements
                };

                item.AddEffect(effectData);
                Debug.Log($"Added effect: {effectData.effectName} with value {value}");
                availableEffects.Remove(selectedEffect);
            }
        }
    }

    private ItemStatRange SelectStatByWeight(List<ItemStatRange> stats)
    {
        float totalWeight = stats.Sum(s => s.weight);
        float randomValue = (float)(random.NextDouble() * totalWeight);

        float currentWeight = 0;
        foreach (var stat in stats)
        {
            currentWeight += stat.weight;
            if (randomValue <= currentWeight)
            {
                return stat;
            }
        }

        return stats.LastOrDefault();
    }

    private ItemEffectRange SelectEffectByWeight(List<ItemEffectRange> effects)
    {
        float totalWeight = effects.Sum(e => e.weight);
        float randomValue = (float)(random.NextDouble() * totalWeight);

        float currentWeight = 0;
        foreach (var effect in effects)
        {
            currentWeight += effect.weight;
            if (randomValue <= currentWeight)
            {
                return effect;
            }
        }

        return effects.LastOrDefault();
    }

    private float GenerateStatValue(ItemStatRange statRange, ItemRarity rarity)
    {
        float baseValue = (float)(random.NextDouble() * (statRange.maxValue - statRange.minValue) + statRange.minValue);

        // 레어리티에 따른 값 증가
        float rarityMultiplier = 1 + ((int)rarity * 0.2f);
        float finalValue = baseValue * rarityMultiplier;

        // 증가 타입에 따른 처리
        switch (statRange.increaseType)
        {
            case IncreaseType.Add:
                finalValue = Mathf.Round(finalValue);
                break;
            case IncreaseType.Mul:
                finalValue = Mathf.Round(finalValue * 100) / 100;
                break;
        }

        return finalValue;
    }

    private float GenerateEffectValue(ItemEffectRange effectRange, ItemRarity rarity)
    {
        float baseValue = (float)(random.NextDouble() * (effectRange.maxValue - effectRange.minValue) + effectRange.minValue);
        float rarityMultiplier = 1 + ((int)rarity * 0.2f);
        return baseValue * rarityMultiplier;
    }

    public List<ItemData> GenerateDrops(DropTableData dropTable, float luckMultiplier = 1f)
    {
        if (dropTable == null || dropTable.dropEntries == null)
        {
            Debug.LogWarning("Invalid drop table");
            return new List<ItemData>();
        }

        var drops = new List<ItemData>();
        int dropCount = 0;

        // 보장된 드롭 체크
        if (random.NextDouble() < dropTable.guaranteedDropRate)
        {
            var guaranteedDrop = GenerateGuaranteedDrop(dropTable);
            if (guaranteedDrop != null)
            {
                drops.Add(guaranteedDrop);
                dropCount++;
            }
        }

        // 일반 드롭 생성
        foreach (var entry in dropTable.dropEntries)
        {
            if (dropCount >= dropTable.maxDrops) break;

            float adjustedDropRate = entry.dropRate * luckMultiplier;
            if (random.NextDouble() < adjustedDropRate)
            {
                var item = GenerateItem(entry.itemId, entry.rarity);
                if (item != null)
                {
                    // 아이템 수량 결정
                    item.amount = random.Next(entry.minAmount, entry.maxAmount + 1);
                    drops.Add(item);
                    dropCount++;
                    Debug.Log($"Generated drop: {item.name} x{item.amount}");
                }
            }
        }

        return drops;
    }

    private ItemData GenerateGuaranteedDrop(DropTableData dropTable)
    {
        // 가중치 합계 계산
        float totalWeight = dropTable.dropEntries.Sum(entry => entry.dropRate);
        float randomValue = (float)(random.NextDouble() * totalWeight);

        // 가중치 기반 아이템 선택
        float currentWeight = 0;
        foreach (var entry in dropTable.dropEntries)
        {
            currentWeight += entry.dropRate;
            if (randomValue <= currentWeight)
            {
                var item = GenerateItem(entry.itemId, entry.rarity);
                if (item != null)
                {
                    item.amount = random.Next(entry.minAmount, entry.maxAmount + 1);
                    Debug.Log($"Generated guaranteed drop: {item.name} x{item.amount}");
                    return item;
                }
            }
        }

        return null;
    }
}