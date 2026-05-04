using Unity.VisualScripting;
using UnityEngine;

public class UpgradePickup : MonoBehaviour
{
    private UpgradeData upgrade;

    [SerializeField] private GameObject floatingTextPrefab;

    public void SetUpgrade(UpgradeData data)
    {
        upgrade = data;
        RefreshVisual();
    }

    private void Awake()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();

        if (col == null)
            col = gameObject.AddComponent<CircleCollider2D>();

        col.isTrigger = true;
        col.radius = 0.5f;
    }

    private void RefreshVisual()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();

        if (upgrade != null && upgrade.icon != null)
            sr.sprite = upgrade.icon;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (PlayerUpgradeManager.Instance != null && upgrade != null)
        {
            PlayerUpgradeManager.Instance.AddUpgrade(upgrade);

            SpawnFloatingText();
        }

        Destroy(gameObject);
    }

    private void SpawnFloatingText()
    {
        if (floatingTextPrefab == null || upgrade == null)
            return;

        GameObject obj = Instantiate(
            floatingTextPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity
        );

        FloatingText ft = obj.GetComponent<FloatingText>();

        if (ft != null)
        {
            string message = upgrade.upgradeName;

            if (!string.IsNullOrEmpty(upgrade.description))
            {
                message += "\n" + upgrade.description;
            }

            ft.Setup(message, Color.yellow, 1f);
        }
    }
}