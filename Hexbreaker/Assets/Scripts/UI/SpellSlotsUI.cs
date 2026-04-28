using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;

    [Header("Energy Bar")]
    [SerializeField] private Image energyFill;

    public void SetSpell(SpellData spell)
    {
        if (spell != null && spell.icon != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = spell.icon;
        }
        else
        {
            iconImage.enabled = false;
        }

        SetCooldown(0f, 1f);
    }

    public void SetCooldown(float remaining, float total)
    {
        if (cooldownOverlay == null)
            return;

        if (remaining <= 0f || total <= 0f)
        {
            cooldownOverlay.fillAmount = 0f;

            if (cooldownText != null)
                cooldownText.text = "";

            return;
        }

        cooldownOverlay.fillAmount = remaining / total;

        if (cooldownText != null)
            cooldownText.text = remaining > 0.05f ? remaining.ToString("0.0") : "";
    }

    public void SetEnergy(float current, float max)
    {
        if (energyFill == null)
            return;

        energyFill.fillAmount = max > 0f ? current / max : 0f;
    }
}