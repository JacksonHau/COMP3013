using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("Coins")]
    public int currentCoins = 0;
    public int CurrentCoins => currentCoins;

    [Header("UI")]
    public TMP_Text coinText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateUI();
    }

    public void RemoveCoins(int amount)
    {
        currentCoins -= amount;
        currentCoins = Mathf.Max(0, currentCoins);
        UpdateUI();
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins < amount)
            return false;

        currentCoins -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = currentCoins.ToString();
    }
}