using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float lifetime = 1f;

    private TMP_Text text;

    public void Setup(string message, Color color, float life)
    {
        lifetime = life;

        text = GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = message;
            text.color = color;
        }
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}