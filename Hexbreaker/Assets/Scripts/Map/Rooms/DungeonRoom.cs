using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Start,
    Combat,
    Treasure,
    Shop,
    Boss
}

public class DungeonRoom
{
    public BoundsInt bounds;
    public Vector2Int center;
    public RoomType type;

    public HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    public List<DungeonRoom> connections = new List<DungeonRoom>();

    public DungeonRoom(BoundsInt bounds)
    {
        this.bounds = bounds;
        center = (Vector2Int)Vector3Int.RoundToInt(bounds.center);
    }
}