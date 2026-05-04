using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeManager : MonoBehaviour
{
    public static PlayerUpgradeManager Instance { get; private set; }

    private readonly List<UpgradeData> activeUpgrades = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
            return;

        activeUpgrades.Add(upgrade);
        Debug.Log("Upgrade added: " + upgrade.upgradeName);
    }

    public List<UpgradeData> GetUpgradesForTarget(UpgradeTarget target)
    {
        List<UpgradeData> upgrades = new();

        foreach (UpgradeData upgrade in activeUpgrades)
        {
            if (upgrade.target == target || upgrade.target == UpgradeTarget.Global)
            {
                upgrades.Add(upgrade);
            }
        }

        return upgrades;
    }
}