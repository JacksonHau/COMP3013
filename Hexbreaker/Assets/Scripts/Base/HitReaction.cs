using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HitReaction : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private SpriteRenderer[] flashRenderers;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.08f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackDuration = 0.12f;

    private Rigidbody2D rb;
    private Color[][] originalColors;
    private Coroutine flashRoutine;
    private Coroutine knockbackRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (flashRenderers == null || flashRenderers.Length == 0)
        {
            flashRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        originalColors = new Color[flashRenderers.Length][];
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            originalColors[i] = new Color[1];
            originalColors[i][0] = flashRenderers[i].color;
        }
    }

    public void PlayReaction(Vector2 hitDirection)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
        knockbackRoutine = StartCoroutine(KnockbackRoutine(hitDirection));
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            if (flashRenderers[i] != null)
                flashRenderers[i].color = flashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < flashRenderers.Length; i++)
        {
            if (flashRenderers[i] != null)
                flashRenderers[i].color = originalColors[i][0];
        }

        flashRoutine = null;
    }

    private IEnumerator KnockbackRoutine(Vector2 hitDirection)
    {
        Vector2 dir = hitDirection.normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        knockbackRoutine = null;
    }
}