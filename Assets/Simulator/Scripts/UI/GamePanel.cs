using UnityEngine;
using System.Collections;
using GM;
using UnityEngine.UI;
using Spine.Unity;

namespace Simulator
{
    public class GamePanel : UIContents
    {
        public new Transform transform;
        private static readonly Battle mainBattle;

        public void Init(Transform mainTransform, Battle bt, GameData data)
        {
            transform = mainTransform;
            battle = bt;
            gameData = data;
            SetMap();
            SetCharacter();
        }

        public void SetMap()
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            var mapTilePadding = GameConsts.mapTilePadding;
            var tileList = gameData.tileList;

            var groundObject = transform.Find("Background").gameObject;
            var wallObject = transform.Find("Wall").gameObject;
            var mountainObject = transform.Find("Mountain").gameObject;
            var startPosX = GameConsts.startPosX;
            var startPosY = GameConsts.startPosY;

            var index = 0;
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    var tileData = TileManager.GetTileData(tileList[index]);
                    var tile = new Tile((TileType)tileList[index], tileData);
                    if (tile.type == TileType.GROUND)
                        tile.go = Simulator.InstantiateObject(groundObject, transform, new Vector2(startPosX + mapTilePadding * j, startPosY - mapTilePadding * i));
                    else if (tile.type == TileType.WALL)
                        tile.go = Simulator.InstantiateObject(wallObject, transform, new Vector2(startPosX + mapTilePadding * j, startPosY - mapTilePadding * i));
                    else if (tile.type == TileType.MOUNTAIN)
                        tile.go = Simulator.InstantiateObject(mountainObject, transform, new Vector2(startPosX + mapTilePadding * j, startPosY - mapTilePadding * i));
                    else
                        Debug.Log("타일 타입이 없습니다" + index);

                    if (LuaMain.Instance.LogDebugger)
                        tile.go.transform.GetChild(0).GetComponent<Text>().text = index.ToString();
                    else
                        tile.go.transform.GetChild(0).gameObject.SetActive(false);

                    tile.go.GetComponent<Image>().enabled = false;

                    Simulator.AddEventTrigger(tile.go, "tile", index);
                    gameData.mapData.Add(tile);
                    index++;
                }
            }
        }

        public void SetCharacter()
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            var mapTilePadding = GameConsts.mapTilePadding;
            var mapData = gameData.mapData;
            var startPosX = GameConsts.startPosX;
            var startPosY = GameConsts.startPosY;

            var allyList = StringUtil.StringToList(gameData.allyData);
            var enemyList = StringUtil.StringToList(gameData.enemyData);
            if (allyList == null || enemyList == null)
            {
                Debug.Log("캐릭터 혹은 적 데이터가 존재하지 않습니다.");
                return;
            }

            GameObject go;
            var index = 0;
            var allyObject = transform.Find("Character").gameObject;
            var enemyObject = transform.Find("Enemy").gameObject;

            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (allyList[index] != 0)
                    {
                        var statData = CharacterManager.GetCharacterStatData(allyList[index]);
                        if (statData != null)
                        {
                            GameObject spineData;
                            if (1 < statData.img && statData.img < 10) // TODO: 스파인 파일명 이슈
                                spineData = LuaAssets.Instantiate("test0" + statData.img + "_UI");
                            else if (statData.img == 18) // 이올린 임시 하드코딩 
                                spineData = LuaAssets.Instantiate("test01_UI");
                            else
                                spineData = LuaAssets.Instantiate("test" + statData.img + "_UI");

                            if (spineData != null)
                            {
                                SkeletonGraphic sg = spineData.transform.GetChild(0).GetComponent<SkeletonGraphic>(); // 서버에서 내려준 프리팹에는 스파인 런타임이 없어 제대로 동작시키기 위해 다시 set
                                sg.material = sg.defaultMaterial;
                                go = Simulator.InstantiateObject(spineData.gameObject, transform, new Vector2(startPosX + mapTilePadding * j, startPosY - mapTilePadding * i));
                                Simulator.DestroyObject(spineData);
                            }
                            else
                            {
                                var img = allyObject.GetComponent<Image>();
                                Sprite sprite = LuaAssets.LoadSprite($"character/{statData.img}");
                                if (sprite)
                                    img.sprite = sprite;

                                go = Simulator.InstantiateObject(allyObject, transform, new Vector2(startPosX + mapTilePadding * j, startPosY - mapTilePadding * i));
                            }

                            var ally = new Character(go, allyList[index], index, CharacterKey.ALLY);
                            var hpImage = go.transform.Find("HpBar/Image").GetComponent<Image>();
                            hpImage.fillAmount = ally.Hp.Ratio;

                            battle.allies.Add(ally);
                            Simulator.AddEventTrigger(go, "character", index);
                        }
                        else
                        {
                            Debug.Log("내려온 아군 캐릭터 데이터가 없습니다");
                            allyList[index] = 0;
                        }
                    }
                    else if (enemyList[index] != 0)
                    {
                        var statData = CharacterManager.GetCharacterStatData(enemyList[index]);
                        if (statData != null)
                        {
                            var spineData = LuaAssets.Instantiate("test" + statData.img + "_UI");
                            if (spineData != null)
                            {
                                SkeletonGraphic sg = spineData.transform.GetChild(0).GetComponent<SkeletonGraphic>(); // 서버에서 내려준 프리팹에는 스파인 런타임이 없어 제대로 동작시키기 위해 다시 set
                                sg.material = sg.defaultMaterial;
                                go = Simulator.InstantiateObject(spineData.gameObject, transform, new Vector2(startPosX + mapTilePadding * j, startPosY - mapTilePadding * i));
                                Simulator.DestroyObject(spineData);
                            }
                            else
                            {
                                var img = enemyObject.GetComponent<Image>();
                                Sprite sprite = LuaAssets.LoadSprite($"character/{statData.img}");
                                if (sprite)
                                    img.sprite = sprite;

                                go = Simulator.InstantiateObject(enemyObject, transform, new Vector2(startPosX + mapTilePadding * j, startPosY - mapTilePadding * i));
                            }

                            var enemy = new Character(go, enemyList[index], index, CharacterKey.ENEMY);
                            var hpImage = go.transform.Find("HpBar/Image").GetComponent<Image>();
                            hpImage.fillAmount = enemy.Hp.Ratio;

                            battle.enemies.Add(enemy);
                            Simulator.AddEventTrigger(go, "character", index);
                        }
                        else
                        {
                            Debug.Log("내려온 적군 캐릭터 데이터가 없습니다");
                            enemyList[index] = 0;
                        }
                    }
                    index++;
                }
            }

            battle.allyCount = battle.allies.Count;
            battle.enemyCount = battle.enemies.Count;
        }
    }
}
