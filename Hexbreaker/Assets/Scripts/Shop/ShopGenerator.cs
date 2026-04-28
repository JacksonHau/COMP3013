using System.Collections.Generic;
using UnityEngine;

public class ShopGenerator : MonoBehaviour
{
    [Header("Shop Setup")]
    [SerializeField] private ShopItem shopItemPrefab;
    [SerializeField] private Transform[] itemSpawnPoints;

    [Header("Possible Upgrades")]
    [SerializeField] private UpgradeData[] possibleUpgrades;

    [SerializeField] private int itemsToSpawn = 3;

    private void Start()
    {
        SpawnShopItems();
    }

    private void SpawnShopItems()
    {
        if (shopItemPrefab == null || itemSpawnPoints.Length == 0 || possibleUpgrades.Length == 0)
            return;

        List<UpgradeData> upgradePool = new List<UpgradeData>(possibleUpgrades);

        int spawnCount = Mathf.Min(itemsToSpawn, itemSpawnPoints.Length, upgradePool.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = Random.Range(0, upgradePool.Count);
            UpgradeData chosenUpgrade = upgradePool[randomIndex];
            upgradePool.RemoveAt(randomIndex);

            ShopItem item = Instantiate(
                shopItemPrefab,
                itemSpawnPoints[i].position,
                Quaternion.identity,
                transform
            );

            item.Setup(chosenUpgrade);
        }
    }
}