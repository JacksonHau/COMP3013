using System.Collections;
using UnityEngine;

public class EnemySpellCaster : EnemyBase
{
    [Header("Spell Setup")]
    [SerializeField] private SpellData[] possibleSpells;
    [SerializeField] private SpellData assignedSpell;
    [SerializeField] private Transform castPoint;
    [SerializeField] private LineRenderer laserRenderer;

    [Header("Casting")]
    [SerializeField] private float castRange = 8f;
    [SerializeField] private float castInterval = 2f;
    private bool isCasting = false;

    [Header("Pickup Drop")]
    [SerializeField] private GameObject spellPickupPrefab;

    private float lastCastTime;
    private Health health;

    protected override void Awake()
    {
        base.Awake();

        health = GetComponent<Health>();

        if (laserRenderer == null)
        {
            laserRenderer = gameObject.AddComponent<LineRenderer>();
        }

        laserRenderer.positionCount = 2;
        laserRenderer.startWidth = 0.1f;
        laserRenderer.endWidth = 0.1f;
        laserRenderer.enabled = false;
    }

    protected override void Start()
    {
        base.Start();

        if (assignedSpell == null && possibleSpells != null && possibleSpells.Length > 0)
        {
            assignedSpell = possibleSpells[Random.Range(0, possibleSpells.Length)];
        }

        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!CanSeePlayer())
            return;

        if (assignedSpell == null || player == null)
            return;

        if (isCasting)
            return;

        Vector2 direction = DirectionToPlayer();

        if (!isCasting)
        {
            RotateTowards(direction);
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (assignedSpell.rangeType == SpellRangeType.Melee)
        {
            if (distance > assignedSpell.range)
            {
                MoveTowardsPlayer(direction);
                return;
            }
        }
        else
        {
            if (distance > castRange)
                return;
        }

        if (Time.time < lastCastTime + assignedSpell.cooldown + castInterval)
            return;

        TryCast(direction);
    }

    private void TryCast(Vector2 direction)
    {
        if (castPoint == null || assignedSpell == null)
            return;

        if (isCasting)
            return;

        StartCoroutine(CastRoutine(direction));
    }

    private IEnumerator CastRoutine(Vector2 direction)
    {
        isCasting = true;
        lastCastTime = Time.time;

        CastSpell(assignedSpell, direction);

        float castTime = assignedSpell.cooldown;

        if (assignedSpell.spellType == SpellType.Laser)
        {
            castTime = assignedSpell.telegraphTime + assignedSpell.laserDuration;
        }

        yield return new WaitForSeconds(castTime);

        isCasting = false;
    }

