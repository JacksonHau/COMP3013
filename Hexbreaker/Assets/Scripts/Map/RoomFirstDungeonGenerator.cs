using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [Header("Room Size")]
    [SerializeField] private int minRoomWidth = 4;
    [SerializeField] private int minRoomHeight = 4;

    [Header("Dungeon Size")]
    [SerializeField] private int dungeonWidth = 50;
    [SerializeField] private int dungeonHeight = 50;

    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;

    [SerializeField] private bool randomWalkRooms = false;

    private List<DungeonRoom> debugRooms;

    [Header("Room Prefabs")]
    [SerializeField] private RoomPrefabSet[] roomPrefabs;

    [Header("Doors")]
    [SerializeField] private GameObject doorPrefab;

    [Header("Room Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] fillerPrefabs;

    [SerializeField] private int minEnemiesPerRoom = 1;
    [SerializeField] private int maxEnemiesPerRoom = 3;

    [SerializeField] private int minFillerPerRoom = 2;
    [SerializeField] private int maxFillerPerRoom = 5;

    [Header("Spawn Container")]
    [SerializeField] private Transform spawnedRoomObjectsParent;

    // One doorway anchor per room connection.
    private Dictionary<DungeonRoom, List<Vector2Int>> roomDoorPositions =
        new Dictionary<DungeonRoom, List<Vector2Int>>();

    private void Start()
    {
        GenerateDungeon();
    }

    protected override void RunProceduralGeneration()
    {
        ClearSpawnedRoomObjects();
        CreateRooms();
    }

    private void CreateRooms()
    {
        roomDoorPositions.Clear();

        if (spawnedRoomObjectsParent == null)
        {
            GameObject parent = new GameObject("GeneratedRoomObjects");
            spawnedRoomObjectsParent = parent.transform;
        }

        var bspRooms = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt((Vector3Int)startPosition,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth,
            minRoomHeight);

        List<DungeonRoom> dungeonRooms = new List<DungeonRoom>();
        debugRooms = dungeonRooms;

        foreach (var roomBounds in bspRooms)
        {
            DungeonRoom dungeonRoom = new DungeonRoom(roomBounds);
            dungeonRooms.Add(dungeonRoom);
            roomDoorPositions[dungeonRoom] = new List<Vector2Int>();
        }

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        if (randomWalkRooms)
            floor = CreateRoomsRandomly(dungeonRooms);
        else
            floor = CreateSimpleRooms(dungeonRooms);

        HashSet<Vector2Int> corridors = ConnectRooms(dungeonRooms);
        floor.UnionWith(corridors);

        tilemapVisualiser.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualiser);

        AssignRoomTypes(dungeonRooms);
        SpawnRoomPrefabs(dungeonRooms);
        SpawnRoomContents(dungeonRooms, corridors);
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<DungeonRoom> rooms)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        foreach (var room in rooms)
        {
            for (int col = offset; col < room.bounds.size.x - offset; col++)
            {
                for (int row = offset; row < room.bounds.size.y - offset; row++)
                {
                    Vector2Int position =
                        (Vector2Int)room.bounds.min + new Vector2Int(col, row);

                    floor.Add(position);
                    room.floorTiles.Add(position);
                }
            }
        }

        return floor;
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<DungeonRoom> rooms)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        foreach (var room in rooms)
        {
            var roomCenter = room.center;
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);

            foreach (var position in roomFloor)
            {
                if (position.x >= room.bounds.xMin + offset &&
                    position.x <= room.bounds.xMax - offset &&
                    position.y >= room.bounds.yMin + offset &&
                    position.y <= room.bounds.yMax - offset)
                {
                    floor.Add(position);
                    room.floorTiles.Add(position);
                }
            }
        }

        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<DungeonRoom> rooms)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        DungeonRoom currentRoom = rooms[Random.Range(0, rooms.Count)];
        List<DungeonRoom> remainingRooms = new List<DungeonRoom>(rooms);
        remainingRooms.Remove(currentRoom);

        while (remainingRooms.Count > 0)
        {
            DungeonRoom closest = FindClosestRoom(currentRoom, remainingRooms);
            remainingRooms.Remove(closest);

            List<Vector2Int> corridorPath = CreateCorridorPath(currentRoom.center, closest.center);

            foreach (Vector2Int tile in corridorPath)
                corridors.Add(tile);

            RegisterDoorPositions(currentRoom, closest, corridorPath);

            currentRoom.connections.Add(closest);
            closest.connections.Add(currentRoom);

            currentRoom = closest;
        }

        return corridors;
    }

    private DungeonRoom FindClosestRoom(DungeonRoom current, List<DungeonRoom> rooms)
    {
        DungeonRoom closest = null;
        float distance = float.MaxValue;

        foreach (var room in rooms)
        {
            float d = Vector2.Distance(room.center, current.center);

            if (d < distance)
            {
                distance = d;
                closest = room;
            }
        }

        return closest;
    }

    private List<Vector2Int> CreateCorridorPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();

        Vector2Int position = start;
        corridor.Add(position);

        while (position.y != end.y)
        {
            if (end.y > position.y)
                position += Vector2Int.up;
            else if (end.y < position.y)
                position += Vector2Int.down;

            corridor.Add(position);
        }

        while (position.x != end.x)
        {
            if (end.x > position.x)
                position += Vector2Int.right;
            else if (end.x < position.x)
                position += Vector2Int.left;

            corridor.Add(position);
        }

        return corridor;
    }

    private void RegisterDoorPositions(DungeonRoom roomA, DungeonRoom roomB, List<Vector2Int> corridorPath)
    {
        // First corridor tile outside roomA
        for (int i = 1; i < corridorPath.Count; i++)
        {
            if (!roomA.floorTiles.Contains(corridorPath[i]) &&
                 roomA.floorTiles.Contains(corridorPath[i - 1]))
            {
                roomDoorPositions[roomA].Add(corridorPath[i]);
                break;
            }
        }

        // First corridor tile outside roomB from the far end
        for (int i = corridorPath.Count - 2; i >= 0; i--)
        {
            if (!roomB.floorTiles.Contains(corridorPath[i]) &&
                 roomB.floorTiles.Contains(corridorPath[i + 1]))
            {
                roomDoorPositions[roomB].Add(corridorPath[i]);
                break;
            }
        }
    }

    private void AssignRoomTypes(List<DungeonRoom> rooms)
    {
        if (rooms.Count == 0)
            return;

        DungeonRoom startRoom = rooms[0];
        startRoom.type = RoomType.Start;

        List<DungeonRoom> leafRooms = new List<DungeonRoom>();

        foreach (var room in rooms)
        {
            if (room.connections.Count == 1 && room != startRoom)
            {
                leafRooms.Add(room);
            }
        }

        DungeonRoom bossRoom;

        if (leafRooms.Count > 0)
            bossRoom = leafRooms[Random.Range(0, leafRooms.Count)];
        else
            bossRoom = rooms[rooms.Count - 1];

        bossRoom.type = RoomType.Boss;

        foreach (var room in rooms)
        {
            if (room == startRoom || room == bossRoom)
                continue;

            float roll = Random.value;

            if (roll < 0.7f)
                room.type = RoomType.Combat;
            else if (roll < 0.9f)
                room.type = RoomType.Treasure;
            else
                room.type = RoomType.Shop;
        }
    }

    private void SpawnRoomPrefabs(List<DungeonRoom> rooms)
    {
        foreach (var room in rooms)
        {
            RoomPrefabSet prefabSet =
                System.Array.Find(roomPrefabs, x => x.type == room.type);

            if (prefabSet == null || prefabSet.prefabs.Length == 0)
                continue;

            GameObject prefab =
                prefabSet.prefabs[Random.Range(0, prefabSet.prefabs.Length)];

            Instantiate(prefab,
                new Vector3(room.center.x + 0.5f, room.center.y + 0.5f, 0),
                Quaternion.identity,
                spawnedRoomObjectsParent);
        }
    }

    private Quaternion GetDoorRotation(Vector2Int doorTile, DungeonRoom room)
    {
        bool hasRoomLeft = room.floorTiles.Contains(doorTile + Vector2Int.left);
        bool hasRoomRight = room.floorTiles.Contains(doorTile + Vector2Int.right);

        // Room is left/right of the corridor tile -> doorway runs vertically
        if (hasRoomLeft || hasRoomRight)
            return Quaternion.identity;

        // Room is above/below -> doorway runs horizontally
        return Quaternion.Euler(0, 0, 90);
    }

    private bool TouchesRoom(Vector2Int tile, DungeonRoom room)
    {
        foreach (Vector2Int dir in Direction2D.cardinalDirectionsList)
        {
            if (room.floorTiles.Contains(tile + dir))
                return true;
        }

        return false;
    }

    private List<Vector2Int> GetDoorwaySegmentTiles(
        Vector2Int doorTile,
        DungeonRoom room,
        HashSet<Vector2Int> corridors)
    {
        List<Vector2Int> segmentTiles = new List<Vector2Int>();

        bool roomLeft = room.floorTiles.Contains(doorTile + Vector2Int.left);
        bool roomRight = room.floorTiles.Contains(doorTile + Vector2Int.right);

        // Vertical doorway: expand up/down
        if (roomLeft || roomRight)
        {
            int minY = doorTile.y;
            int maxY = doorTile.y;

            while (corridors.Contains(new Vector2Int(doorTile.x, minY - 1)) &&
                   TouchesRoom(new Vector2Int(doorTile.x, minY - 1), room))
            {
                minY--;
            }

            while (corridors.Contains(new Vector2Int(doorTile.x, maxY + 1)) &&
                   TouchesRoom(new Vector2Int(doorTile.x, maxY + 1), room))
            {
                maxY++;
            }

            for (int y = minY; y <= maxY; y++)
            {
                segmentTiles.Add(new Vector2Int(doorTile.x, y));
            }
        }
        else
        {
            // Horizontal doorway: expand left/right
            int minX = doorTile.x;
            int maxX = doorTile.x;

            while (corridors.Contains(new Vector2Int(minX - 1, doorTile.y)) &&
                   TouchesRoom(new Vector2Int(minX - 1, doorTile.y), room))
            {
                minX--;
            }

            while (corridors.Contains(new Vector2Int(maxX + 1, doorTile.y)) &&
                   TouchesRoom(new Vector2Int(maxX + 1, doorTile.y), room))
            {
                maxX++;
            }

            for (int x = minX; x <= maxX; x++)
            {
                segmentTiles.Add(new Vector2Int(x, doorTile.y));
            }
        }

        return segmentTiles;
    }

    private void SpawnDoorSegments(
        Vector2Int doorTile,
        DungeonRoom room,
        HashSet<Vector2Int> corridors,
        Transform parent,
        RoomController roomController)
    {
        List<Vector2Int> segmentTiles = GetDoorwaySegmentTiles(doorTile, room, corridors);

        foreach (Vector2Int segmentTile in segmentTiles)
        {
            Vector3 doorPos = new Vector3(segmentTile.x + 0.5f, segmentTile.y + 0.5f, 0f);
            Quaternion rot = GetDoorRotation(segmentTile, room);

            GameObject doorObj = Instantiate(doorPrefab, doorPos, rot, parent);
            DoorController door = doorObj.GetComponent<DoorController>();

            if (door != null)
            {
                roomController.RegisterDoor(door);
                door.SetLocked(false);
            }
        }
    }

    private void SpawnRoomContents(List<DungeonRoom> rooms, HashSet<Vector2Int> corridors)
    {
        foreach (var room in rooms)
        {
            if (room.type == RoomType.Start)
                continue;

            GameObject roomContainer = new GameObject($"Room_{room.type}_{room.center.x}_{room.center.y}");
            roomContainer.transform.SetParent(spawnedRoomObjectsParent);

            List<Vector2Int> tiles = new List<Vector2Int>(room.floorTiles);
            HashSet<Vector2Int> usedTiles = new HashSet<Vector2Int>();
            List<GameObject> spawnedEnemies = new List<GameObject>();

            RoomController roomController = null;

            if (room.type == RoomType.Combat || room.type == RoomType.Boss)
            {
                int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

                for (int i = 0; i < enemyCount; i++)
                {
                    Vector2Int tile = GetRandomFreeTile(tiles, usedTiles);
                    if (tile == Vector2Int.zero) break;

                    GameObject enemy = SpawnObject(enemyPrefabs, tile, roomContainer.transform);
                    if (enemy != null)
                    {
                        enemy.SetActive(false);
                        spawnedEnemies.Add(enemy);
                    }

                    usedTiles.Add(tile);
                }

                roomController = roomContainer.AddComponent<RoomController>();
                roomController.Setup(room.floorTiles, spawnedEnemies);
            }

            int fillerCount = Random.Range(minFillerPerRoom, maxFillerPerRoom + 1);

            for (int i = 0; i < fillerCount; i++)
            {
                Vector2Int tile = GetRandomFreeTile(tiles, usedTiles);
                if (tile == Vector2Int.zero) break;

                SpawnObject(fillerPrefabs, tile, roomContainer.transform);
                usedTiles.Add(tile);
            }

            if ((room.type == RoomType.Combat || room.type == RoomType.Boss) &&
                roomController != null &&
                doorPrefab != null &&
                roomDoorPositions.ContainsKey(room))
            {
                HashSet<Vector2Int> usedDoorTiles = new HashSet<Vector2Int>();

                foreach (Vector2Int doorTile in roomDoorPositions[room])
                {
                    if (usedDoorTiles.Contains(doorTile))
                        continue;

                    List<Vector2Int> segmentTiles = GetDoorwaySegmentTiles(doorTile, room, corridors);

                    foreach (Vector2Int tile in segmentTiles)
                        usedDoorTiles.Add(tile);

                    SpawnDoorSegments(doorTile, room, corridors, roomContainer.transform, roomController);
                }
            }
        }
    }

    private Vector2Int GetRandomFreeTile(List<Vector2Int> tiles, HashSet<Vector2Int> used)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        foreach (var tile in tiles)
        {
            if (!used.Contains(tile))
                candidates.Add(tile);
        }

        if (candidates.Count == 0)
            return Vector2Int.zero;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private GameObject SpawnObject(GameObject[] prefabs, Vector2Int tile, Transform parent)
    {
        if (prefabs == null || prefabs.Length == 0)
            return null;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        Vector3 pos = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0);

        return Instantiate(prefab, pos, Quaternion.identity, parent);
    }

    private void ClearSpawnedRoomObjects()
    {
        if (spawnedRoomObjectsParent == null) return;

        for (int i = spawnedRoomObjectsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnedRoomObjectsParent.GetChild(i).gameObject);
        }
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (debugRooms == null) return;

        foreach (var room in debugRooms)
        {
            Vector3 pos = new Vector3(room.center.x + 0.5f, room.center.y + 0.5f, 0);

            Handles.color = Color.white;
            Handles.Label(pos + Vector3.up * 0.5f, room.type.ToString());
        }
#endif
    }
}