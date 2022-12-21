using UnityEngine;

namespace Simulator
{
    public static class TileManager
    {
        public static TileSet tileSet;
        public static TileData GetTileData(int key)
        {
            TileData tileData = null;
            if (key == 0)
                tileData = JsonUtility.FromJson<TileData>(tileSet.Ground);
            else if (key == 1)
                tileData = JsonUtility.FromJson<TileData>(tileSet.Wall);
            else if (key == 2)
                tileData = JsonUtility.FromJson<TileData>(tileSet.Mountain);

            return tileData;
        }

        public static float CalcDamageRate(Tile attacker, Tile target)
        {
            var damageRate = attacker.damageRate - target.damageRate;
            if (damageRate > 0)
            {
                Debug.Log("지형으로 인해 공격자가 데미지 증가를 얻습니다.");
            }
            else if (damageRate < 0)
            {
                Debug.Log("지형으로 인해 수비자가 데미지 감소를 얻습니다.");
            }

            return 1.0f; // TOOD: 지형 데미지
        }

        public static string GetTileTypeName(TileType type)
        {
            string name = "";

            if (type == TileType.GROUND)
                name = "땅";
            else if (type == TileType.WALL)
                name = "벽";
            else if (type == TileType.MOUNTAIN)
                name = "산";

            return name;
        }

        public static bool IsCanMoveTile(TileType tileType)
        {
            if (tileType == TileType.GROUND || tileType == TileType.MOUNTAIN)
                return true;

            return false;
        }

        public static bool IsCanAtkTile(TileType tileType)
        {
            if (tileType == TileType.GROUND || tileType == TileType.MOUNTAIN)
                return true;

            return false;
        }
    }

}
