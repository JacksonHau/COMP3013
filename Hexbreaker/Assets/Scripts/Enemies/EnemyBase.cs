using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("References")]
    protected Rigidbody2D rb;
    protected Transform player;

    [Header("Perception")]
    [SerializeField] protected float detectionRadius = 8f;
    [SerializeField] protected float fieldOfView = 360f;
    [SerializeField] protected LayerMask obstacleMask;

    protected bool playerDetected;

    [Header("Combat")]
    [SerializeField] protected float contactDamage = 1f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    protected virtual void Update()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        if (player == null)
        {
            playerDetected = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        playerDetected = distance <= detectionRadius;
    }

    protected Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.zero;

        return (player.position - transform.position).normalized;
    }

    protected void RotateTowards(Vector2 direction, float offset = 0f)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rb.MoveRotation(angle + offset);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(contactDamage);
        }
    }

    protected bool CanSeePlayer()
    {
        return playerDetected;
    }

    protected Vector2 PlayerPosition()
    {
        if (player == null) return Vector2.zero;
        return player.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}