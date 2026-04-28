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

    private UpgradeData upgrade;
    private bool playerInRange;
    private bool purchased;

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteractPerformed;
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

        PlayerUpgradeApplier applier = FindFirstObjectByType<PlayerUpgradeApplier>();

        if (applier != null)
            applier.ApplyUpgrade(upgrade);

        purchased = true;
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}