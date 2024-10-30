using UnityEngine;

public static class ElementalEffects
{
    public static void ApplyElementalEffect(ElementType element, float elementalPower, GameObject target)
    {
        switch (element)
        {
            case ElementType.Dark:
                ApplyDarkEffect(elementalPower, target);
                break;
            case ElementType.Water:
                ApplyWaterEffect(elementalPower, target);
                break;
            case ElementType.Fire:
                ApplyFireEffect(elementalPower, target);
                break;
            case ElementType.Earth:
                ApplyEarthEffect(elementalPower, target);
                break;
        }
    }

    private static void ApplyDarkEffect(float power, GameObject target)
    {
        // 어둠 속성 효과: 대상의 방어력 감소
        if (target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.ApplyDefenseDebuff(power, 5f); // 5초간 방어력 감소
        }
    }

    private static void ApplyWaterEffect(float power, GameObject target)
    {
        // 물 속성 효과: 대상의 이동속도 감소
        if (target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.ApplySlowEffect(power, 3f); // 3초간 이동속도 감소
        }
    }

    private static void ApplyFireEffect(float power, GameObject target)
    {
        // 불 속성 효과: 지속 데미지
        if (target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.ApplyDotDamage(power, 0.5f, 3f); // 3초간 0.5초마다 데미지
        }
    }

    private static void ApplyEarthEffect(float power, GameObject target)
    {
        // 대지 속성 효과: 스턴
        if (target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.ApplyStun(power, 2f); // 2초간 스턴
        }
    }
}