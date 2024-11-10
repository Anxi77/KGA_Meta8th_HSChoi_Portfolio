public interface ISkillModifier
{
    // 스킬 스탯 수정
    float ModifySkillDamage(float baseDamage, SkillType skillType, ElementType elementType);
    float ModifySkillCooldown(float baseCooldown, SkillType skillType);

    // 투사체 스킬 수정
    float ModifyProjectileSpeed(float baseSpeed);
    float ModifyProjectileRange(float baseRange);
    bool IsHomingEnabled(bool baseHoming);

    // 범위 스킬 수정
    float ModifyAreaRadius(float baseRadius);
    float ModifyAreaDuration(float baseDuration);

    // 이벤트 핸들러
    void OnSkillCast(Skill skill);
    void OnSkillHit(Skill skill, Enemy target);
}