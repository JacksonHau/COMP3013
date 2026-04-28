using UnityEngine;

public enum SpellType
{
    Projectile,
    Laser,
    ExplosiveProjectile,
    Nova,
    BounceProjectile
}

public enum SpellRangeType
{
    Melee,
    Ranged
}

public struct RuntimeSpellStats
{
    public float damage;
    public float speed;
    public float range;
    public float cooldown;
    public float spawnOffset;
    public int extraProjectiles;
    public int extraBounces;
    public int extraPulses;
    public bool canDamageCaster;
}

[CreateAssetMenu(menuName = "Spells/Spell")]
public class SpellData : ScriptableObject
{
    public string spellName;

    [Header("UI")]
    public Sprite icon;

    [Header("Type")]
    public SpellType spellType;

    [Header("Range")]
    public SpellRangeType rangeType = SpellRangeType.Ranged;

    [Header("Stats")]
    public float damage = 1f;
    public float cooldown = 0.2f;

    [Header("Damage Rules")]
    public bool canDamageCaster = false;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float speed = 10f;

    [Header("Spawn Settings")]
    public float spawnOffset = 0.6f;

    [Header("Laser Telegraph")]
    public bool telegraphLaser = true;
    public float telegraphTime = 0.6f;
    public float telegraphWidth = 0.05f;
    public Color telegraphColor = Color.yellow;

    [Header("Laser / Area Settings")]
    public float range = 10f;
    public float laserWidth = 0.12f;
    public float laserDuration = 0.05f;
    public Color laserColor = Color.red;

    [Header("Visual Effects")]
    public GameObject castEffect;

    [Header("Ultimate")]
    public float ultimateDamageMultiplier = 2f;
    public float ultimateSpeedMultiplier;
    public float ultimateRangeMultiplier = 1.5f;
    public float ultimateCooldownMultiplier = 1f;
    public float ultimateSizeMultiplier = 1.5f;
    public int ultimateExtraProjectiles = 0;
    public int ultimateExtraBounces = 0;
    public int ultimateExtraPulses = 0;

    private RuntimeSpellStats GetSpellStats(SpellData spell, bool isUltimate)
    {
        RuntimeSpellStats stats = new RuntimeSpellStats
        {
            damage = spell.damage,
            speed = spell.speed,
            range = spell.range,
            cooldown = spell.cooldown,
            spawnOffset = spell.spawnOffset,
            extraProjectiles = 0,
            extraBounces = 0,
            extraPulses = 0,
            canDamageCaster = spell.canDamageCaster
        };

        if (isUltimate)
        {
            stats.damage *= spell.ultimateDamageMultiplier;
            stats.speed *= spell.ultimateSpeedMultiplier;
            stats.range *= spell.ultimateRangeMultiplier;
            stats.cooldown *= spell.ultimateCooldownMultiplier;
            stats.spawnOffset *= spell.ultimateSizeMultiplier;
            stats.extraProjectiles += spell.ultimateExtraProjectiles;
            stats.extraBounces += spell.ultimateExtraBounces;
            stats.extraPulses += spell.ultimateExtraPulses;
        }

        return stats;
    }
}

