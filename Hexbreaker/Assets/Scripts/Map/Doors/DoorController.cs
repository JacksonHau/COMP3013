using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorRenderer;

    private void Awake()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (doorRenderer == null)
            doorRenderer = GetComponent<SpriteRenderer>();
    }

    public void Configure(Vector2 size, bool horizontal)
    {
        BoxCollider2D box = doorCollider as BoxCollider2D;
        if (box != null)
        {
            box.size = size;
        }

        if (doorRenderer != null)
        {
            transform.rotation = horizontal
                ? Quaternion.identity
                : Quaternion.Euler(0f, 0f, 90f);
        }
    }

    public void SetLocked(bool locked)
    {
        if (doorCollider != null)
            doorCollider.enabled = locked;

        if (doorRenderer != null)
            doorRenderer.enabled = locked;
    }
}