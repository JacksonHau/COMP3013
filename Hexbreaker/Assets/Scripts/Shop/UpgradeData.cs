using UnityEngine;

public enum UpgradeType
{
    MaxHealth,
    MoveSpeed,
    DashCooldown,
    SpellDamage,
    FireRate
}

[CreateAssetMenu(menuName = "Shop/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public Sprite icon;
    public int cost = 10;
    public UpgradeType upgradeType;
    public float value = 1f;
}