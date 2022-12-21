using DG.Tweening;
using GM;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Simulator
{
    public class FixedPanel : UIContents
    {
        public new Transform transform;

        public Transform parent;
        public Transform topPanel;
        public Transform bottomPanel;
        public Transform skillPanel;
        public Transform battleView;
        public Transform turnInfoPanel;
        public Transform gamePausePopup;

        public Button turnBtn;
        public Text turnBtnText;

        public Button skillBtn;
        public Text skillBtnText;

        public List<GameObject> dangerArea = new List<GameObject>();

        public void Init(Transform mainTransform, Battle bt, GameData data)
        {
            transform = mainTransform;
            battle = bt;
            gameData = data;

            parent = transform.parent;
            topPanel = transform.Find("TopPanel");
            bottomPanel = transform.Find("BottomPanel");
            skillPanel = transform.Find("SkillPanel");
            battleView = transform.Find("BattleView");
            turnInfoPanel = transform.Find("TurnInfoPanel");
            gamePausePopup = transform.Find("GamePausePopup");

            turnBtn = transform.Find("BottomPanel/ButtonGroup/TurnButton").gameObject.GetComponent<Button>();
            turnBtnText = turnBtn.transform.Find("Text").gameObject.GetComponent<Text>();

            skillBtn = transform.Find("BottomPanel/ButtonGroup/SkillButton").gameObject.GetComponent<Button>();
            skillBtnText = skillBtn.transform.Find("Text").gameObject.GetComponent<Text>();

            var autoBtn = bottomPanel.Find("ButtonGroup/AutoButton").gameObject.GetComponent<Button>();
            autoBtn.onClick.AddListener(ClickAutoBtn);

            var showAreaBtn = bottomPanel.Find("ButtonGroup/ShowAreaButton").gameObject.GetComponent<Button>();
            showAreaBtn.onClick.AddListener(ClickShowAreaBtn);

            var optionBtn = bottomPanel.Find("ButtonGroup/SettingsButton").gameObject.GetComponent<Button>();
            optionBtn.onClick.AddListener(() =>
            {
                gamePausePopup.gameObject.SetActive(true);
                isOpenPopup = true;
            });

            var backBtn = transform.Find("GamePausePopup/Image/Buttons/Button (2)").GetComponent<Button>();
            backBtn.onClick.AddListener(() => {
                gamePausePopup.gameObject.SetActive(false);
                isOpenPopup = false;
            });
        }

        public void ShowStatsPanel(Character character)
        {
            var statsPanel = topPanel.Find("StatsPanel");
            statsPanel.gameObject.SetActive(true);

            var characterImg = statsPanel.Find("CharacterImage").gameObject.GetComponent<Image>();
            characterImg.sprite = LuaAssets.LoadSprite($"illust/{character.Img}/s");

            statsPanel.Find("Stats/Lv/LvText").GetComponent<Text>().text = character.level.ToString();
            statsPanel.Find("Stats/Lv/ExpText").GetComponent<Text>().text = "";

            var nameText = statsPanel.Find("NameBar/Text").gameObject.GetComponent<Text>();
            nameText.text = CharacterManager.GetChracterName(character.dataId);
            var hpText = statsPanel.Find("Stats/Hp/StatText").gameObject.GetComponent<Text>();
            hpText.text = character.Hp.Value().ToString();
            var atkText = statsPanel.Find("Stats/Atk/StatText").gameObject.GetComponent<Text>();
            atkText.text = character.Atk.Value().ToString();
            var defText = statsPanel.Find("Stats/Def/StatText").gameObject.GetComponent<Text>();
            defText.text = character.Def.Value().ToString();
            var typeText = statsPanel.Find("Stats/Type/StatText").gameObject.GetComponent<Text>();
            typeText.text = CharacterManager.GetCharacterJobName(character.jobId);
            var moveRangeText = statsPanel.Find("Stats/MovRange/StatText").gameObject.GetComponent<Text>();
            moveRangeText.text = character.moveRange.ToString();
            var attackRangeText = statsPanel.Find("Stats/AtkRange/StatText").gameObject.GetComponent<Text>();
            attackRangeText.text = character.attackRange.ToString();
            var tileText = statsPanel.Find("TileBar/Text").gameObject.GetComponent<Text>();
            tileText.text = "타일 속성 : " + TileManager.GetTileTypeName(gameData.mapData[character.position].type);
        }

        public void HideStatsPanel()
        {
            var statsPanel = topPanel.Find("StatsPanel");
            statsPanel.gameObject.SetActive(false);
        }

        public void ShowVsPanel(Character character, Character enemy, bool isAllyTurn) // 적의 턴에 공격 할때는 적이 character가 되고 내가 enemy가 된다
        {
            var vsPanel = topPanel.Find("VSPanel");
            vsPanel.gameObject.SetActive(true);

            if (isAllyTurn == true)
            {
                var characterImg = vsPanel.Find("CharacterImage1").gameObject.GetComponent<Image>();
                characterImg.sprite = LuaAssets.LoadSprite($"illust/{character.Img}/s");
                var attackerNameText = vsPanel.Find("NameBar1/Text").gameObject.GetComponent<Text>(); // TODO: 이름 제대로 적용
                attackerNameText.text = CharacterManager.GetCharacterJobName(character.jobId);
                var attackerHpText = vsPanel.Find("Hp/StatText1").gameObject.GetComponent<Text>();
                attackerHpText.text = character.Hp.Value().ToString();
                var attackerAtkText = vsPanel.Find("Atk/StatText1").gameObject.GetComponent<Text>();
                attackerAtkText.text = character.Atk.Value().ToString();

                var enemyImg = vsPanel.Find("CharacterImage2").gameObject.GetComponent<Image>();
                enemyImg.sprite = LuaAssets.LoadSprite($"illust/{enemy.Img}/s");
                var targetNameText = vsPanel.Find("NameBar2/Text").gameObject.GetComponent<Text>();
                targetNameText.text = CharacterManager.GetCharacterJobName(enemy.jobId);
                var targetHpText = vsPanel.Find("Hp/StatText2").gameObject.GetComponent<Text>();
                targetHpText.text = enemy.Hp.Value().ToString();
                var targetAtkHp = vsPanel.Find("Atk/StatText2").gameObject.GetComponent<Text>();
                targetAtkHp.text = enemy.Atk.Value().ToString();

                var atkLeftBgImg = vsPanel.Find("Atk/LeftBg").gameObject.GetComponent<Image>();
                atkLeftBgImg.color = Color.red;
                var hpLeftBgImg = vsPanel.Find("Hp/LeftBg").gameObject.GetComponent<Image>();
                hpLeftBgImg.color = Color.red;
                var atkRightBgImg = vsPanel.Find("Atk/RightBg").gameObject.GetComponent<Image>();
                atkRightBgImg.color = Color.white;
                var hpRightBgImg = vsPanel.Find("Hp/RightBg").gameObject.GetComponent<Image>();
                hpRightBgImg.color = Color.white;

                var redBg = vsPanel.Find("RedBg").gameObject.GetComponent<RectTransform>();
                redBg.anchoredPosition = new Vector2(-Mathf.Abs(redBg.anchoredPosition.x), redBg.anchoredPosition.y);
            }
            else
            {
                var characterImg = vsPanel.Find("CharacterImage1").gameObject.GetComponent<Image>();
                characterImg.sprite = LuaAssets.LoadSprite($"illust/{enemy.Img}/s");
                var attackerNameText = vsPanel.Find("NameBar1/Text").gameObject.GetComponent<Text>();
                attackerNameText.text = CharacterManager.GetCharacterJobName(enemy.jobId);
                var attackerHpText = vsPanel.Find("Hp/StatText1").gameObject.GetComponent<Text>();
                attackerHpText.text = enemy.Hp.Value().ToString();
                var attackerAtkText = vsPanel.Find("Atk/StatText1").gameObject.GetComponent<Text>();
                attackerAtkText.text = enemy.Atk.Value().ToString();

                var enemyImg = vsPanel.Find("CharacterImage2").gameObject.GetComponent<Image>();
                enemyImg.sprite = LuaAssets.LoadSprite($"illust/{character.Img}/s");
                var targetNameText = vsPanel.Find("NameBar2/Text").gameObject.GetComponent<Text>();
                targetNameText.text = CharacterManager.GetCharacterJobName(character.jobId);
                var targetHpText = vsPanel.Find("Hp/StatText2").gameObject.GetComponent<Text>();
                targetHpText.text = character.Hp.Value().ToString();
                var targetAtkHp = vsPanel.Find("Atk/StatText2").gameObject.GetComponent<Text>();
                targetAtkHp.text = character.Atk.Value().ToString();

                var atkLeftBgImg = vsPanel.Find("Atk/LeftBg").gameObject.GetComponent<Image>();
                atkLeftBgImg.color = Color.white;
                var hpLeftBgImg = vsPanel.Find("Hp/LeftBg").gameObject.GetComponent<Image>();
                hpLeftBgImg.color = Color.white;
                var atkRightBgImg = vsPanel.Find("Atk/RightBg").gameObject.GetComponent<Image>();
                atkRightBgImg.color = Color.red;
                var hpRightBgImg = vsPanel.Find("Hp/RightBg").gameObject.GetComponent<Image>();
                hpRightBgImg.color = Color.red;

                var redBg = vsPanel.Find("RedBg").gameObject.GetComponent<RectTransform>();
                redBg.anchoredPosition = new Vector2(Mathf.Abs(redBg.anchoredPosition.x), redBg.anchoredPosition.y);
            }
        }

        public void HideVsPanel()
        {
            var vsPanel = topPanel.Find("VSPanel");
            vsPanel.gameObject.SetActive(false);
        }

        public void ShowSkillPanel(Character character)
        {
            transform.Find("SkillPanel").gameObject.SetActive(true);

            if (character.skills != null)
            {
                Image image;
                for (int i = 0; i < character.skills.Count; i++)
                {
                    var skill = character.skills[i];
                    var child = skillPanel.GetChild(i);
                    image = child.gameObject.GetComponent<Image>();
                    image.sprite = LuaAssets.LoadSprite(skill.icon);

                    child.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    child.gameObject.GetComponent<Button>().onClick.AddListener(() => ClickSkillBtn(character, skill)); // 버튼 이벤트
                    child.GetChild(0).GetComponent<Text>().text = L.Get("SKILL_NAME." + character.skills[i].dataId); // 버튼 이름
                }
            }
        }

        public void ClickSkillBtn(Character character, Skill skill)
        {
            Debug.Log("스킬 버튼을 클릭하였다");
            if (skill.type == "passive")
            {
                Debug.Log("패시브 스킬은 영역을 보여주지 않습니다.");
                return;
            }

            int position = character.tmpPosition != -1 ? character.tmpPosition : character.position;
            battle.SelectedSkill = skill;
            battle.SkillPosition = position;
            turnBtn.gameObject.SetActive(true);
            turnBtnText.text = "취소";
            turnBtn.onClick.RemoveAllListeners();
            turnBtn.onClick.AddListener(() =>
            {
                ClickSkillCancelBtn(character, position);
                turnBtn.gameObject.SetActive(false);
            });
            HideSkillPanel();
            Simulator.simulator.RemoveCandi(character);

            if (skill.targetType == "self")
            {
                Debug.Log("self로 스킬 시전한다.");
                skillBtn.gameObject.SetActive(true);
                skill.UseBuff(character, skill.skillParams[0]);
                return;
            }

            SkillParam skillParam;
            if (skill.skillParams.Count > 1) // 스킬 파라미터 2개 이상인 경우 (ex 스턴 + 공격) 두번째 파라미터 가져온다.
                skillParam = skill.skillParams[1];
            else
                skillParam = skill.skillParams[0];

            if (skillParam.range == 0)
                Simulator.simulator.ShowSingleSkillRange(character, position, skill.targetType, skill.targetRange); // 단일인 경우 target_range로 처리
            else
                Simulator.simulator.ShowSkillAreaRange(character, position, skill.targetType, skillParam.range); // 광역은 range_로 처리
        }

        public void ClickSkillCancelBtn(Character character, int position)
        {
            Debug.Log("스킬 취소 클릭");
            battle.SelectedSkill = null;
            skillBtn.gameObject.SetActive(false);
            Simulator.simulator.HideSkillRange(character);
            Simulator.simulator.HidePath(character, position);
            Simulator.SetAnimation(character.go, "walk");
            battle.ClickedCharacterIdx = character.position;
            Simulator.simulator.FindPath(character.position, character.go);
            ShowSkillPanel(character);
        }

        public void HideSkillPanel()
        {
            skillPanel.gameObject.SetActive(false);
        }

        public void ShowBattleView()
        {
            battleView.gameObject.SetActive(true);
            parent.Find("GamePanel").gameObject.SetActive(false);
        }

        public void HideBattleView()
        {
            battleView.gameObject.SetActive(false);
            parent.Find("GamePanel").gameObject.SetActive(true);
        }

        public void ClickAutoBtn()
        {
            if (battle.turnState == TurnState.ENEMY)
                return;

            isClickAutoBtn = !isClickAutoBtn;

            var btn = bottomPanel.Find("ButtonGroup/AutoButton/Active");
            btn.gameObject.SetActive(isClickAutoBtn);

            if (isClickAutoBtn && battle.turnState == TurnState.ALLY)
            {
                Debug.Log("자동 전투 ON");
                battle.isAutoPlay = true;
            }
            else
            {
                Debug.Log("자동 전투 OFF");
                battle.isAutoPlay = false;
            }
        }

        public void ClickShowAreaBtn()
        {
            if (isClickAutoBtn == true || battle.ClickedCharacterIdx != -1)
                return;

            var btn = bottomPanel.Find("ButtonGroup/ShowAreaButton/Active");
            var active = btn.gameObject.activeSelf;
            if (active)
            {
                isClickShowAreaBtn = false;
                HideEnemyArea();
            }
            else
            {
                isClickShowAreaBtn = true;
                FindEnemyArea(battle.enemies);
            }
            btn.gameObject.SetActive(!active);
        }

        public void FindEnemyArea(List<Character> enemys)
        {
            var gamePanel = parent.Find("GamePanel");
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            var mapTilePadding = GameConsts.mapTilePadding;
            var startPosX = GameConsts.startPosX;
            var startPosY = GameConsts.startPosY;

            var dangerList = new List<int>();
            foreach (var enemy in enemys)
            {
                var childList = GetDangerAreaPosition(enemy);
                foreach (var child in childList)
                {
                    if (!dangerList.Contains(child))
                    {
                        dangerList.Add(child);
                    }
                }
            }

            foreach (var index in dangerList)
            {
                var attackAreaObject = parent.Find("GamePanel/AttackArea").gameObject;
                var newPosition = new Vector2(startPosX + mapTilePadding * (index % mapWidth), startPosY - mapTilePadding * (index / mapWidth));

                var attackArea = Simulator.InstantiateObject(attackAreaObject, gamePanel, newPosition);
                attackArea.transform.GetChild(0).gameObject.SetActive(false);
                dangerArea.Add(attackArea);
            }
        }

        public List<int> GetDangerAreaPosition(Character enemy)
        {
            var mapWidth = gameData.mapWidth;
            var result = new List<int>();
            var list = new List<int>();
            if (IsMoveable(enemy.position, enemy.position + 1))
            {
                list.Add(enemy.position + 1);
                result.Add(enemy.position + 1);
            }

            if (IsMoveable(enemy.position, enemy.position - 1))
            {
                list.Add(enemy.position - 1);
                result.Add(enemy.position - 1);
            }

            if (IsMoveable(enemy.position, enemy.position + (1 * mapWidth)))
            {
                list.Add(enemy.position + (1 * mapWidth));
                result.Add(enemy.position + (1 * mapWidth));
            }

            if (IsMoveable(enemy.position, enemy.position - (1 * mapWidth)))
            {
                list.Add(enemy.position - (1 * mapWidth));
                result.Add(enemy.position - (1 * mapWidth));
            }

            var i = 1;
            while (true)
            {
                if (i == enemy.moveRange + enemy.attackRange)
                {
                    break;
                }
                    
                var childList = new List<int>();
                foreach (var index in list)
                {
                    if (IsMoveable(index, index + 1))
                    {
                        childList.Add(index + 1);
                        result.Add(index + 1);
                    }

                    if (IsMoveable(index, index - 1))
                    {
                        childList.Add(index - 1);
                        result.Add(index - 1);
                    }

                    if (IsMoveable(index, index + mapWidth))
                    {
                        childList.Add(index + mapWidth);
                        result.Add(index + mapWidth);
                    }

                    if (IsMoveable(index, index - mapWidth))
                    {
                        childList.Add(index - mapWidth);
                        result.Add(index - mapWidth);
                    }
                }
                list = childList;
                i++;
            }

            return result;
        }

        public bool IsMoveable(int position, int nextPosition)
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;

            if (position - nextPosition == -1) // 오른쪽
            {
                if ((position + 1) % mapWidth == 0) // 가장 오른쪽이면 리턴 
                {
                    return false;
                }
            }

            else if (position - nextPosition == 1) // 왼쪽
            {
                if (position % mapWidth == 0) // 가장 왼쪽이면 리턴
                {
                    return false;
                }
            }

            else if (nextPosition - position == -mapWidth) // 아래쪽
            {
                if (position / mapWidth == mapHeight) // 가장 아래쪽이면 리턴
                {
                    return false;
                }
            }

            else if (position - nextPosition == mapWidth) // 위쪽
            {
                if (position / mapWidth == 0) // 가장 위쪽이면 리턴
                { 
                    return false;
                }
            }

            if (nextPosition < 0 || nextPosition > mapWidth * mapHeight - 1)
                return false;

            TileType tileType = gameData.mapData[nextPosition].type;
            bool hasCharacter = CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, nextPosition);
            bool isCanMoveTile = TileManager.IsCanMoveTile(tileType);

            if (!hasCharacter && isCanMoveTile)
                return true;

            return false;
        }

        public void HideEnemyArea()
        {
            foreach (var list in dangerArea)
                Simulator.DestroyObject(list);
        }

        public void ShowTurnInfoPanel(string text)
        {
            turnInfoPanel.gameObject.SetActive(true);
            turnInfoPanel.Find("Image/Text").gameObject.GetComponent<Text>().text = text;
            turnInfoPanel.DOMoveY(turnInfoPanel.position.y + 100, 2).onComplete = () =>
            {
                turnInfoPanel.position = new Vector2(turnInfoPanel.position.x, turnInfoPanel.position.y - 100);
                turnInfoPanel.gameObject.SetActive(false);
            };
        }
    }
}
