using System.Collections.Generic;
using UnityEngine;

namespace Simulator
{
    public class GameData
    {
        public int mapWidth;
        public int mapHeight;
        public List<Tile> mapData = new List<Tile>();

        public string tileData;
        public List<int> tileList = new List<int>();
        public string tileSetData;
        public int mapTilePadding = GameConsts.mapTilePadding;
        public string characterStatData;

        public string allyData;
        public string enemyData;
        public int maxTurn;

        public void InitData()
        {
            tileList = StringUtil.StringToList(tileData);
            CharacterManager.characterStatDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, CharacterStatData>>(characterStatData);
            TileManager.tileSet = JsonUtility.FromJson<TileSet>(tileSetData);
        }
    }
}
