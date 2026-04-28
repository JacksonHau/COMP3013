using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CompositeCollider2D))]
public class RoomController : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemies = new List<GameObject>();
    [SerializeField] private List<DoorController> doors = new List<DoorController>();
    [SerializeField] private bool activateOnlyOnce = true;
    [SerializeField] private float activationDelay = 0.5f;

    [Header("Failsafe")]
    [SerializeField] private float lockCheckInterval = 0.2f;
    [SerializeField] private float insideGraceTime = 0.35f;

    private bool hasActivated = false;
    private bool roomCleared = false;
    private bool isLocked = false;
    private int aliveEnemies = 0;

    private Transform player;
    private Coroutine roomLockCheckRoutine;

    private CompositeCollider2D roomComposite;
    private Rigidbody2D rb;

    //dedicated container for generated trigger colliders
    private Transform triggerTileContainer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;

        roomComposite = GetComponent<CompositeCollider2D>();
        roomComposite.isTrigger = true;
        roomComposite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        roomComposite.generationType = CompositeCollider2D.GenerationType.Synchronous;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        EnsureTriggerTileContainer();
    }

    private void Start()
    {
        RebuildRuntimeState();
    }

    public void Setup(HashSet<Vector2Int> roomFloorTiles, List<GameObject> spawnedEnemies)
    {
        enemies = spawnedEnemies;
        gameObject.layer = LayerMask.NameToLayer("RoomTrigger");

        EnsureTriggerTileContainer();
        BuildRoomTriggerFromTiles(roomFloorTiles);
        RebuildRuntimeState();
    }

    private void EnsureTriggerTileContainer()
    {
        if (triggerTileContainer != null)
            return;

        Transform existing = transform.Find("TriggerTiles");
        if (existing != null)
        {
            triggerTileContainer = existing;
            return;
        }

        GameObject container = new GameObject("TriggerTiles");
        container.transform.SetParent(transform);
        container.transform.localPosition = Vector3.zero;
        triggerTileContainer = container.transform;
    }

    private void BuildRoomTriggerFromTiles(HashSet<Vector2Int> roomFloorTiles)
    {
        //clear generated trigger tiles
        List<Transform> childrenToDelete = new List<Transform>();
        foreach (Transform child in triggerTileContainer)
        {
            childrenToDelete.Add(child);
        }

        for (int i = 0; i < childrenToDelete.Count; i++)
        {
            DestroyImmediate(childrenToDelete[i].gameObject);
        }

        foreach (Vector2Int tile in roomFloorTiles)
        {
            GameObject tileTrigger = new GameObject($"TriggerTile_{tile.x}_{tile.y}");
            tileTrigger.transform.SetParent(triggerTileContainer);
            tileTrigger.transform.position = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f);
            tileTrigger.layer = LayerMask.NameToLayer("RoomTrigger");

            BoxCollider2D box = tileTrigger.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = Vector2.one;
            box.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        roomComposite.GenerateGeometry();
    }

    private void RebuildRuntimeState()
    {
        aliveEnemies = 0;
        roomCleared = false;
        hasActivated = false;
        isLocked = false;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.OnDeath -= HandleEnemyDeath;
                health.OnDeath += HandleEnemyDeath;
                aliveEnemies++;
            }
        }

        //Debug.Log($"{name} rebuilt. aliveEnemies={aliveEnemies}");
    }

    public void RegisterDoor(DoorController door)
    {
        if (door != null && !doors.Contains(door))
            doors.Add(door);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (roomCleared)
            return;

        if (hasActivated && activateOnlyOnce)
            return;

        hasActivated = true;
        StartCoroutine(ActivateRoomAfterDelay());
    }

    private IEnumerator ActivateRoomAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        ActivateEnemies();

        if (aliveEnemies > 0)
            LockRoom();
    }

    private void ActivateEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
                enemy.SetActive(true);
        }
    }

    private void HandleEnemyDeath(Health deadEnemy)
    {
        deadEnemy.OnDeath -= HandleEnemyDeath;
        aliveEnemies--;

        if (aliveEnemies <= 0)
            RoomCleared();
    }

    private void RoomCleared()
    {
        roomCleared = true;
        UnlockRoom();
        //Debug.Log($"{name} cleared!");
    }

    private void LockRoom()
    {
        foreach (DoorController door in doors)
        {
            if (door != null)
                door.SetLocked(true);
        }

        isLocked = true;

        if (roomLockCheckRoutine != null)
            StopCoroutine(roomLockCheckRoutine);

        roomLockCheckRoutine = StartCoroutine(LockedRoomFailsafeCheck());
    }

    private void UnlockRoom()
    {
        foreach (DoorController door in doors)
        {
            if (door != null)
                door.SetLocked(false);
        }

        isLocked = false;

        if (roomLockCheckRoutine != null)
        {
            StopCoroutine(roomLockCheckRoutine);
            roomLockCheckRoutine = null;
        }
    }

    private IEnumerator LockedRoomFailsafeCheck()
    {
        yield return new WaitForSeconds(insideGraceTime);

        while (isLocked)
        {
            if (!IsPlayerInsideRoom())
            {
                //Debug.LogWarning($"{name}: player not inside while locked, resetting room.");
                FailsafeResetRoom();
                yield break;
            }

            yield return new WaitForSeconds(lockCheckInterval);
        }
    }

    private bool IsPlayerInsideRoom()
    {
        if (player == null || roomComposite == null)
            return false;

        Vector2 p = player.position;

        return roomComposite.OverlapPoint(p) ||
               roomComposite.OverlapPoint(p + Vector2.up * 0.1f) ||
               roomComposite.OverlapPoint(p + Vector2.down * 0.1f) ||
               roomComposite.OverlapPoint(p + Vector2.left * 0.1f) ||
               roomComposite.OverlapPoint(p + Vector2.right * 0.1f);
    }

    private void FailsafeResetRoom()
    {
        UnlockRoom();

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        enemies.Clear();
        aliveEnemies = 0;
        hasActivated = false;
    }
}