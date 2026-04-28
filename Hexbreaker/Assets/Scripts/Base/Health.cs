using System;
using UnityEngine;
using Random = UnityEngine.Random;

public interface IDamageable
{
    void TakeDamage(float amount);
}

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;
    private bool isDead = false;
    private bool isInvulnerable = false;

    private HitReaction hitReaction;
    private DeathEffect deathEffect;

    [Header("Drops On Death")]
    [SerializeField] private bool dropCoinsOnDeath = false;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoinsDropped = 1;
    [SerializeField] private int maxCoinsDropped = 3;
    [SerializeField] private float coinScatterRadius = 0.5f;
    [SerializeField] private bool dropPlayerCurrentCoins = false;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float NormalizedHealth => maxHealth <= 0f ? 0f : currentHealth / maxHealth;

    public event Action<Health> OnDeath;
    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
        hitReaction = GetComponent<HitReaction>();
        deathEffect = GetComponent<DeathEffect>();
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || isInvulnerable)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        //Debug.Log(name + " took " + amount + " damage");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (isDead || isInvulnerable)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (hitReaction != null)
        {
            hitReaction.PlayReaction(hitDirection);
        }

        //Debug.Log(name + " took " + amount + " damage");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void DropCoins()
    {
        if (!dropCoinsOnDeath || coinPrefab == null)
            return;

        int coinAmount;

        if (dropPlayerCurrentCoins && CoinManager.Instance != null)
        {
            coinAmount = CoinManager.Instance.CurrentCoins;
            CoinManager.Instance.RemoveCoins(coinAmount);
        }
        else
        {
            coinAmount = Random.Range(minCoinsDropped, maxCoinsDropped + 1);
        }

        for (int i = 0; i < coinAmount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * coinScatterRadius;

            Instantiate(
                coinPrefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        DropCoins();

        OnDeath?.Invoke(this);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (deathEffect != null)
        {
            deathEffect.PlayDeath();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}