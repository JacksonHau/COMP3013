using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Drops")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject spellPickupPrefab;

    [Header("Upgrade Pool")]
    [SerializeField] private UpgradeData[] possibleUpgrades;

    [Header("Spell Pool")]
    [SerializeField] private SpellData[] possibleSpells;

    [Header("Coin Settings")]
    [SerializeField] private int minCoins = 5;
    [SerializeField] private int maxCoins = 10;
    [SerializeField] private float scatterRadius = 1.2f;

    [Header("Drop Chances")]
    [Range(0f, 1f)]
    [SerializeField] private float upgradeDropChance = 0.5f;

    private bool opened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened)
            return;

        if (!other.CompareTag("Player"))
            return;

        OpenChest();
    }

    private void OpenChest()
    {
        opened = true;

        DropCoins();
        DropReward();

        // TODO: play animation / sound here

        Destroy(gameObject);
    }

    private void DropCoins()
    {
        if (coinPrefab == null)
            return;

        int amount = Random.Range(minCoins, maxCoins + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * scatterRadius;

            Instantiate(
                coinPrefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );
        }
    }

    private void DropReward()
    {
        float roll = Random.value;

        if (roll <= upgradeDropChance)
        {
            DropUpgrade();
        }
        else
        {
            DropSpell();
        }
    }

    private void DropUpgrade()
    {
        if (possibleUpgrades.Length == 0)
            return;

        UpgradeData upgrade = possibleUpgrades[
            Random.Range(0, possibleUpgrades.Length)
        ];

        SpawnUpgradePickup(upgrade);
    }

    private void DropSpell()
    {
        if (possibleSpells.Length == 0 || spellPickupPrefab == null)
            return;

        SpellData spell = possibleSpells[
            Random.Range(0, possibleSpells.Length)
        ];

        GameObject pickup = Instantiate(
            spellPickupPrefab,
            transform.position,
            Quaternion.identity
        );

        SpellPickup sp = pickup.GetComponent<SpellPickup>();

        if (sp != null)
        {
            sp.spell = spell;
            sp.RefreshDisplay();
        }
    }

    private void SpawnUpgradePickup(UpgradeData upgrade)
    {
        GameObject obj = new GameObject("UpgradePickup");

        obj.transform.position = transform.position;

        UpgradePickup pickup = obj.AddComponent<UpgradePickup>();
        pickup.SetUpgrade(upgrade);
    }
}