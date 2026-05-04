using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;

    [Header("Hover Info")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Vector3 floatingTextOffset = new Vector3(0f, 1f, 0f);

    private UpgradeData upgrade;
    private bool playerInRange;
    private bool purchased;

    private GameObject activeHoverText;

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteractPerformed;

        HideHoverText();
    }

    public void Setup(UpgradeData upgradeData)
    {
        upgrade = upgradeData;

        if (iconRenderer != null)
            iconRenderer.sprite = upgrade.icon;

        if (nameText != null)
            nameText.text = upgrade.upgradeName;

        if (costText != null)
            costText.text = upgrade.cost.ToString();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!playerInRange || purchased || upgrade == null)
            return;

        TryPurchase();
    }

    private void TryPurchase()
    {
        if (CoinManager.Instance == null)
            return;

        if (!CoinManager.Instance.SpendCoins(upgrade.cost))
        {
            Debug.Log("Not enough coins!");
            return;
        }

        PlayerUpgradeManager applier = FindFirstObjectByType<PlayerUpgradeManager>();

        if (applier != null)
            applier.AddUpgrade(upgrade);

        purchased = true;

        HideHoverText();
        Destroy(gameObject);
    }

    private void ShowHoverText()
    {
        if (floatingTextPrefab == null || upgrade == null || activeHoverText != null)
            return;

        activeHoverText = Instantiate(
            floatingTextPrefab,
            transform.position + floatingTextOffset,
            Quaternion.identity,
            transform
        );

        FloatingText floatingText = activeHoverText.GetComponent<FloatingText>();

        if (floatingText != null)
        {
            string message = upgrade.upgradeName;

            if (!string.IsNullOrEmpty(upgrade.description))
                message += "\n" + upgrade.description;

            message += "\nCost: " + upgrade.cost;

            floatingText.Setup(message, Color.yellow, 3f);
        }
    }

    private void HideHoverText()
    {
        if (activeHoverText != null)
        {
            Destroy(activeHoverText);
            activeHoverText = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        ShowHoverText();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        HideHoverText();
    }
}