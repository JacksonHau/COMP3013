using System.Collections;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private SpriteRenderer[] fadeRenderers;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private Color deathColor = Color.black;

    [Header("Disable On Death")]
    [SerializeField] private Collider2D[] collidersToDisable;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;
    [SerializeField] private Rigidbody2D rb;

    private bool isDying = false;

    private void Awake()
    {
        if (fadeRenderers == null || fadeRenderers.Length == 0)
            fadeRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider2D>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    public void PlayDeath()
    {
        if (isDying)
            return;

        isDying = true;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // Disable colliders immediately
        foreach (Collider2D col in collidersToDisable)
        {
            if (col != null)
                col.enabled = false;
        }

        // Disable selected scripts
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        // Stop movement/physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // Cache starting colors
        Color[] startColors = new Color[fadeRenderers.Length];
        for (int i = 0; i < fadeRenderers.Length; i++)
        {
            if (fadeRenderers[i] != null)
                startColors[i] = fadeRenderers[i].color;
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            for (int i = 0; i < fadeRenderers.Length; i++)
            {
                if (fadeRenderers[i] != null)
                {
                    fadeRenderers[i].color = Color.Lerp(startColors[i], deathColor, t);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < fadeRenderers.Length; i++)
        {
            if (fadeRenderers[i] != null)
                fadeRenderers[i].color = deathColor;
        }

        Destroy(gameObject);
    }
}