using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "LevelThemeData", menuName = "Level Design/Level Theme Data")]
public class LevelThemeData : ScriptableObject
{
    [Header("Ground Tiles")]
    [FormerlySerializedAs("grassTile")] public TileBase groundTile;
    [FormerlySerializedAs("dirtPathTile")] public TileBase pathTile;

    [Header("Structure Tiles")]
    [FormerlySerializedAs("castleFloorTile")] public TileBase floorTile;
    [FormerlySerializedAs("castleWallTile")] public TileBase wallTile;

    [Header("Prefabs")]
    public GameObject[] obstaclePrefabs;
    [FormerlySerializedAs("castlePropPrefabs")] public GameObject[] structurePropPrefabs;
    public GameObject[] foliagePrefabs;
}