    [SerializeField] private float moveSpeed = 2f;
    private void MoveTowardsPlayer(Vector2 direction)
    {
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    private void CastSpell(SpellData spell, Vector2 direction)
    {
        if (spell.spellType == SpellType.Projectile)
        {
            SpawnProjectile(spell, direction);
        }
        else if (spell.spellType == SpellType.Laser)
        {
            FireLaser(spell, direction);
        }
        else if (spell.spellType == SpellType.ExplosiveProjectile)
        {
            SpawnExplosiveProjectile(spell, direction);
        }
        else if (spell.spellType == SpellType.Nova)
        {
            CastNova(spell);
        }
        else if (spell.spellType == SpellType.BounceProjectile)
        {
            SpawnBounceProjectile(spell, direction);
        }
    }

    private bool ShouldDamageTarget(SpellData spell, Collider2D target)
    {
        if (spell.canDamageCaster)
            return true;

        return target.gameObject != gameObject &&
               target.transform.root.gameObject != transform.root.gameObject;
    }

    private Vector2 GetSpawnPosition(SpellData spell, Vector2 direction)
    {
        return (Vector2)castPoint.position + direction * spell.spawnOffset;
    }

    private void SpawnProjectile(SpellData spell, Vector2 direction)
    {
        Vector2 spawnPos = GetSpawnPosition(spell, direction);

        GameObject proj = Instantiate(
            spell.projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(direction, spell.speed, spell.damage, spell.canDamageCaster, gameObject);
        }
    }

    private bool laserActive = false;
    private void FireLaser(SpellData spell, Vector2 direction)
    {
        if (!laserActive)
        {
            StartCoroutine(EnemyLaserRoutine(spell));
        }
    }

    private IEnumerator EnemyLaserRoutine(SpellData spell)
    {
        if (player == null)
            yield break;

        laserActive = true;

        Vector2 lockedDirection = DirectionToPlayer();
        Vector2 lockedStart = GetSpawnPosition(spell, lockedDirection);
        Vector2 lockedEnd = lockedStart + lockedDirection * spell.range;

        if (spell.telegraphLaser)
        {
            laserRenderer.enabled = true;
            laserRenderer.startColor = spell.telegraphColor;
            laserRenderer.endColor = spell.telegraphColor;
            laserRenderer.startWidth = spell.telegraphWidth;
            laserRenderer.endWidth = spell.telegraphWidth;

            float timer = 0f;

            while (timer < spell.telegraphTime)
            {
                if (player == null)
                    break;

                Vector2 direction = DirectionToPlayer();
                RotateTowards(direction);

                Vector2 startPoint = GetSpawnPosition(spell, direction);

                RaycastHit2D[] hits = Physics2D.RaycastAll(
                    startPoint,
                    direction,
                    spell.range
                );

                Vector2 endPoint = startPoint + direction * spell.range;

                for (int i = 0; i < hits.Length; i++)
                {
                    if (ShouldIgnoreLaserHit(spell, hits[i].collider))
                        continue;

                    endPoint = hits[i].point;
                    break;
                }

                laserRenderer.SetPosition(0, startPoint);
                laserRenderer.SetPosition(1, endPoint);

                lockedDirection = direction;
                lockedStart = startPoint;
                lockedEnd = endPoint;

                timer += Time.deltaTime;
                yield return null;
            }
        }

        laserRenderer.enabled = true;
        laserRenderer.startColor = spell.laserColor;
        laserRenderer.endColor = spell.laserColor;
        laserRenderer.startWidth = spell.laserWidth;
        laserRenderer.endWidth = spell.laserWidth;
        laserRenderer.SetPosition(0, lockedStart);
        laserRenderer.SetPosition(1, lockedEnd);

        RaycastHit2D[] damageHits = Physics2D.RaycastAll(
            lockedStart,
            lockedDirection,
            spell.range
        );

        for (int i = 0; i < damageHits.Length; i++)
        {
            if (ShouldIgnoreLaserHit(spell, damageHits[i].collider))
                continue;

            IDamageable dmg = damageHits[i].collider.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(spell.damage);
            }

            break;
        }

        yield return new WaitForSeconds(spell.laserDuration);

        laserRenderer.enabled = false;
        laserActive = false;
    }

    private bool IsCasterCollider(Collider2D col)
    {
        return col.gameObject == gameObject ||
               col.transform.root.gameObject == transform.root.gameObject;
    }

    private bool ShouldIgnoreLaserHit(SpellData spell, Collider2D col)
    {
        if (col == null)
            return true;

        if (col.gameObject.layer == LayerMask.NameToLayer("RoomTrigger"))
            return true;

        if (!spell.canDamageCaster && IsCasterCollider(col))
            return true;

        return false;
    }


    private void SpawnExplosiveProjectile(SpellData spell, Vector2 direction)
    {
        Vector2 spawnPos = GetSpawnPosition(spell, direction);

        GameObject proj = Instantiate(
            spell.projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        ExplosiveProjectile explosive = proj.GetComponent<ExplosiveProjectile>();
        if (explosive != null)
        {
            explosive.Init(direction, spell.speed, spell.damage, spell.canDamageCaster, gameObject, spell.castEffect);
        }
    }

    void CastNova(SpellData spell)
    {
        if (spell.castEffect != null)
        {
            GameObject effect = Instantiate(
                spell.castEffect,
                castPoint.position,
                Quaternion.identity
            );

            Destroy(effect, 1f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            castPoint.position,
            spell.range
        );

        foreach (var hit in hits)
        {
            if (!ShouldDamageTarget(spell, hit))
                continue;

            IDamageable dmg = hit.GetComponent<IDamageable>();

            if (dmg != null)
            {
                dmg.TakeDamage(spell.damage);
            }
        }
    }

    private void SpawnBounceProjectile(SpellData spell, Vector2 direction)
    {
        Vector2 spawnPos = GetSpawnPosition(spell, direction);

        GameObject proj = Instantiate(
            spell.projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        BounceProjectile bounce = proj.GetComponent<BounceProjectile>();
        if (bounce != null)
        {
            bounce.Init(direction, spell.speed, spell.damage, spell.canDamageCaster, gameObject);
        }
    }

    private void HandleDeath(Health deadHealth)
    {
        if (spellPickupPrefab == null || assignedSpell == null)
            return;

        GameObject pickupObj = Instantiate(
            spellPickupPrefab,
            transform.position,
            Quaternion.identity
        );

        SpellPickup pickup = pickupObj.GetComponent<SpellPickup>();
        if (pickup != null)
        {
            pickup.spell = assignedSpell;
        }
    }

    public SpellData GetAssignedSpell()
    {
        return assignedSpell;
    }
}