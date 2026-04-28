using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    protected float speed;
    protected float damage;
    protected Vector2 direction;
    protected bool canDamageCaster;
    protected GameObject caster;

    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private GameObject explosionEffect;

    public void Init(
        Vector2 dir,
        float spd,
        float dmg,
        bool canHitCaster,
        GameObject spellCaster,
        GameObject explosionPrefab)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        canDamageCaster = canHitCaster;
        caster = spellCaster;
        explosionEffect = explosionPrefab;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canDamageCaster)
        {
            if (collision.gameObject == caster ||
                collision.transform.root.gameObject == caster.transform.root.gameObject)
            {
                return;
            }
        }

        Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamageCaster)
        {
            if (other.gameObject == caster ||
                other.transform.root.gameObject == caster.transform.root.gameObject)
            {
                return;
            }
        }

        Explode();
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(
                explosionEffect,
                transform.position,
                Quaternion.identity
            );

            Destroy(effect, 1f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (var hit in hits)
        {
            if (!canDamageCaster)
            {
                if (hit.gameObject == caster ||
                    hit.transform.root.gameObject == caster.transform.root.gameObject)
                {
                    continue;
                }
            }

            IDamageable dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

                if (dmg is Health health)
                {
                    health.TakeDamage(damage, knockbackDir);
                }
                else
                {
                    dmg.TakeDamage(damage);
                }

                SpellCaster Spellcaster = caster.GetComponent<SpellCaster>();
                if (Spellcaster != null)
                {
                    Spellcaster.AddUltimateEnergy(damage);
                }
            }


        }

        Destroy(gameObject);
    }
}