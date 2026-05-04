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
    public float ultimateSpeedMultiplier = 1f;
    public float ultimateRangeMultiplier = 1.5f;
    public float ultimateCooldownMultiplier = 1f;
    public float ultimateSizeMultiplier = 1.5f;
    public int ultimateExtraProjectiles = 0;
    public int ultimateExtraBounces = 0;
    public int ultimateExtraPulses = 0;
}