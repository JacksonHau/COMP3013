using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellCaster : MonoBehaviour
{
    [Header("Spell Slots")]
    public SpellData spellSlot1;
    public SpellData spellSlot2;
    public SpellData ultimateSlot;

    [Header("Casting")]
    public Transform castPoint;
    [SerializeField] private LineRenderer laserRenderer;

    [Header("Ultimate")]
    [SerializeField] private float ultimateEnergy = 0f;
    [SerializeField] private float maxUltimateEnergy = 100f;
    [SerializeField] private float ultimateCost = 100f;

    [Header("Pickup Settings")]
    [SerializeField] private GameObject spellPickupPrefab;
    [SerializeField] private Vector3 droppedPickupOffset = new Vector3(0.4f, 0f, 0f);

    private SpellPickup nearbyPickup;
    private PlayerInputActions inputActions;

    private float lastCastTime1;
    private float lastCastTime2;
    private float lastUltimateCastTime;

    private bool laserActive = false;

    public float GetUltimateEnergy() => ultimateEnergy;
    public float GetMaxUltimateEnergy() => maxUltimateEnergy;

    public event Action<SpellData, SpellData, SpellData> OnSpellsChanged;
    public event Action<int, float> OnCooldownStarted;
    public event Action<float, float> OnUltimateEnergyChanged;

    private struct RuntimeSpellStats
    {
        public float damage;
        public float speed;
        public float range;
        public float cooldown;
        public float spawnOffset;
        public int extraProjectiles;
        public int extraBounces;
        public int extraPulses;
        public bool canDamageCaster;
        public float sizeMultiplier;
    }

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        if (laserRenderer == null)
            laserRenderer = gameObject.AddComponent<LineRenderer>();

        laserRenderer.positionCount = 2;
        laserRenderer.startWidth = 0.1f;
        laserRenderer.endWidth = 0.1f;
        laserRenderer.enabled = false;
    }

    private void Start()
    {
        NotifySpellsChanged();
        OnUltimateEnergyChanged?.Invoke(ultimateEnergy, maxUltimateEnergy);
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.FireLeft.performed += OnFireLeft;
        inputActions.Player.FireRight.performed += OnFireRight;
        inputActions.Player.PickupSlot1.performed += OnPickupSlot1;
        inputActions.Player.PickupSlot2.performed += OnPickupSlot2;
        inputActions.Player.PickupSlot3.performed += OnPickupSlot3;
        inputActions.Player.CastUltimate.performed += OnCastUltimate;
    }

    private void OnDisable()
    {
        inputActions.Player.FireLeft.performed -= OnFireLeft;
        inputActions.Player.FireRight.performed -= OnFireRight;
        inputActions.Player.PickupSlot1.performed -= OnPickupSlot1;
        inputActions.Player.PickupSlot2.performed -= OnPickupSlot2;
        inputActions.Player.PickupSlot3.performed -= OnPickupSlot3;
        inputActions.Player.CastUltimate.performed -= OnCastUltimate;

        inputActions.Disable();
    }

    private void OnFireLeft(InputAction.CallbackContext ctx) => TryCast(1);
    private void OnFireRight(InputAction.CallbackContext ctx) => TryCast(2);
    private void OnCastUltimate(InputAction.CallbackContext ctx) => TryCastUltimate();

    private void OnPickupSlot1(InputAction.CallbackContext ctx)
    {
        if (nearbyPickup != null)
            nearbyPickup.PickupIntoSlot(this, 1);
    }

    private void OnPickupSlot2(InputAction.CallbackContext ctx)
    {
        if (nearbyPickup != null)
            nearbyPickup.PickupIntoSlot(this, 2);
    }

    private void OnPickupSlot3(InputAction.CallbackContext ctx)
    {
        if (nearbyPickup != null)
            nearbyPickup.PickupIntoSlot(this, 3);
    }

    public void TryCast(int slot)
    {
        SpellData spell = slot == 1 ? spellSlot1 : spellSlot2;

        if (spell == null)
            return;

        float lastTime = slot == 1 ? lastCastTime1 : lastCastTime2;
        RuntimeSpellStats stats = GetSpellStats(spell, false, slot);

        if (Time.time < lastTime + stats.cooldown)
            return;

        CastSpell(spell, false, slot);

        if (slot == 1)
            lastCastTime1 = Time.time;
        else
            lastCastTime2 = Time.time;

        OnCooldownStarted?.Invoke(slot, stats.cooldown);
    }

    public void TryCastUltimate()
    {
        if (ultimateSlot == null)
            return;

        RuntimeSpellStats stats = GetSpellStats(ultimateSlot, true, 3);

        if (ultimateEnergy < ultimateCost)
            return;

        if (Time.time < lastUltimateCastTime + stats.cooldown)
            return;

        ultimateEnergy = 0f;
        OnUltimateEnergyChanged?.Invoke(ultimateEnergy, maxUltimateEnergy);

        CastSpell(ultimateSlot, true, 3);

        lastUltimateCastTime = Time.time;
        OnCooldownStarted?.Invoke(3, stats.cooldown);
    }

    public void AddUltimateEnergy(float amount)
    {
        ultimateEnergy = Mathf.Clamp(ultimateEnergy + amount, 0f, maxUltimateEnergy);
        OnUltimateEnergyChanged?.Invoke(ultimateEnergy, maxUltimateEnergy);
    }

    private void CastSpell(SpellData spell, bool isUltimate, int slot)
    {
        RuntimeSpellStats stats = GetSpellStats(spell, isUltimate, slot);
        Vector2 direction = GetMouseDirection();

        switch (spell.spellType)
        {
            case SpellType.Projectile:
                SpawnProjectile(spell, stats, direction);
                break;

            case SpellType.Laser:
                FireLaser(spell, stats);
                break;

            case SpellType.ExplosiveProjectile:
                SpawnExplosiveProjectile(spell, stats, direction);
                break;

            case SpellType.Nova:
                if (isUltimate && stats.extraPulses > 0)
                    StartCoroutine(UltimateNovaRoutine(spell, stats));
                else
                    CastNova(spell, stats);
                break;

            case SpellType.BounceProjectile:
                SpawnBounceProjectile(spell, stats, direction);
                break;
        }
    }

    private RuntimeSpellStats GetSpellStats(SpellData spell, bool isUltimate, int slot)
    {
        RuntimeSpellStats stats = new RuntimeSpellStats
        {
            damage = spell.damage,
            speed = spell.speed,
            range = spell.range,
            cooldown = spell.cooldown,
            spawnOffset = spell.spawnOffset,
            extraProjectiles = 0,
            extraBounces = 0,
            extraPulses = 0,
            canDamageCaster = spell.canDamageCaster,
            sizeMultiplier = 1f
        };

        if (isUltimate)
        {
            stats.damage *= spell.ultimateDamageMultiplier;
            stats.speed *= spell.ultimateSpeedMultiplier;
            stats.range *= spell.ultimateRangeMultiplier;
            stats.cooldown *= spell.ultimateCooldownMultiplier;
            stats.spawnOffset *= spell.ultimateSizeMultiplier;
            stats.sizeMultiplier *= spell.ultimateSizeMultiplier;
            stats.extraProjectiles += spell.ultimateExtraProjectiles;
            stats.extraBounces += spell.ultimateExtraBounces;
            stats.extraPulses += spell.ultimateExtraPulses;
        }

        ApplyUpgrades(ref stats, slot, isUltimate);

        stats.cooldown = Mathf.Max(0.05f, stats.cooldown);
        stats.speed = Mathf.Max(0.1f, stats.speed);
        stats.range = Mathf.Max(0.1f, stats.range);
        stats.sizeMultiplier = Mathf.Max(0.1f, stats.sizeMultiplier);

        return stats;
    }

    private void ApplyUpgrades(ref RuntimeSpellStats stats, int slot, bool isUltimate)
    {
        if (PlayerUpgradeManager.Instance == null)
            return;

        UpgradeTarget target = GetUpgradeTargetForSlot(slot, isUltimate);
        List<UpgradeData> upgrades = PlayerUpgradeManager.Instance.GetUpgradesForTarget(target);

        foreach (UpgradeData upgrade in upgrades)
        {
            switch (upgrade.effectType)
            {
                case UpgradeEffectType.DamageMultiplier:
                    stats.damage *= upgrade.value;
                    break;

                case UpgradeEffectType.ExtraProjectiles:
                    stats.extraProjectiles += Mathf.RoundToInt(upgrade.value);
                    break;

                case UpgradeEffectType.CooldownReduction:
                    stats.cooldown *= 1f - upgrade.value;
                    break;

                case UpgradeEffectType.Special:
                    ApplySpecialUpgrade(upgrade.specialID, upgrade.value, ref stats);
                    break;
            }
        }
    }

    private UpgradeTarget GetUpgradeTargetForSlot(int slot, bool isUltimate)
    {
        if (isUltimate || slot == 3)
            return UpgradeTarget.Ultimate;

        if (slot == 1)
            return UpgradeTarget.SpellSlot1;

        if (slot == 2)
            return UpgradeTarget.SpellSlot2;

        return UpgradeTarget.Global;
    }

    private void ApplySpecialUpgrade(string specialID, float value, ref RuntimeSpellStats stats)
    {
        switch (specialID)
        {
            case "BIG_PROJECTILES":
                stats.sizeMultiplier += value;
                break;

            case "BOUNCE":
                stats.extraBounces += Mathf.RoundToInt(value);
                break;

            case "NOVA_PULSE":
                stats.extraPulses += Mathf.RoundToInt(value);
                break;

            case "LONG_RANGE":
                stats.range *= value;
                break;

            case "FAST_PROJECTILES":
                stats.speed *= value;
                break;

            case "ULTIMATE_DAMAGE":
                stats.damage *= value;
                break;

            case "ULTIMATE_EXTRA_PROJECTILES":
                stats.extraProjectiles += Mathf.RoundToInt(value);
                break;
        }
    }

    private Vector2 GetMouseDirection()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
        return (worldMouse - (Vector2)castPoint.position).normalized;
    }

    private Vector2 GetSpawnPosition(RuntimeSpellStats stats, Vector2 direction)
    {
        return (Vector2)castPoint.position + direction * stats.spawnOffset;
    }

    private bool IsCasterCollider(Collider2D col)
    {
        return col != null &&
               (col.gameObject == gameObject ||
                col.transform.root.gameObject == transform.root.gameObject);
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

    private void SpawnProjectile(SpellData spell, RuntimeSpellStats stats, Vector2 direction)
    {
        int projectileCount = 1 + stats.extraProjectiles;

        if (projectileCount <= 1)
        {
            SpawnSingleProjectile(spell, stats, direction);
            return;
        }

        float spreadAngle = 20f;

        for (int i = 0; i < projectileCount; i++)
        {
            float t = projectileCount == 1 ? 0f : (float)i / (projectileCount - 1);
            float angle = Mathf.Lerp(-spreadAngle, spreadAngle, t);
            Vector2 rotatedDir = Quaternion.Euler(0f, 0f, angle) * direction;
            SpawnSingleProjectile(spell, stats, rotatedDir);
        }
    }

    private void SpawnSingleProjectile(SpellData spell, RuntimeSpellStats stats, Vector2 direction)
    {
        if (spell.projectilePrefab == null)
            return;

        Vector2 spawnPos = GetSpawnPosition(stats, direction);

        GameObject proj = Instantiate(spell.projectilePrefab, spawnPos, Quaternion.identity);
        proj.transform.localScale *= stats.sizeMultiplier;

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(direction, stats.speed, stats.damage, stats.canDamageCaster, gameObject);
        }
    }

    private void SpawnExplosiveProjectile(SpellData spell, RuntimeSpellStats stats, Vector2 direction)
    {
        if (spell.projectilePrefab == null)
            return;

        Vector2 spawnPos = GetSpawnPosition(stats, direction);

        GameObject proj = Instantiate(spell.projectilePrefab, spawnPos, Quaternion.identity);
        proj.transform.localScale *= stats.sizeMultiplier;

        ExplosiveProjectile explosive = proj.GetComponent<ExplosiveProjectile>();
        if (explosive != null)
        {
            explosive.Init(direction, stats.speed, stats.damage, stats.canDamageCaster, gameObject, spell.castEffect);
        }
    }

    private void SpawnBounceProjectile(SpellData spell, RuntimeSpellStats stats, Vector2 direction)
    {
        if (spell.projectilePrefab == null)
            return;

        Vector2 spawnPos = GetSpawnPosition(stats, direction);

        GameObject proj = Instantiate(spell.projectilePrefab, spawnPos, Quaternion.identity);
        proj.transform.localScale *= stats.sizeMultiplier;

        BounceProjectile bounce = proj.GetComponent<BounceProjectile>();
        if (bounce != null)
        {
            bounce.Init(direction, stats.speed, stats.damage, stats.canDamageCaster, gameObject);
            bounce.SetExtraBounces(stats.extraBounces);
        }
    }

    private void FireLaser(SpellData spell, RuntimeSpellStats stats)
    {
        if (!laserActive)
            StartCoroutine(PlayerLaserRoutine(spell, stats));
    }

    private IEnumerator PlayerLaserRoutine(SpellData spell, RuntimeSpellStats stats)
    {
        laserActive = true;

        try
        {
            Vector2 lockedDirection = GetMouseDirection();
            Vector2 lockedStart = GetSpawnPosition(stats, lockedDirection);
            Vector2 lockedEnd = lockedStart + lockedDirection * stats.range;

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
                    Vector2 direction = GetMouseDirection();
                    Vector2 startPoint = GetSpawnPosition(stats, direction);
                    Vector2 endPoint = startPoint + direction * stats.range;

                    RaycastHit2D[] hits = Physics2D.RaycastAll(startPoint, direction, stats.range);

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
            laserRenderer.startWidth = spell.laserWidth * stats.sizeMultiplier;
            laserRenderer.endWidth = spell.laserWidth * stats.sizeMultiplier;
            laserRenderer.SetPosition(0, lockedStart);
            laserRenderer.SetPosition(1, lockedEnd);

            RaycastHit2D[] damageHits = Physics2D.RaycastAll(lockedStart, lockedDirection, stats.range);

            for (int i = 0; i < damageHits.Length; i++)
            {
                if (ShouldIgnoreLaserHit(spell, damageHits[i].collider))
                    continue;

                IDamageable dmg = damageHits[i].collider.GetComponentInParent<IDamageable>();

                if (dmg == null)
                    continue;

                if (dmg is Health health)
                    health.TakeDamage(stats.damage, lockedDirection);
                else
                    dmg.TakeDamage(stats.damage);

                AddUltimateEnergy(stats.damage);

                if (spell != ultimateSlot)
                    break;
            }

            yield return new WaitForSeconds(spell.laserDuration);
        }
        finally
        {
            if (laserRenderer != null)
                laserRenderer.enabled = false;

            laserActive = false;
        }
    }

    private void CastNova(SpellData spell, RuntimeSpellStats stats)
    {
        if (spell.castEffect != null)
        {
            GameObject effect = Instantiate(spell.castEffect, castPoint.position, Quaternion.identity);
            effect.transform.localScale *= stats.sizeMultiplier;
            Destroy(effect, 1f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(castPoint.position, stats.range);

        foreach (Collider2D hit in hits)
        {
            if (!stats.canDamageCaster)
            {
                if (hit.gameObject == gameObject ||
                    hit.transform.root.gameObject == transform.root.gameObject)
                {
                    continue;
                }
            }

            IDamageable dmg = hit.GetComponentInParent<IDamageable>();

            if (dmg == null)
                continue;

            Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)castPoint.position).normalized;

            if (dmg is Health health)
                health.TakeDamage(stats.damage, knockbackDir);
            else
                dmg.TakeDamage(stats.damage);

            AddUltimateEnergy(stats.damage);
        }
    }

    private IEnumerator UltimateNovaRoutine(SpellData spell, RuntimeSpellStats stats)
    {
        int pulses = 1 + stats.extraPulses;

        for (int i = 0; i < pulses; i++)
        {
            CastNova(spell, stats);
            yield return new WaitForSeconds(0.15f);
        }
    }

    public float GetRemainingCooldown(int slot)
    {
        SpellData spell = GetSpellInSlot(slot);

        if (spell == null)
            return 0f;

        float lastTime = slot switch
        {
            1 => lastCastTime1,
            2 => lastCastTime2,
            3 => lastUltimateCastTime,
            _ => 0f
        };

        bool isUltimate = slot == 3;
        RuntimeSpellStats stats = GetSpellStats(spell, isUltimate, slot);

        float remaining = (lastTime + stats.cooldown) - Time.time;
        return Mathf.Max(0f, remaining);
    }

    public float GetCooldownDuration(int slot)
    {
        SpellData spell = GetSpellInSlot(slot);

        if (spell == null)
            return 1f;

        bool isUltimate = slot == 3;
        RuntimeSpellStats stats = GetSpellStats(spell, isUltimate, slot);

        return stats.cooldown;
    }

    public SpellData GetSpellInSlot(int slot)
    {
        return slot switch
        {
            1 => spellSlot1,
            2 => spellSlot2,
            3 => ultimateSlot,
            _ => null
        };
    }

    public SpellData ReplaceSpellInSlot(int slotIndex, SpellData newSpell)
    {
        SpellData replacedSpell = null;

        if (slotIndex == 1)
        {
            replacedSpell = spellSlot1;
            spellSlot1 = newSpell;
        }
        else if (slotIndex == 2)
        {
            replacedSpell = spellSlot2;
            spellSlot2 = newSpell;
        }
        else if (slotIndex == 3)
        {
            replacedSpell = ultimateSlot;
            ultimateSlot = newSpell;
        }

        NotifySpellsChanged();
        return replacedSpell;
    }

    public void SpawnSpellPickup(SpellData spellToDrop, Vector3 worldPosition)
    {
        if (spellPickupPrefab == null || spellToDrop == null)
            return;

        Vector3 spawnPos = worldPosition + droppedPickupOffset;

        GameObject pickupObj = Instantiate(spellPickupPrefab, spawnPos, Quaternion.identity);

        SpellPickup pickup = pickupObj.GetComponent<SpellPickup>();
        if (pickup != null)
        {
            pickup.spell = spellToDrop;
            pickup.RefreshDisplay();
        }
    }

    public void SetUltimateSpell(SpellData spell)
    {
        ultimateSlot = spell;
        NotifySpellsChanged();
    }

    private void NotifySpellsChanged()
    {
        OnSpellsChanged?.Invoke(spellSlot1, spellSlot2, ultimateSlot);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SpellPickup pickup = other.GetComponent<SpellPickup>();

        if (pickup != null)
        {
            nearbyPickup = pickup;
            pickup.ShowPrompt(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        SpellPickup pickup = other.GetComponent<SpellPickup>();

        if (pickup != null && pickup == nearbyPickup)
        {
            pickup.ShowPrompt(false);
            nearbyPickup = null;
        }
    }
}