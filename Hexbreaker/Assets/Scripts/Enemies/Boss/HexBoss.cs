using System.Collections;
using UnityEngine;

public class HexBoss : BossBase
{
    [Header("Projectile Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private int projectileCount = 5;
    [SerializeField] private float spreadAngle = 60f;

    [Header("Nova Attack")]
    [SerializeField] private float novaRadius = 3f;
    [SerializeField] private float novaDamage = 15f;
    [SerializeField] private GameObject novaEffect;

    private int attackIndex;

    protected override IEnumerator PerformAttack()
    {
        isAttacking = true;

        if (attackIndex % 2 == 0)
            yield return StartCoroutine(ProjectileSpreadAttack());
        else
            yield return StartCoroutine(NovaAttack());

        attackIndex++;
        isAttacking = false;
    }

    private IEnumerator ProjectileSpreadAttack()
    {
        yield return new WaitForSeconds(0.4f);

        int count = inPhaseTwo ? projectileCount + 2 : projectileCount;
        float angleStep = count > 1 ? spreadAngle / (count - 1) : 0f;
        float startAngle = -spreadAngle / 2f;

        Vector2 baseDirection = DirectionToPlayer();

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * baseDirection;

            SpawnProjectile(direction);
        }

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator NovaAttack()
    {
        yield return new WaitForSeconds(0.7f);

        if (novaEffect != null)
            Instantiate(novaEffect, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, novaRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            Health playerHealth = hit.GetComponentInParent<Health>();

            if (playerHealth != null)
            {
                Vector2 knockbackDir =
                    ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

                playerHealth.TakeDamage(novaDamage, knockbackDir);
            }
        }

        yield return new WaitForSeconds(0.3f);
    }

    private void SpawnProjectile(Vector2 direction)
    {
        if (projectilePrefab == null)
            return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject obj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Projectile projectile = obj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Init(
                direction,
                projectileSpeed,
                projectileDamage,
                false,
                gameObject
            );
        }
    }
}