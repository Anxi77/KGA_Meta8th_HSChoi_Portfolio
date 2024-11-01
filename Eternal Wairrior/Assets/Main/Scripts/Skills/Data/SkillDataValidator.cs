using System.Collections.Generic;

public static class SkillDataValidator
{
    public static ValidationResult ValidateSkillData(SkillData skillData)
    {
        var result = new ValidationResult();

        if (skillData == null)
        {
            result.AddError("Skill data is null");
            return result;
        }

        ValidateMetadata(skillData.metadata, result);
        ValidateStats(skillData, result);
        ValidateReferences(skillData, result);

        return result;
    }

    private static void ValidateMetadata(SkillMetadata metadata, ValidationResult result)
    {
        if (string.IsNullOrEmpty(metadata.Name))
            result.AddError("Skill name is empty");

        if (metadata.Type == SkillType.None)
            result.AddWarning("Skill type is not set");

        if (metadata.ID == SkillID.None)
            result.AddError("Skill ID is not set");

        if (metadata.Tier <= 0)
            result.AddWarning("Skill tier should be greater than 0");
    }

    private static void ValidateStats(SkillData skillData, ValidationResult result)
    {
        var stats = skillData.GetCurrentTypeStat();
        if (stats == null)
        {
            result.AddError("Skill stats are not initialized");
            return;
        }

        if (stats is ProjectileSkillStat projectileStats)
        {
            ValidateProjectileStats(projectileStats, result);
        }
        else if (stats is AreaSkillStat areaStats)
        {
            ValidateAreaStats(areaStats, result);
        }
    }

    private static void ValidateProjectileStats(ProjectileSkillStat stats, ValidationResult result)
    {
        if (stats.projectileSpeed <= 0)
            result.AddWarning("Projectile speed should be greater than 0");

        if (stats.shotInterval <= 0)
            result.AddWarning("Shot interval should be greater than 0");

        if (stats.persistenceData != null)
        {
            if (stats.persistenceData.isPersistent && stats.persistenceData.duration <= 0)
                result.AddWarning("Persistent projectile should have duration greater than 0");
        }
    }

    private static void ValidateAreaStats(AreaSkillStat stats, ValidationResult result)
    {
        if (stats.radius <= 0)
            result.AddWarning("Area radius should be greater than 0");

        if (stats.tickRate <= 0)
            result.AddWarning("Tick rate should be greater than 0");
    }

    private static void ValidateReferences(SkillData skillData, ValidationResult result)
    {
        if (skillData.metadata.Type == SkillType.Projectile)
        {
            if (skillData.projectile == null)
                result.AddError("Projectile prefab is not assigned");
        }
    }
}

public class ValidationResult
{
    public List<string> Errors { get; } = new List<string>();
    public List<string> Warnings { get; } = new List<string>();

    public void AddError(string message) => Errors.Add(message);
    public void AddWarning(string message) => Warnings.Add(message);

    public bool HasErrors => Errors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
}