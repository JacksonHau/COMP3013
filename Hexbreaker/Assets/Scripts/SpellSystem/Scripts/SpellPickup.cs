using TMPro;
using UnityEngine;

public class SpellPickup : MonoBehaviour
{
    [Header("Spell")]
    public SpellData spell;

    [Header("Display")]
    [SerializeField] private TMP_Text worldLabel;
    [SerializeField] private GameObject promptObject;

    private void Start()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (worldLabel != null)
        {
            worldLabel.text = spell != null ? spell.spellName : "Empty Spell";
        }

        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }

    public void ShowPrompt(bool show)
    {
        if (promptObject != null)
        {
            promptObject.SetActive(show);
        }
    }

    public void PickupIntoSlot(SpellCaster caster, int slotIndex)
    {
        if (caster == null || spell == null)
            return;

        SpellData replacedSpell = caster.ReplaceSpellInSlot(slotIndex, spell);
        Vector3 dropPosition = transform.position;

        if (replacedSpell != null)
        {
            caster.SpawnSpellPickup(replacedSpell, dropPosition);
        }

        Destroy(gameObject);
    }
}