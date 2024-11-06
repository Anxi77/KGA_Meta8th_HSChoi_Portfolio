/// <summary>
/// Defines the basic type of a skill
/// </summary>
public enum SkillType
{
    None = 0,
    Projectile,  // Skills that fire projectiles (e.g., SingleShot, MultiShot)
    Area,        // Skills that affect an area (e.g., Bind, Orbit)
    Passive      // Skills that provide passive effects
}

/// <summary>
/// Unique identifier for each skill
/// </summary>
public enum SkillID
{
    None = 100000,
    //Earth
    Vine,        // Area
    EarthRift,   // Projectile
    GaiasGrace,  // Passive
    //Water
    FrostTide,   // Area
    FrostHunt,   // Projectile
    TidalEssence,// Passive
    //Dark
    ShadowWaltz, // Area
    EventHorizon, // Projectile
    AbyssalExpansion, // Passive
    //Fire
    Flame, // Projectile
    FireRing, // Area
    ThermalElevation // Passive
}

/// <summary>
/// Defines the elemental type of a skill
/// </summary>
public enum ElementType
{
    None = 0,
    Dark,    // Reduces target's defense
    Water,   // Slows target's movement
    Fire,    // Deals damage over time
    Earth    // Can stun targets
}