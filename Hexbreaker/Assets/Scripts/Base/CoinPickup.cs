using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    [Header("Magnet Settings")]
    [SerializeField] private float magnetRange = 2f;
    [SerializeField] private float moveSpeed = 8f;

    [Header("Magnet Delay")]
    [SerializeField] private float magnetDelay = 0.25f;

    private Transform player;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null)
            return;

        if (Time.time < spawnTime + magnetDelay)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= magnetRange)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        CoinManager.Instance.AddCoins(coinValue);
        Destroy(gameObject);
    }
}