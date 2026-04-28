using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform container;

    [Header("Settings")]
    [SerializeField] private float healthPerHeart = 20f;

    private List<Image> fillImages = new List<Image>();

    public void SetHearts(float currentHealth, float maxHealth)
    {
        if (healthPerHeart <= 0f)
            return;

        int requiredHearts = Mathf.Max(3, Mathf.CeilToInt(maxHealth / healthPerHeart));

        EnsureHeartCount(requiredHearts);

        for (int i = 0; i < fillImages.Count; i++)
        {
            float min = i * healthPerHeart;
            float max = min + healthPerHeart;

            float fill = Mathf.InverseLerp(min, max, currentHealth);
            fillImages[i].fillAmount = Mathf.Clamp01(fill);
        }
    }

    private void EnsureHeartCount(int count)
    {
        // Add hearts if needed
        while (fillImages.Count < count)
        {
            GameObject heart = Instantiate(heartPrefab, container);

            Image fill = heart.transform.Find("FillHeart").GetComponent<Image>();
            fillImages.Add(fill);
        }

        // Remove extra hearts
        while (fillImages.Count > count)
        {
            Image last = fillImages[fillImages.Count - 1];
            Destroy(last.transform.parent.gameObject);
            fillImages.RemoveAt(fillImages.Count - 1);
        }
    }
}