using System.Collections;
using UnityEngine;

public abstract class BossBase : MonoBehaviour
{
    [Header("Boss References")]
    [SerializeField] protected Health health;
    [SerializeField] protected Transform player;

    [Header("Boss Settings")]
    [SerializeField] protected string bossName = "Boss";
    [SerializeField] protected float introDelay = 1f;
    [SerializeField] protected float attackCooldown = 2f;

    [Header("Phase Settings")]
    [SerializeField] protected float phaseTwoHealthPercent = 0.5f;

    [Header("Rewards")]
    [SerializeField] protected GameObject coinPrefab;
    [SerializeField] protected int coinsDropped = 25;
    [SerializeField] protected float coinScatterRadius = 2f;

    protected bool fightStarted;
    protected bool isDead;
    protected bool inPhaseTwo;
    protected bool isAttacking;

    protected virtual void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    protected virtual void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath += HandleDeath;
            health.OnHealthChanged += HandleHealthChanged;
        }
    }

    protected virtual void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
            health.OnHealthChanged -= HandleHealthChanged;
        }
    }

    public virtual void StartBossFight()
    {
        if (fightStarted)
            return;

        fightStarted = true;
        StartCoroutine(BossFightRoutine());
    }

    protected virtual IEnumerator BossFightRoutine()
    {
        yield return new WaitForSeconds(introDelay);

        while (!isDead)
        {
            if (player != null && !isAttacking)
            {
                yield return StartCoroutine(PerformAttack());
                yield return new WaitForSeconds(attackCooldown);
            }

            yield return null;
        }
    }

    protected abstract IEnumerator PerformAttack();

    protected virtual void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (inPhaseTwo)
            return;

        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= phaseTwoHealthPercent)
        {
            inPhaseTwo = true;
            EnterPhaseTwo();
        }
    }

    protected virtual void EnterPhaseTwo()
    {
        attackCooldown *= 0.7f;
        Debug.Log(bossName + " entered phase two!");
    }

    protected virtual void HandleDeath(Health deadHealth)
    {
        if (isDead)
            return;

        isDead = true;
        DropRewards();

        Debug.Log(bossName + " defeated!");
    }

    protected virtual void DropRewards()
    {
        if (coinPrefab == null)
            return;

        for (int i = 0; i < coinsDropped; i++)
        {
            Vector2 offset = Random.insideUnitCircle * coinScatterRadius;

            Instantiate(
                coinPrefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );
        }
    }

    protected Vector2 DirectionToPlayer()
    {
        if (player == null)
            return Vector2.right;

        return ((Vector2)player.position - (Vector2)transform.position).normalized;
    }
}