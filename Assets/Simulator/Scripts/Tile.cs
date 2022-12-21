using UnityEngine;

namespace Simulator
{
    public enum TileType
    {
        GROUND,
        WALL,
        MOUNTAIN,
    }

    public class TileSet
    {
        public string Ground;
        public string Wall;
        public string Mountain;
    }

    public class TileData
    {
        public bool walkable;
        public int damage_rate;
    }

    public class Tile
    {
        public TileType type;
        public bool walkable;
        public int damageRate;
        public GameObject go;

        public Tile(TileType tileType, TileData tileData)
        {
            type = tileType;
            walkable = tileData.walkable;
            damageRate = tileData.damage_rate;
        }
    }
}