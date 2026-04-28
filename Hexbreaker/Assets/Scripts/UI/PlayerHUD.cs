using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private SpellCaster spellCaster;

    [Header("Health UI")]
    [SerializeField] private HeartUI heartUI;

    [Header("Spell Slots")]
    [SerializeField] private SpellSlotUI slot1UI;
    [SerializeField] private SpellSlotUI slot2UI;
    [SerializeField] private SpellSlotUI ultimateSlotUI;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
        }

        if (spellCaster != null)
        {
            spellCaster.OnSpellsChanged += HandleSpellsChanged;
            spellCaster.OnCooldownStarted += HandleCooldownStarted;
            spellCaster.OnUltimateEnergyChanged += HandleUltimateEnergyChanged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        if (spellCaster != null)
        {
            spellCaster.OnSpellsChanged -= HandleSpellsChanged;
            spellCaster.OnCooldownStarted -= HandleCooldownStarted;
            spellCaster.OnUltimateEnergyChanged -= HandleUltimateEnergyChanged;
        }
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        if (spellCaster != null)
        {
            HandleSpellsChanged(
                spellCaster.GetSpellInSlot(1),
                spellCaster.GetSpellInSlot(2),
                spellCaster.GetSpellInSlot(3)
            );

            HandleUltimateEnergyChanged(
                spellCaster.GetUltimateEnergy(),
                spellCaster.GetMaxUltimateEnergy()
            );
        }
    }

    private void Update()
    {
        if (spellCaster == null)
            return;

        UpdateSlotCooldown(1, slot1UI);
        UpdateSlotCooldown(2, slot2UI);
        UpdateSlotCooldown(3, ultimateSlotUI);
    }

    private void UpdateSlotCooldown(int slotIndex, SpellSlotUI slotUI)
    {
        if (slotUI == null)
            return;

        SpellData spell = spellCaster.GetSpellInSlot(slotIndex);

        if (spell != null)
        {
            float remaining = spellCaster.GetRemainingCooldown(slotIndex);
            float total = spellCaster.GetCooldownDuration(slotIndex);
            slotUI.SetCooldown(remaining, total);
        }
        else
        {
            slotUI.SetCooldown(0f, 1f);
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (heartUI != null)
        {
            heartUI.SetHearts(current, max);
        }
    }

    private void HandleSpellsChanged(SpellData slot1, SpellData slot2, SpellData ultimate)
    {
        if (slot1UI != null)
            slot1UI.SetSpell(slot1);

        if (slot2UI != null)
            slot2UI.SetSpell(slot2);

        if (ultimateSlotUI != null)
            ultimateSlotUI.SetSpell(ultimate);

        if (spellCaster != null && ultimateSlotUI != null)
        {
            ultimateSlotUI.SetEnergy(
                spellCaster.GetUltimateEnergy(),
                spellCaster.GetMaxUltimateEnergy()
            );
        }
    }

    private void HandleCooldownStarted(int slot, float duration)
    {
        
    }

    private void HandleUltimateEnergyChanged(float current, float max)
    {
        if (ultimateSlotUI != null)
        {
            ultimateSlotUI.SetEnergy(current, max);
        }
    }
}