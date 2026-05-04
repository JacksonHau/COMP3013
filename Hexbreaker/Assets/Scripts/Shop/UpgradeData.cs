using UnityEngine;

public enum UpgradeTarget
{
    Global,
    SpellSlot1,
    SpellSlot2,
    SpellSlot3,
    Dash,
    Ultimate
}

public enum UpgradeEffectType
{
    DamageMultiplier,
    ExtraProjectiles,
    CooldownReduction,
    OnHitEffect,
    Special
}

[CreateAssetMenu(menuName = "Shop/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public Sprite icon;
    public int cost;

    [TextArea]
    public string description;

    [Header("Target")]
    public UpgradeTarget target;

    [Header("Effect")]
    public UpgradeEffectType effectType;
    public float value;

    [Header("Special")]
    public string specialID; 
}