using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected float speed;
    protected float damage;
    protected Vector2 direction;
    protected bool canDamageCaster;
    protected GameObject caster;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamageCaster)
        {
            if (other.gameObject == caster || other.transform.root.gameObject == caster.transform.root.gameObject)
                return;
        }

        IDamageable dmg = other.GetComponentInParent<IDamageable>();

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

        

        Destroy(gameObject);
    }
}