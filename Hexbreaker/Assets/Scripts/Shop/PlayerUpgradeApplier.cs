using UnityEngine;

public class PlayerUpgradeApplier : MonoBehaviour
{
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.MaxHealth:
                Debug.Log("Apply max health upgrade: " + upgrade.value);
                break;

            case UpgradeType.MoveSpeed:
                Debug.Log("Apply move speed upgrade: " + upgrade.value);
                break;

            case UpgradeType.DashCooldown:
                Debug.Log("Apply dash cooldown upgrade: " + upgrade.value);
                break;

            case UpgradeType.SpellDamage:
                Debug.Log("Apply spell damage upgrade: " + upgrade.value);
                break;

            case UpgradeType.FireRate:
                Debug.Log("Apply fire rate upgrade: " + upgrade.value);
                break;
        }
    }
}