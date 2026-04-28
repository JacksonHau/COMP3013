using UnityEngine;

public class BounceProjectile : MonoBehaviour
{
    protected float speed;
    protected float damage;
    protected Vector2 direction;
    protected bool canDamageCaster;
    protected GameObject caster;

    int bounces = 3;
    bool damagePlayer;

    public void Init(Vector2 dir, float spd, float dmg, bool canHitCaster, GameObject spellCaster)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        canDamageCaster = canHitCaster;
        caster = spellCaster;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public void SetExtraBounces(int extraBounces)
    {
        bounces += extraBounces;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canDamageCaster && caster != null)
        {
            if (collision.gameObject == caster || collision.transform.root.gameObject == caster.transform.root.gameObject)
                return;
        }

        IDamageable dmg = collision.collider.GetComponentInParent<IDamageable>();

        if (dmg != null)
        {
            if (dmg is Health health)
            {
                health.TakeDamage(damage, direction);
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


        direction = Vector2.Reflect(direction, collision.contacts[0].normal);

        bounces--;

        if (bounces <= 0)
            Destroy(gameObject);
    }
}