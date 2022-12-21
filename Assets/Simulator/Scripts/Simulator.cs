using DG.Tweening;
using GM;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Simulator
{
    public sealed class Simulator : MonoBehaviour
    {
        public static Simulator simulator;
        public CameraController cameraController;
        public UIContents uiContents;
        public Battle battle;
        public GameData gameData;
        public Coroutine coroutine;
        public bool isCompletedLoadData = false; // 데이터가 로드 되었는지 확인하기 위한 임시 플래그
        public static ITouchState touchState { get; set; }
        public static void SetTouchState(ITouchState state)
        {
            touchState = state;
            touchState.Simul = simulator;
        }

        private void Awake()
        {
            if (simulator)
                return;

            simulator = this;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1); // 루아에서 에셋 번들, 데이터 등의 세팅 완료 보장을 위해 의도적으로 1초 대기한다.

            cameraController = new CameraController(transform.Find("GamePanel"));
            battle = new Battle(gameData.maxTurn);
            gameData.InitData(); // 루아에서 데이터를 가져와 Init한다. 
            uiContents = new UIContents(transform, battle, gameData);
            StartAllyTurn();
            isCompletedLoadData = true;
            yield break;
        }

        private void Update()
        {
            if (coroutine != null || isCompletedLoadData == false) // 코루틴의 중복 수행을 방지합니다.
                return;

            if (battle.isAutoPlay == true && coroutine == null) // 자동 전투 버튼이 클릭되면 코루틴을 시작하고 필드에 레퍼런스를 보관합니다.
            {
                Debug.Log("자동 전투 시작");
                coroutine = StartCoroutine(CO_PlayAllyTurn());
            }
                
            else if (coroutine != null) // 자동 전투 버튼이 꺼지면 코루틴을 중단하고 필드를 비웁니다.
            {
                Debug.Log("자동 전투 중지");
                StopCoroutine(CO_PlayAllyTurn());
                coroutine = null;
            }
        }

        private void LateUpdate()
        {
            if (uiContents == null)
                return;
            if (uiContents.isOpenPopup == false)
                cameraController.Update();
        }

        public void StartAllyTurn()
        {
            if (battle.enemyCount <= 0 || battle.allyCount <= 0)
            {
                Debug.Log("아군 혹은 적군이 모두 죽어 게임 종료 처리합니다.");
                LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
                return;
            }

            Debug.Log("아군의 턴입니다.");

            uiContents.fixedPanel.ShowTurnInfoPanel("My Phase");
            battle.turnState = TurnState.ALLY;
            battle.turnCount = battle.allyCount;

            foreach (var enemy in battle.enemies)
                ChangeImageAlpha(enemy.go, 1);

            foreach (var ally in battle.allies)
            {
                if (ally.state != CharacterState.DEAD)
                    ally.state = CharacterState.TURN_START;
            }

            if (coroutine != null)
            {
                Debug.Log("진행중이던 적군의 턴 코루틴을 정리합니다.");
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public void StartEnemyTurn()
        {

            if (battle.enemyCount <= 0 || battle.allyCount <= 0)
            {
                Debug.Log("아군 혹은 적군이 모두 죽어 게임 종료 처리합니다.");
                LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
                return;
            }

            Debug.Log("적군의 턴입니다.");

            uiContents.fixedPanel.ShowTurnInfoPanel("Enemy Phase");
            battle.turnState = TurnState.ENEMY;
            battle.turnCount = battle.enemyCount;

            foreach (var ally in battle.allies)
                ChangeImageAlpha(ally.go, 1);

            foreach (var enemy in battle.enemies)
            {
                if (enemy.state != CharacterState.DEAD)
                    enemy.state = CharacterState.TURN_START;
            }

            if (coroutine != null)
            {
                Debug.Log("진행중이던 코루틴이 있어 중지하고 진행합니다.");
                StopCoroutine(coroutine);
            }

            coroutine = StartCoroutine(CO_PlayEnemyTurn());
        }

        public IEnumerator CO_PlayAllyTurn()
        {
            yield return null; // 의도적으로 한 프레임 대기
            
            if (battle.isAutoPlay == false || battle.turnState == TurnState.ENEMY)
            {
                Debug.Log("오토 플레이가 꺼져있거나 적군의 턴이라 자동 전투를 진행하지 않습니다.");
                yield break;
            }
                
            Debug.Log("자동 전투를 시작합니다.");

            // 턴 진행중이던 캐릭터 먼저 처리
            var turnCharater = BattleManager.GetInTurnCharacter(battle.allies);
            if (turnCharater != null)
            {
                if (battle.isAutoPlay)
                {
                    Debug.Log("턴 진행중이던 아군 먼저 턴을 진행합니다.");
                    yield return CoPlayCharacterTurn(turnCharater);
                }
                else
                    yield break;

                if (battle.enemyCount <= 0 || battle.allyCount <= 0)
                {
                    Debug.Log("아군 혹은 적군이 모두 죽어 게임 종료 처리합니다.");
                    LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
                    yield break;
                }
            }

            foreach (var character in battle.allies)
            {
                if (character.state == CharacterState.DEAD || character.state == CharacterState.TURN_END)
                    continue;

                if (battle.enemyCount <= 0 || battle.allyCount <= 0)
                {
                    Debug.Log("아군 혹은 적군이 모두 죽어 게임 종료 처리합니다.");
                    LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
                    yield break;
                }

                if (battle.isAutoPlay)
                {
                    Debug.Log("포지션 " + character.position + " 캐릭터의 턴을 자동으로 진행합니다.");
                    yield return CoPlayCharacterTurn(character);
                }
                else
                    yield break;
            }

            if (battle.enemyCount <= 0 || battle.allyCount <= 0)
            {
                Debug.Log("아군 혹은 적군이 모두 죽어 게임 종료 처리합니다.");
                LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
                yield break;
            }

            yield return new WaitForSeconds(2.0f);
            StartEnemyTurn();
        }

        private IEnumerator CO_PlayEnemyTurn()
        {
            Debug.Log("적군의 행동을 수행합니다.");
            yield return new WaitForSeconds(2.0f);

            if (battle.allyCount <= 0 || battle.enemyCount <= 0)
            {
                LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
                yield break;
            }

            foreach (var enemy in battle.enemies)
            {
                if (enemy.state == CharacterState.DEAD || enemy.state == CharacterState.TURN_END)
                    continue;

                yield return CoPlayCharacterTurn(enemy);
            }
            Debug.Log("적군의 모든 턴 종료");

            if (battle.allyCount <= 0 || battle.enemyCount <= 0)
            {
                LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
                yield break;
            }

            yield return new WaitForSeconds(2.0f);
            StartAllyTurn();
        }


        public IEnumerator CoPlayCharacterTurn(Character character)
        {
            cameraController.MoveToCharacter(character.go.transform.GetComponent<RectTransform>().anchoredPosition); // 카메라 위치 이동

            bool isAlly = character.key == CharacterKey.ALLY ? true : false;
            var tmpString = isAlly ? "아군" : "적군";
            Debug.Log(tmpString + character.position + " 의 턴을 수행합니다.");

            GameObject go;
            int position;
            int minPosition = -1;

            if (character.state == CharacterState.TURN_START)
            {
                if (battle.isAutoPlay == false && isAlly)
                {
                    coroutine = null;
                    Debug.Log("오토 중지");
                    yield break;
                }
                
                uiContents.fixedPanel.ShowStatsPanel(character);
                uiContents.fixedPanel.ShowSkillPanel(character);
                SetAnimation(character.go, "walk");
                FindPath(character.position, character.go);

                yield return new WaitForSeconds(1.0f);

                character.state = CharacterState.SELF_CLICK;
                uiContents.fixedPanel.HideStatsPanel();
                uiContents.fixedPanel.HideSkillPanel();
                SetAnimation(character.go, "idle");

                if (battle.isAutoPlay == false && isAlly)
                {
                    coroutine = null;
                    Debug.Log("오토 중지");
                    yield break;
                }
            }

            if (character.state == CharacterState.SELF_CLICK)
            {
                if (character.attackCandi.Count > 0) // 공격할 수 있는 위치 먼저 검사, attackCandi라고 해서 무조건 공격할 수 없으므로 리스트에서 빼면서 검사해본다.
                {
                    var enemyList = character.key == CharacterKey.ALLY ? battle.enemies : battle.allies;
                    var attackList = CharacterManager.GetAttackableEnemyList(enemyList, character);
                    if (attackList.Count > 0) // 공격할 적이 있는 경우
                    {
                        character.state = CharacterState.ATTACK_CLICK;
                        position = GetNearestAttackEnemyPosition(attackList, isAlly, character.position);
                        character.tmpPosition = position;
                    }
                    else // 공격할 적이 없는 경우
                    {
                        if (battle.isAutoPlay == false && isAlly)
                        {
                            coroutine = null;
                            Debug.Log("오토 중지");
                            yield break;
                        }

                        var moveList = CharacterManager.GetMoveableTileList(character);
                        position = GetNearestMoveEnemyPosition(moveList, isAlly, character.position);
                        ShowPath(character, position);
                        character.state = CharacterState.MOVE_CLICK;
                        character.tmpPosition = position;

                        yield return new WaitForSeconds(1.0f);

                        if (battle.isAutoPlay == false && isAlly)
                        {
                            coroutine = null;
                            Debug.Log("오토 중지");
                            yield break;
                        }
                    }
                }

                else
                {
                    if (battle.isAutoPlay == false && isAlly)
                    {
                        coroutine = null;
                        Debug.Log("오토 중지");
                        yield break;
                    }
                    var moveList = CharacterManager.GetMoveableTileList(character);
                    position = GetNearestMoveEnemyPosition(moveList, isAlly, character.position);
                    ShowPath(character, position);
                    character.state = CharacterState.MOVE_CLICK;
                    character.tmpPosition = position;
                    
                    yield return new WaitForSeconds(1.0f);

                    if (battle.isAutoPlay == false && isAlly)
                    {
                        coroutine = null;
                        Debug.Log("오토 중지");
                        yield break;
                    }
                }
            }

            if (character.state == CharacterState.MOVE_CLICK)
            {
                character.moveCandi.TryGetValue(character.tmpPosition, out go);
                MoveCharacter(character, go, character.tmpPosition);
                HidePath(character, character.tmpPosition);
                RemoveCandi(character);

                RequestCharacterMove(character.objectId, character.position, character.Hp.Value());
            }
            else if (character.state == CharacterState.ATTACK_CLICK)
            {   
                var enemy = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, character.tmpPosition);
                // 제자리에서 공격할 수 있는 경우
                if (CheckAttackable(character.position, character.attackRange, character.tmpPosition, character.key) == true)
                {
                    character.state = CharacterState.ATTACK_CLICK;
                    HidePath(character, character.tmpPosition);
                    RemoveCandi(character);
                }
                else // 이동해야만 공격할 수 있는 경우
                {
                    if (battle.isAutoPlay == false && isAlly)
                    {
                        coroutine = null;
                        Debug.Log("오토 중지");
                        yield break;
                    }

                    character.state = CharacterState.MOVE_CLICK;
                    minPosition = GetShortestPositionInMoveList(character, character.tmpPosition);
                    ShowPath(character, minPosition);
                    yield return new WaitForSeconds(1.0f);

                    character.attackCandi.TryGetValue(character.tmpPosition, out go);
                    HidePath(character, minPosition);
                    MoveCharacter(character, go, minPosition, true);
                    RemoveCandi(character);

                    if (battle.isAutoPlay == false && isAlly)
                    {
                        coroutine = null;
                        Debug.Log("오토 중지");
                        yield break;
                    }
                }

                yield return WaitForSpineAnimationComplete(character.go, "attack");
                ShowVFX("Hit", transform.Find("GamePanel"), enemy.go.GetComponent<RectTransform>().anchoredPosition);
                yield return new WaitForSeconds(1.0f);

                uiContents.fixedPanel.ShowVsPanel(character ,enemy, isAlly);
                uiContents.fixedPanel.ShowBattleView();

                if (battle.isAutoPlay == false && isAlly)
                {
                    coroutine = null;
                    Debug.Log("오토 중지");
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);

                uiContents.fixedPanel.HideVsPanel();
                uiContents.fixedPanel.HideBattleView();

                if (battle.isAutoPlay == false && isAlly)
                {
                    coroutine = null;
                    Debug.Log("오토 중지");
                    yield break;
                }

                float damageRate = 
                    TileManager.CalcDamageRate(gameData.mapData[character.position], gameData.mapData[enemy.position]) 
                    * BattleManager.CalcDamageRate(character.jobId, enemy.jobId);

                Character deadCharacter;
                if (BattleManager.InCounterAttackRange(character, enemy))
                    deadCharacter = BattleManager.GiveDamage(character, enemy, damageRate, true);
                else
                    deadCharacter = BattleManager.GiveDamage(character, enemy, damageRate, false);

                if (deadCharacter != null)
                {
                    yield return WaitForSpineAnimationComplete(deadCharacter.go, "dead");
                    DeadCharacterCallback(deadCharacter);
                }

                if (character.Hp.Value() <= 0)
                {
                    yield return WaitForSpineAnimationComplete(character.go, "dead");
                    DeadCharacterCallback(character);
                }

                RequestCharacterAttack(character.objectId, character.position, character.Hp.Value(), enemy.objectId, enemy.Hp.Value());
            }

            if (character.state != CharacterState.DEAD)
            {
                character.state = CharacterState.TURN_END;
                SetAnimation(character.go, "idle");
                ChangeImageAlpha(character.go, 0.7f);
            }

            battle.turnCount--;

            Debug.Log(tmpString + character.position + " 의 턴이 끝났습니다.");
            yield return null;
        }

        public void StartSimulation()
        {
            Debug.Log("시뮬레이션을 시작합니다.");
            for (int i = 0; i < gameData.maxTurn; i++)
            {
                Debug.Log((i + 1) + "번째 턴 시작");
                if (battle.allyCount == 0)
                {
                    Debug.Log("우리팀 패배");
                    return;
                }
                if (battle.enemyCount == 0)
                {
                    Debug.Log("적팀 패배");
                    return;
                }

                Debug.Log("아군의 " + (i + 1) + "턴 시작");
                foreach (var enemy in battle.enemies)
                    ChangeImageAlpha(enemy.go, 1);

                foreach (var ally in battle.allies)
                {
                    if (ally.state == CharacterState.DEAD)
                        continue;

                    ally.state = CharacterState.TURN_START;
                    Play(ally);
                }

                Debug.Log("적군의 " + (i + 1) + "턴 시작");

                foreach (var ally in battle.allies)
                    ChangeImageAlpha(ally.go, 1);

                foreach (var enemy in battle.enemies)
                {
                    if (enemy.state == CharacterState.DEAD)
                        continue;

                    enemy.state = CharacterState.TURN_START;
                    Play(enemy);
                }
            }
            Debug.Log("제한턴 : " + gameData.maxTurn + "안에 게임이 끝나지 않았습니다.");
            LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
        }

        private void Play(Character character) // 순차적으로 캐릭터의 한턴을 수행 (1: 공격할 적이 있으면 랜덤으로 공격 2: 없으면 랜덤으로 이동)
        {
            int position;
            GameObject go;

            if (character.state == CharacterState.TURN_END)
                return;

            character.state = CharacterState.SELF_CLICK;
            FindPath(character.position, character.go);

            int minPosition = -1;
            bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;

            if (character.attackCandi.Count > 0) // 공격할 수 있는 위치 먼저 검사, attackCandi라고 해서 무조건 공격할 수 없으므로 리스트에서 빼면서 검사해본다.
            {
                var enemyList = isMyTeam ? battle.enemies : battle.allies;
                var attackList = CharacterManager.GetAttackableEnemyList(enemyList, character);
                if (attackList.Count > 0) // 공격할 적이 있는 경우
                {
                    position = GetNearestAttackEnemyPosition(attackList, isMyTeam, character.position);
                    character.attackCandi.TryGetValue(position, out go);

                    var enemy = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, position);

                    // 제자리에서 공격할 수 있는 경우
                    if (CheckAttackable(character.position, character.attackRange, position, character.key) == true)
                    {
                        HidePath(character, character.tmpPosition);
                        RemoveCandi(character);
                    }
                    else // 이동해야만 공격할 수 있는 경우
                    {
                        minPosition = GetShortestPositionInMoveList(character, position);
                        ShowPath(character, minPosition);
                        HidePath(character, minPosition);
                        MoveCharacter(character, go, minPosition, true);
                        RemoveCandi(character);
                    }

                    float damageRate = 
                        TileManager.CalcDamageRate(gameData.mapData[character.position], gameData.mapData[enemy.position]) 
                        * BattleManager.CalcDamageRate(character.jobId, enemy.jobId);

                    Character deadCharacter;
                    if (BattleManager.InCounterAttackRange(character, enemy))
                        deadCharacter = BattleManager.GiveDamage(character, enemy, damageRate, true);
                    else
                        deadCharacter = BattleManager.GiveDamage(character, enemy, damageRate, false);

                    if (deadCharacter != null)
                        DeadCharacterCallback(deadCharacter);

                    if (character.Hp.Value() <= 0)
                        DeadCharacterCallback(character);
                }

                else // 공격할 적이 없는 경우
                {
                    var moveList = CharacterManager.GetMoveableTileList(character);
                    position = GetNearestMoveEnemyPosition(moveList, isMyTeam, character.position);
                    character.moveCandi.TryGetValue(position, out go);
                    HidePath(character, character.tmpPosition);
                    MoveCharacter(character, go, position);
                    RemoveCandi(character);
                }
            }
            else
            {
                character.state = CharacterState.MOVE_CLICK;
            }
            if (character.state != CharacterState.DEAD)
            {
                character.state = CharacterState.TURN_END;
                ChangeImageAlpha(character.go, 0.7f);
            }
        }

        public static void AddEventTrigger(GameObject go, string type, int position = -1)
        {
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data, go, type, position); });
            trigger.triggers.Add(entry);
        }

        public static void RemoveEventTrigger(GameObject go)
        {
            if (go == null)
                return;

            EventTrigger trigger = go.GetComponent<EventTrigger>();
            if (trigger.triggers.Count > 0)
                trigger.triggers.Remove(trigger.triggers[0]);
        }

        public static void ChangeEventTrigger(GameObject go, string type, int position)
        {
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            var prevTrigger = trigger.triggers[0];
            prevTrigger.callback.RemoveAllListeners();
            prevTrigger.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data, go, type, position); });
        }

        private static void OnPointerDownDelegate(PointerEventData data, GameObject go, string type, int position)
        {
            // 자동 전투 중이거나 위험 영역 표시중이거나 적의 턴일 때에는 이벤트를 붙이지 않는다.
            if (simulator.uiContents.isClickAutoBtn || simulator.uiContents.isClickShowAreaBtn || simulator.battle.turnState == TurnState.ENEMY)
                return;

            if (type == "character")
                SetTouchState(new TouchCharacter());

            else if (type == "tile")
                SetTouchState(new TouchTile());

            else if (type == "enemy")
                SetTouchState(new TouchEnemy());

            else if (type == "skill_area")
                SetTouchState(new TouchSkillArea());

            else if (type == "skill_buff")
                SetTouchState(new TouchSkillBuff());

            else if (type == "skill_attack")
                SetTouchState(new TouchSkillAttack());

            touchState.TouchObject(go, position);
        }

        public void FindPath(int position, GameObject go)
        {
            Character character = CharacterManager.GetCharacterByClicked(battle.allies, battle.enemies);
            if (character == null)
                character = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, position);

            character.moveCandi = new Dictionary<int, GameObject>();
            character.attackCandi = new Dictionary<int, GameObject>();
            character.moveRoute = new Dictionary<int, List<int>>();

            var candidates = CheckTile(position, go, character);
            for (int i = 1; i < character.moveRange; i++)
            {
                var childCandidates = new Dictionary<int, GameObject>();
                foreach (var candi in candidates)
                {
                    var tempCandidates = CheckTile(candi.Key, candi.Value, character);
                    foreach (var tempCandi in tempCandidates)
                    {
                        if (childCandidates.ContainsKey(tempCandi.Key) == false)
                            childCandidates.Add(tempCandi.Key, tempCandi.Value);
                    }
                }
                candidates = childCandidates;
            }

            foreach (var candi in character.moveCandi)
                CheckEnemy(candi.Key, candi.Value, character);
        }

        public void ShowPath(Character character, int targetPos)
        {
            var gamePanel = transform.Find("GamePanel");
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            var mapTilePadding = GameConsts.mapTilePadding;
            var startPosX = GameConsts.startPosX;
            var startPosY = GameConsts.startPosY;
            var pathPoint = transform.Find("GamePanel/PathPoint").gameObject;
            var pathLine = transform.Find("GamePanel/PathLine").gameObject;
            var pathEnd = transform.Find("GamePanel/PathEnd").gameObject;

            character.moveRoute.TryGetValue(targetPos, out var route);
            if (route == null)
                return;

            character.routeObject = new List<GameObject>();
            character.go.SetActive(false);

            GameObject routeObject;
            var pos = new Vector2(startPosX + mapTilePadding * (character.position % mapWidth), startPosY - mapTilePadding * (character.position / mapWidth));
            routeObject = InstantiateObject(pathPoint, gamePanel, pos);
            character.routeObject.Add(routeObject);

            bool beforeHorizontal = true;
            for (int i = 1; i < route.Count; i++)
            {
                if (character.moveCandi.TryGetValue(route[i], out var go))
                {
                    var rotZ = 0;
                    if (route[i - 1] + 1 == route[i]) // 좌에서 우 
                    {
                        pos = new Vector2(startPosX - 32 + mapTilePadding * (route[i] % mapWidth), startPosY - mapTilePadding * (route[i] / mapWidth));
                    }
                    else if (route[i - 1] - 1 == route[i]) // 우에서 좌
                    {
                        pos = new Vector2(startPosX + 32 + mapTilePadding * (route[i] % mapWidth), startPosY - mapTilePadding * (route[i] / mapWidth));
                        rotZ = 180;
                    }
                    else if (route[i - 1] + mapWidth == route[i]) // 상에서 하 
                    {
                        pos = new Vector2(startPosX + mapTilePadding * (route[i] % mapWidth), startPosY + 32 - mapTilePadding * (route[i] / mapWidth));
                        rotZ = 270;
                    }
                    else if (route[i - 1] - mapWidth == route[i]) // 하에서 상 
                    {
                        pos = new Vector2(startPosX + mapTilePadding * (route[i] % mapWidth), startPosY - 32 - mapTilePadding * (route[i] / mapWidth));
                        rotZ = 90;
                    }

                   
                    if (route[i - 1] - 1 == route[i] || route[i - 1] + 1 == route[i])
                    {
                        routeObject = InstantiateObject(pathLine, gamePanel, pos);
                        character.routeObject.Add(routeObject);
                        if (beforeHorizontal == false)
                        {
                            beforeHorizontal = true;
                            // 동그라미 추가
                            pos = new Vector2(startPosX + mapTilePadding * (route[i-1] % mapWidth), startPosY - mapTilePadding * (route[i-1] / mapWidth));
                            routeObject = InstantiateObject(pathPoint, gamePanel, pos);
                            character.routeObject.Add(routeObject);
                        }
                    }
                    else if (route[i - 1] - mapWidth == route[i] || route[i - 1] + mapWidth == route[i])
                    {
                        routeObject = InstantiateObject(pathLine, gamePanel, pos);
                        routeObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                        character.routeObject.Add(routeObject);
                        if (beforeHorizontal == true)
                        {
                            beforeHorizontal = false;
                            // 동그라미 추가
                            pos = new Vector2(startPosX + mapTilePadding * (route[i-1] % mapWidth), startPosY - mapTilePadding * (route[i-1] / mapWidth));
                            routeObject = InstantiateObject(pathPoint, gamePanel, pos);
                            character.routeObject.Add(routeObject);
                        }
                    }

                    if (i + 1 == route.Count) // 마지막 인덱스
                    {
                        pos = new Vector2(startPosX + mapTilePadding * (route[i] % mapWidth), startPosY - mapTilePadding * (route[i] / mapWidth));
                        routeObject = InstantiateObject(pathEnd, gamePanel, pos);
                        routeObject.transform.rotation = Quaternion.Euler(0, 0, rotZ);
                        character.routeObject.Add(routeObject);

                        var characterEnd = InstantiateObject(character.go, gamePanel, pos);
                        SetAnimation(characterEnd, "walk");
                        character.routeObject.Add(characterEnd);
                    }
                }
            }
        }

        public void HidePath(Character character, int targetPos)
        {
            character.go.SetActive(true);
            SetAnimation(character.go, "idle");
            character.moveRoute.TryGetValue(targetPos, out var route);
            if (route == null)
                return;

            foreach (var obj in character.routeObject)
                Destroy(obj);
        }

        private Dictionary<int, GameObject> CheckTile(int position, GameObject obj, Character ch)
        {
            var character = CharacterManager.GetCharacterByClicked(battle.allies, battle.enemies);
            if (character == null)
                character = ch;

            if (!character.moveRoute.TryGetValue(position, out var moveRouteList))
                moveRouteList = new List<int> { position };

            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            var mapTilePadding = GameConsts.mapTilePadding;
            var gamePanel = transform.Find("GamePanel");
            var moveAreaObject = transform.Find("GamePanel/MoveArea").gameObject;
            var attackAreaObject = transform.Find("GamePanel/AttackArea").gameObject;
            var candidates = new Dictionary<int, GameObject>();

            if ((position % mapWidth) - 1 >= 0)
            {
                int nextPosition = position - 1;
                TileType tileType = gameData.mapData[nextPosition].type;
                Vector2 newPosition = new Vector2(obj.GetComponent<RectTransform>().anchoredPosition.x - mapTilePadding, obj.GetComponent<RectTransform>().anchoredPosition.y);

                bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                if (hasEnemy)
                {
                    if (character.attackCandi.ContainsKey(nextPosition) == false)
                    {
                        var attackArea = InstantiateObject(attackAreaObject, gamePanel, newPosition);
                        attackArea.transform.GetChild(0).gameObject.SetActive(true);
                        ChangeImageAlpha(attackArea, 1.0f);
                        AddEventTrigger(attackArea, "enemy", nextPosition);
                        character.attackCandi.Add(nextPosition, attackArea);
                    }
                }
                else
                {
                    bool hasCharacter = CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, nextPosition);
                    bool isCanMoveTile = TileManager.IsCanMoveTile(tileType);
                    if (!hasCharacter && isCanMoveTile)
                    {
                        if (character.moveCandi.ContainsKey(nextPosition) == false && character.position != nextPosition)
                        {
                            var moveArea = InstantiateObject(moveAreaObject, gamePanel, newPosition);
                            AddEventTrigger(moveArea, "tile", nextPosition);
                            character.moveCandi.Add(nextPosition, moveArea);
                            candidates.Add(nextPosition, moveArea);

                            if (character.moveRoute.ContainsKey(nextPosition) == false)
                            {
                                var newMoveRouteList = new List<int>();
                                foreach (var routeIndex in moveRouteList)
                                    newMoveRouteList.Add(routeIndex);
                                newMoveRouteList.Add(nextPosition);

                                character.moveRoute.Add(nextPosition, newMoveRouteList);
                            }
                        }
                    }

                }
            }

            if ((position % mapWidth) + 1 < mapWidth)
            {
                int nextPosition = position + 1;
                TileType tileType = gameData.mapData[nextPosition].type;
                Vector2 newPosition = new Vector2(obj.GetComponent<RectTransform>().anchoredPosition.x + mapTilePadding, obj.GetComponent<RectTransform>().anchoredPosition.y);

                bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                if (hasEnemy)
                {
                    if (character.attackCandi.ContainsKey(nextPosition) == false)
                    {
                        var attackArea = InstantiateObject(attackAreaObject, gamePanel, newPosition);
                        attackArea.transform.GetChild(0).gameObject.SetActive(true);
                        ChangeImageAlpha(attackArea, 1.0f);
                        AddEventTrigger(attackArea, "enemy", nextPosition);
                        character.attackCandi.Add(nextPosition, attackArea);
                    }
                }
                else
                {
                    bool hasCharacter = CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, nextPosition);
                    bool isCanMoveTile = TileManager.IsCanMoveTile(tileType);
                    if (!hasCharacter && isCanMoveTile)
                    {
                        if (character.moveCandi.ContainsKey(nextPosition) == false && character.position != nextPosition)
                        {
                            var moveArea = InstantiateObject(moveAreaObject, gamePanel, newPosition);
                            AddEventTrigger(moveArea, "tile", nextPosition);
                            character.moveCandi.Add(nextPosition, moveArea);
                            candidates.Add(nextPosition, moveArea);

                            if (character.moveRoute.ContainsKey(nextPosition) == false)
                            {
                                var newMoveRouteList = new List<int>();
                                foreach (var routeIndex in moveRouteList)
                                    newMoveRouteList.Add(routeIndex);
                                newMoveRouteList.Add(nextPosition);

                                character.moveRoute.Add(nextPosition, newMoveRouteList);
                            }
                        }
                    }
                }
            }

            if (position + mapWidth < mapWidth * mapHeight)
            {
                int nextPosition = position + gameData.mapWidth;
                TileType tileType = gameData.mapData[nextPosition].type;
                Vector2 newPosition = new Vector2(obj.GetComponent<RectTransform>().anchoredPosition.x, obj.GetComponent<RectTransform>().anchoredPosition.y - mapTilePadding);

                bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                if (hasEnemy)
                {
                    if (character.attackCandi.ContainsKey(nextPosition) == false)
                    {
                        var attackArea = InstantiateObject(attackAreaObject, gamePanel, newPosition);
                        attackArea.transform.GetChild(0).gameObject.SetActive(true);
                        ChangeImageAlpha(attackArea, 1.0f);
                        AddEventTrigger(attackArea, "enemy", nextPosition);
                        character.attackCandi.Add(nextPosition, attackArea);
                    }
                }

                bool hasCharacter = CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, nextPosition);
                bool isCanMoveTile = TileManager.IsCanMoveTile(tileType);
                if (!hasCharacter && isCanMoveTile)
                {
                    if (character.moveCandi.ContainsKey(nextPosition) == false && character.position != nextPosition)
                    {
                        var moveArea = InstantiateObject(moveAreaObject, gamePanel, newPosition);
                        AddEventTrigger(moveArea, "tile", nextPosition);
                        character.moveCandi.Add(nextPosition, moveArea);
                        candidates.Add(nextPosition, moveArea);

                        if (character.moveRoute.ContainsKey(nextPosition) == false)
                        {
                            var newMoveRouteList = new List<int>();
                            foreach (var routeIndex in moveRouteList)
                                newMoveRouteList.Add(routeIndex);
                            newMoveRouteList.Add(nextPosition);

                            character.moveRoute.Add(nextPosition, newMoveRouteList);
                        }
                    }
                }
            }

            if (position - mapWidth >= 0)
            {
                int nextPosition = position - mapWidth;
                TileType tileType = gameData.mapData[nextPosition].type;
                Vector2 newPosition = new Vector2(obj.GetComponent<RectTransform>().anchoredPosition.x, obj.GetComponent<RectTransform>().anchoredPosition.y + mapTilePadding);

                bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                if (hasEnemy)
                {
                    if (character.attackCandi.ContainsKey(nextPosition) == false)
                    {
                        var attackArea = InstantiateObject(attackAreaObject, gamePanel, newPosition);
                        attackArea.transform.GetChild(0).gameObject.SetActive(true);
                        ChangeImageAlpha(attackArea, 1.0f);
                        AddEventTrigger(attackArea, "enemy", nextPosition);
                        character.attackCandi.Add(nextPosition, attackArea);
                    }
                }

                bool hasCharacter = CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, nextPosition);
                bool isCanMoveTile = TileManager.IsCanMoveTile(tileType);
                if (!hasCharacter && isCanMoveTile)
                {
                    if (character.moveCandi.ContainsKey(nextPosition) == false && character.position != nextPosition)
                    {
                        var moveArea = InstantiateObject(moveAreaObject, gamePanel, newPosition);
                        AddEventTrigger(moveArea, "tile", nextPosition);
                        character.moveCandi.Add(nextPosition, moveArea);
                        candidates.Add(nextPosition, moveArea);

                        if (character.moveRoute.ContainsKey(nextPosition) == false)
                        {
                            var newMoveRouteList = new List<int>();
                            foreach (var routeIndex in moveRouteList)
                                newMoveRouteList.Add(routeIndex);
                            newMoveRouteList.Add(nextPosition);

                            character.moveRoute.Add(nextPosition, newMoveRouteList);
                        }
                    }
                }
            }

            return candidates;
        }

        private void CheckEnemy(int position, GameObject obj, Character character)
        {
            bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;

            var mapTilePadding = GameConsts.mapTilePadding;
            var gamePanel = transform.Find("GamePanel");
            var moveAreaObject = transform.Find("GamePanel/MoveArea").gameObject;
            var attackAreaObject = transform.Find("GamePanel/AttackArea").gameObject;
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            var mapData = gameData.mapData;

            var parentList = new List<int>();
            parentList.Add(position);

            int i = 1;
            while (true)
            {
                if (i == character.attackRange + 1)
                    break;

                var childList = new List<int>();
                foreach (var parent in parentList)
                {
                    if ((parent % mapWidth) - 1 >= 0)
                    {
                        int nextPosition = parent - 1;
                        var tileType = mapData[nextPosition].type;
                        if (character.attackCandi.ContainsKey(nextPosition) == false
                            && character.moveCandi.ContainsKey(nextPosition) == false
                            && CharacterManager.IsCharacterInPosition(battle.allies, battle.enemies, character.key, nextPosition) == false
                            && TileManager.IsCanAtkTile(tileType))
                        {
                            bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                            var go = InstantiateObject(attackAreaObject, gamePanel, mapData[nextPosition].go.GetComponent<RectTransform>().anchoredPosition);
                            go.transform.GetChild(0).gameObject.SetActive(hasEnemy);
                            ChangeImageAlpha(go);
                            AddEventTrigger(go, "enemy", nextPosition);
                            character.attackCandi.Add(nextPosition, go);
                            childList.Add(nextPosition);
                        }
                        if (character.moveCandi.ContainsKey(nextPosition) == true || character.attackCandi.ContainsKey(nextPosition) == true)
                            childList.Add(nextPosition);
                    }

                    if ((parent % mapWidth) + 1 < mapWidth)
                    {
                        int nextPosition = parent + 1;
                        var tileType = mapData[nextPosition].type;
                        if (character.attackCandi.ContainsKey(nextPosition) == false
                            && character.moveCandi.ContainsKey(nextPosition) == false
                            && CharacterManager.IsCharacterInPosition(battle.allies, battle.enemies, character.key, nextPosition) == false
                            && TileManager.IsCanAtkTile(tileType))
                        {
                            bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                            var go = InstantiateObject(attackAreaObject, gamePanel, mapData[nextPosition].go.GetComponent<RectTransform>().anchoredPosition);
                            go.transform.GetChild(0).gameObject.SetActive(hasEnemy);
                            ChangeImageAlpha(go);
                            AddEventTrigger(go, "enemy", nextPosition);
                            character.attackCandi.Add(nextPosition, go);
                            childList.Add(nextPosition);
                        }
                        if (character.moveCandi.ContainsKey(nextPosition) == true || character.attackCandi.ContainsKey(nextPosition) == true)
                            childList.Add(nextPosition);
                    }

                    if (parent + mapWidth < mapWidth * mapHeight)
                    {
                        int nextPosition = parent + gameData.mapWidth;
                        var tileType = gameData.mapData[nextPosition].type;
                        if (character.attackCandi.ContainsKey(nextPosition) == false
                            && character.moveCandi.ContainsKey(nextPosition) == false
                            && CharacterManager.IsCharacterInPosition(battle.allies, battle.enemies, character.key, nextPosition) == false
                            && TileManager.IsCanAtkTile(tileType))
                        {
                            bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                            var go = InstantiateObject(attackAreaObject, gamePanel, mapData[nextPosition].go.GetComponent<RectTransform>().anchoredPosition);
                            go.transform.GetChild(0).gameObject.SetActive(hasEnemy);
                            ChangeImageAlpha(go);
                            AddEventTrigger(go, "enemy", nextPosition);
                            character.attackCandi.Add(nextPosition, go);
                            childList.Add(nextPosition);
                        }
                        if (character.moveCandi.ContainsKey(nextPosition) == true || character.attackCandi.ContainsKey(nextPosition) == true)
                            childList.Add(nextPosition);
                    }

                    if (parent - gameData.mapWidth >= 0)
                    {
                        int nextPosition = parent - gameData.mapWidth;
                        var tileType = gameData.mapData[nextPosition].type;
                        if (character.attackCandi.ContainsKey(nextPosition) == false 
                            && character.moveCandi.ContainsKey(nextPosition) == false 
                            && CharacterManager.IsCharacterInPosition(battle.allies, battle.enemies, character.key, nextPosition) == false 
                            && TileManager.IsCanAtkTile(tileType))
                        {
                            bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                            var go = InstantiateObject(attackAreaObject, gamePanel, mapData[nextPosition].go.GetComponent<RectTransform>().anchoredPosition);
                            go.transform.GetChild(0).gameObject.SetActive(hasEnemy);
                            ChangeImageAlpha(go);
                            AddEventTrigger(go, "enemy", nextPosition);
                            character.attackCandi.Add(nextPosition, go);
                            childList.Add(nextPosition);
                        }
                        if (character.moveCandi.ContainsKey(nextPosition) == true || character.attackCandi.ContainsKey(nextPosition) == true)
                            childList.Add(nextPosition);
                    }
                }

                parentList = childList.ConvertAll(s => s);
                i++;
            }
        }

        public bool CheckAttackable(int characterPosition, int characterAttackRange, int enemyPosition, CharacterKey key)
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;

            var character = CharacterManager.GetCharacterByClicked(battle.allies, battle.enemies);
            if (character == null)
            {
                Debug.LogError("캐릭터가 없다");
                return false;
            }

            characterAttackRange--;
            if (characterAttackRange < 0)
                return false;

            bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
            bool hasEnemyInPosition = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, enemyPosition);
            TileType tileType = gameData.mapData[characterPosition].type;
            bool isMoveTile = TileManager.IsCanMoveTile(tileType);
            if ((characterPosition % gameData.mapWidth) - 1 >= 0)
            {
                var leftSide = characterPosition - 1;
                if (hasEnemyInPosition && leftSide == enemyPosition && TileManager.IsCanAtkTile(tileType))
                    return true;

                TileType nextTileType = gameData.mapData[characterPosition - 1].type;
                bool isMoveNextTile = TileManager.IsCanMoveTile(nextTileType);
                if (isMoveNextTile == true)
                    if (CheckAttackable(leftSide, characterAttackRange, enemyPosition, key) == true)
                        return true;
            }

            if ((characterPosition % gameData.mapWidth) + 1 < mapWidth)
            {
                var rightSide = characterPosition + 1;
                if (hasEnemyInPosition && rightSide == enemyPosition && TileManager.IsCanAtkTile(tileType))
                    return true;

                TileType nextTileType = gameData.mapData[rightSide].type;
                bool nextMoveTile = TileManager.IsCanMoveTile(nextTileType);
                if (nextMoveTile)
                    if (CheckAttackable(rightSide, characterAttackRange, enemyPosition, key) == true)
                        return true;
            }

            if (characterPosition + mapWidth < mapWidth * mapHeight)
            {
                var downSide = characterPosition + mapWidth;
                if (hasEnemyInPosition && downSide == enemyPosition && TileManager.IsCanAtkTile(tileType))
                    return true;

                TileType nextTileType = gameData.mapData[downSide].type;
                bool nextMoveTile = TileManager.IsCanMoveTile(nextTileType);
                if (nextMoveTile)
                    if (CheckAttackable(downSide, characterAttackRange, enemyPosition, key) == true)
                        return true;
            }

            if (characterPosition - mapWidth >= 0)
            {
                var upSide = characterPosition - mapWidth;
                if (hasEnemyInPosition && upSide == enemyPosition && TileManager.IsCanAtkTile(tileType))
                    return true;
                TileType nextTileType = gameData.mapData[upSide].type;
                bool nextMoveTile = TileManager.IsCanMoveTile(nextTileType);
                if (nextMoveTile)
                    if (CheckAttackable(upSide, characterAttackRange, enemyPosition, key) == true)
                        return true;
            }

            return false;
        }

        public void MoveCharacter(Character character, GameObject targetObject, int newPosition, bool isAttack = false)
        {
            var tmpString = character.key == CharacterKey.ALLY ? "아군" : "적군";
            Debug.Log(tmpString + " " + character.position + "에서" + newPosition + "으로 이동합니다.");

            if (isAttack == true || character.position == newPosition)
                character.go.transform.position = gameData.mapData[newPosition].go.transform.position;
            else
                character.go.transform.position = targetObject.transform.position;
            character.position = newPosition;

            character.state = CharacterState.TURN_START;
            ChangeEventTrigger(character.go, "character", newPosition);
        }

        public void DeadCharacterCallback(Character character)
        {
            SetAnimation(character.go, "dead");
            RemoveEventTrigger(character.go);
            Destroy(character.go);

            if (character.key == CharacterKey.ALLY)
            {
                Debug.Log("아군" + character.objectId + "이 " + character.position + "위치에서" + "사망하였습니다.");
                character.state = CharacterState.DEAD;
                battle.allyCount--;
            }
            else
            {
                Debug.Log("적군" + character.objectId + "이 " + character.position + "위치에서" + "사망하였습니다.");
                character.state = CharacterState.DEAD;
                battle.enemyCount--;
            }
        }

        public int GetShortestPositionInMoveList(Character character, int targetPosition)
        {
            foreach (var moveCandi in character.moveCandi)
            {
                if (CheckAttackable(moveCandi.Key, character.attackRange, targetPosition, character.key) == true)
                {
                    return moveCandi.Key;
                }   
            }

            return -1;
        }

        public Tuple<int, List<int>> GetShortestEnemyPositionAndRoute(int position, bool isMyTeam) // 현재 내 위치에서 가중치가 가장 낮은 적의 포지션을 반환
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            int min = mapWidth * mapHeight;
            int enemyPos = -1;
            Dictionary<int, List<int>> enemyInfo = new Dictionary<int, List<int>>();

            // 일반적으로 장애물보다는 타일이 많으므로 dfs가 아닌 bfs로 계속 꾸준히 돈다
            var enemyList = isMyTeam ? battle.enemies : battle.allies;
            List<int> routeList = new List<int>();
            foreach (var enemy in enemyList)
            {
                if (enemy.state == CharacterState.DEAD) // 죽은 적은 제외
                    continue;

                var candi = BFS(enemy.position, 0, position, isMyTeam);
                int depth = candi.Item1;

                if (depth != -1 && depth < min)
                {
                    min = depth;
                    routeList = candi.Item2;
                    enemyPos = enemy.position;
                }
            }

            return new Tuple<int, List<int>> (enemyPos, routeList);
        }

        public Tuple<int, List<int>> BFS(int targetPos, int depth, int position, bool isMyTeam, List<bool> isVisited = null, Dictionary<int, List<int>> parentList = null) // 자식 노드가 최대 3개(이전 포지션을 제외한 상, 하, 좌, 우)인 TREE 구조
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            var mapData = gameData.mapData;
            var tileList = gameData.tileList;
            
            if (parentList == null && position != -1)
            {
                var routeList = new List<int>(); // TODO: LinkedList로 바꿔서 메모리 낭비하지 않기 (?? ConvertAll)
                routeList.Add(position);
                for (int i = 1; i < mapWidth * mapHeight; i++)
                    routeList.Add(0);

                parentList = new Dictionary<int, List<int>>();
                parentList.Add(position, routeList);
            }

            if (isVisited == null)
            {
                isVisited = new List<bool>();
                for (int i = 0; i < mapWidth * mapHeight; i++)
                    isVisited.Add(false);
            }

            var childList = new Dictionary<int, List<int>>();
            foreach (var parent in parentList)
            {
                var routeList = parent.Value;
                var index = parent.Key;
                isVisited[index] = true;

                // 좌
                if ((index % mapWidth) - 1 >= 0 && isVisited[index - 1] == false) // IndexOutOfRange 및 방문 노드 여부 체크 
                {
                    int newPos = index - 1;
                    if (CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, newPos) == true && targetPos == newPos) // 타겟을 찾은 경우
                    {
                        // Debug.Log("적을 발견하였다. 깊이 : " + depth + " 좌표 : " + newPos);
                        isVisited[newPos] = true;
                        routeList[depth] = newPos;

                        return new Tuple<int, List<int>> (depth, routeList);
                    }
                    else if (CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, newPos) == false && (TileType)tileList[newPos] != TileType.WALL) // 이동이 가능한 경우
                    {
                        // Debug.Log("부모 : " + parent + "에서" + "좌표 : " + newPos + " 방문");
                        isVisited[newPos] = true;
                        routeList[depth] = newPos;
                        var route = routeList.ConvertAll(i => i);
                        childList.Add(newPos, route);
                        routeList[depth] = 0;
                    }
                }

                // 우
                if ((index % mapWidth) + 1 < mapWidth && isVisited[index + 1] == false)
                {
                    int newPos = index + 1;
                    if (CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, newPos) == true && targetPos == newPos)
                    {
                        //Debug.Log("적을 발견하였다. 깊이 : " + depth + " 좌표 : " + newPos);
                        isVisited[newPos] = true;
                        routeList[depth] = newPos;

                        return new Tuple<int, List<int>>(depth, routeList);
                    }
                    else if (CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, newPos) == false && (TileType)tileList[newPos] != TileType.WALL) // 이동이 가능한 경우
                    {
                        // Debug.Log("부모 : " + parent + "에서" + "좌표 : " + newPos + " 방문");
                        isVisited[newPos] = true;
                        routeList[depth] = newPos;
                        var route = routeList.ConvertAll(i => i);
                        childList.Add(newPos, route);
                        routeList[depth] = 0;
                    }
                }

                // 상
                if ((index + mapWidth) < mapWidth * mapHeight && isVisited[index + mapWidth] == false)
                {
                    int newPos = index + mapWidth;
                    if (CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, newPos) == true && targetPos == newPos)
                    {
                        // Debug.Log("적을 발견하였다. 깊이 : " + depth + " 좌표 : " + newPos);
                        isVisited[newPos] = true;
                        routeList[depth] = newPos;

                        return new Tuple<int, List<int>>(depth, routeList);
                    }
                    else if (CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, newPos) == false && (TileType)tileList[newPos] != TileType.WALL) // 이동이 가능한 경우
                    {
                        // Debug.Log("부모 : " + parent + "에서" + "좌표 : " + newPos + " 방문");
                        isVisited[newPos] = true;
                        routeList[depth] = newPos;
                        var route = routeList.ConvertAll(i => i);
                        childList.Add(newPos, route);
                        routeList[depth] = 0;
                    }
                }

                // 하
                if (index - mapWidth >= 0 && isVisited[index - mapWidth] == false)
                {
                    int newPos = index - mapWidth;
                    if (CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, newPos) == true && targetPos == newPos)
                    {
                        //Debug.Log("적을 발견하였다. 깊이 : " + depth + " 좌표 : " + newPos);
                        isVisited[newPos] = true;
                        routeList[depth] = newPos;

                        return new Tuple<int, List<int>>(depth, routeList);
                    }
                    else if (CharacterManager.HasCharacterInPosition(battle.allies, battle.enemies, newPos) == false && (TileType)tileList[newPos] != TileType.WALL) // 이동이 가능한 경우
                    {
                        // Debug.Log("부모 : " + parent + "에서" + "좌표 : " + newPos + " 방문");
                        isVisited[newPos] = true;
                        routeList[depth] = newPos;
                        var route = routeList.ConvertAll(i => i);
                        childList.Add(newPos, route);
                        routeList[depth] = 0;
                    }
                }
            }

            if (childList.Count == 0)
            {
                return new Tuple<int, List<int>>(-1, new List<int>());
            }
            else
            {
                var result = BFS(targetPos, depth + 1, -1, isMyTeam, isVisited, childList);
                if (result.Item1 != -1)
                    return new Tuple<int, List<int>>(result.Item1, result.Item2);
            }

            return new Tuple<int, List<int>>(-1, new List<int>());
        }

        public int GetNearestMoveEnemyPosition(List<int> moveList, bool isMyTeam, int position) // 현재 캐릭터의 좌표에서 적의 위치들 중 가장 가까운 이동 가능한 포지션을 반환한다.
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            int min = mapWidth * mapHeight;
            List<int> enemyPositions = new List<int>();

            var targetInfo = GetShortestEnemyPositionAndRoute(position, isMyTeam);
            int targetEnemyPosition = targetInfo.Item1;
            List<int> routeList = targetInfo.Item2;

            min = mapWidth * mapHeight;

            int movePos = -1;
            foreach (var pos in routeList)
            {
                if (moveList.Contains(pos))
                    movePos = pos;
            }
           
            if (movePos == -1) // 최적 이동 경로가 없는 경우
            {
                if (moveList.Count != 0)
                {
                    Debug.Log("적에게 최적 이동 경로가 없으면 이동 가능한 곳 중에 랜덤으로 이동한다.");
                    return moveList[UnityEngine.Random.Range(0, moveList.Count - 1)];
                }

                Debug.Log("적에게 최적 이동 경로가 없으면 이동 가능한 곳도 없으면 제자리에 있는다.");
                return position;
            }

            return movePos;
        }

        public int GetNearestAttackEnemyPosition(List<int> attackList, bool isMyTeam, int position) // 현재 캐릭터의 좌표에서 적의 위치들 중 가장 공격할 수 있는 포지션을 반환한다.
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            int min = mapWidth * mapHeight;
            List<int> attackPositions = new List<int>();
            foreach (var attackPosition in attackList) // 정해진 적의 위치로 attackList 중에서 가장 가까운 좌표를 찾는다.
            {
                int rowWeight = Math.Abs((attackPosition / mapWidth) - (position / mapWidth));
                int colWeight = Math.Abs((attackPosition % mapWidth) - (position % mapHeight));
                int weight = rowWeight + colWeight;
                if (min == weight)
                    attackPositions.Add(attackPosition);
                else if (min > weight)
                {
                    min = weight;
                    attackPositions = new List<int>
                    {
                        attackPosition
                    };
                }
            }

            int targetPosition = 0;
            if (attackPositions.Count == 1)
            {
                targetPosition = attackPositions[0];
                Debug.Log("공격할 수 있는 좌표 중 적과 가장 가까운 포지션 : " + targetPosition);
            }
            else
            {
                targetPosition = attackPositions[UnityEngine.Random.Range(0, attackPositions.Count)];
                Debug.Log("공격할 수 있는 좌표 중 적과 가장 가까운 포지션 : " + targetPosition); // 가중치가 같으면 랜덤으로 적 타겟을 정한다.
            }

            return targetPosition;
        }

        public void RemoveCandi(Character character)
        {
            if (battle.ClickedCharacterIdx != -1)
                battle.ClickedCharacterIdx = -1;

            if (character.tmpPosition != -1)
                character.tmpPosition = -1;

            foreach (var moveCandi in character.moveCandi)
                Destroy(moveCandi.Value);

            foreach (var attackCandi in character.attackCandi)
                Destroy(attackCandi.Value);

            character.moveCandi = new Dictionary<int, GameObject>();
            character.attackCandi = new Dictionary<int, GameObject>();
        }

        public void EndTurn(Character character) // 대기, 이동, 공격, 스킬 등의 행위 이후의 처리
        {
            Debug.Log(character.position + "턴 종료");
            ChangeImageAlpha(character.go, 0.7f);
            SetAnimation(character.go, "idle");
            HidePath(character, character.tmpPosition); // 임시로 이동하여 보여준 경로 제거
            character.state = CharacterState.TURN_END;
            character.tmpPosition = -1;

            // 스킬 관련
            uiContents.fixedPanel.HideSkillPanel();
            HideSkillRange(character);
            uiContents.fixedPanel.skillBtn.gameObject.SetActive(false);

            RemoveCandi(character);
            uiContents.fixedPanel.turnBtn.gameObject.SetActive(false);
            battle.ClickedCharacterIdx = -1;
            battle.SelectedSkill = null;
            battle.SkillPosition = -1;
            battle.turnCount--;

            if (battle.enemyCount <= 0 || battle.allyCount <= 0)
            {
                Debug.Log("아군 혹은 적군이 모두 죽어 게임 종료 처리합니다.");
                LuaMain.Instance.DoRequest($"{LuaMain.Instance.CurrentState["ServerUri"]}/lua/ingame_end.lua");
                return;
            }

            if (battle.turnCount == 0)
                StartEnemyTurn();
        }

        public void ClickMoveBtn(Character character, GameObject go)
        {
            MoveCharacter(character, go, character.tmpPosition);
            RequestCharacterMove(character.objectId, character.position, character.Hp.Value());

            EndTurn(character);
        }

        public void ClickAtkBtn(Character character, Character enemy, int position, bool isAtk)
        {
            Debug.Log("공격 클릭");
            StartCoroutine(CO_ClickAtkBtn(character, enemy, position, isAtk));
        }

        public IEnumerator CO_ClickAtkBtn(Character character, Character enemy, int position, bool isAtk)
        {
            GameObject obj;
            if (character.routeObject != null)
                obj = character.routeObject[character.routeObject.Count - 1];
            else
                obj = character.go;

            yield return WaitForSpineAnimationComplete(obj, "attack");
            ShowVFX("Hit", transform.Find("GamePanel"), enemy.go.GetComponent<RectTransform>().anchoredPosition);
            yield return new WaitForSeconds(1.0f);

            uiContents.fixedPanel.ShowVsPanel(character, enemy, true);
            uiContents.fixedPanel.ShowBattleView();
            
            Character deadCharacter;
            float damageRate =
                TileManager.CalcDamageRate(gameData.mapData[character.position], gameData.mapData[enemy.position])
                * BattleManager.CalcDamageRate(character.jobId, enemy.jobId);

            if (BattleManager.InCounterAttackRange(character, enemy))
                deadCharacter = BattleManager.GiveDamage(character, enemy, damageRate, true);
            else
                deadCharacter = BattleManager.GiveDamage(character, enemy, damageRate, false);

            if (deadCharacter != null)
            {
                yield return WaitForSpineAnimationComplete(deadCharacter.go, "dead");
                DeadCharacterCallback(deadCharacter);
            }
                
            if (character.Hp.Value() <= 0)
            {
                yield return WaitForSpineAnimationComplete(obj, "dead");
                DeadCharacterCallback(character);
            }

            if (position != -1)
                MoveCharacter(character, character.go, position, isAtk);
            RequestCharacterAttack(character.objectId, character.position, character.Hp.Value(), enemy.objectId, enemy.Hp.Value());

            EndTurn(character);

            yield return new WaitForSeconds(2.0f);
            uiContents.fixedPanel.HideBattleView();
            uiContents.fixedPanel.HideVsPanel();
        }

        public void UseSkill(Character character, int touchPosition)
        {
            Debug.Log("스킬을 보여준다.");
            StartCoroutine(CO_UseSkill(character, touchPosition));
        }

        public IEnumerator CO_UseSkill(Character character, int touchPosition)
        {
            var obj = character.go;
            if (obj == null)
            {
                Debug.Log("캐릭터 오브젝트가 없습니다.");
            }
            else
            {
                Debug.Log("캐릭터 오브젝트가 있습니다. 이펙트를 적용합니다.");
                // SetAnimation(character.go, "attack", false);
            }

            var skill = battle.SelectedSkill;
            Debug.Log("사용한 스킬의 skill id :" + skill.dataId);
            Debug.Log("터치한 위치" + touchPosition);
            SkillParam skillParam;
            if (skill.skillParams.Count > 1) // 상태이상 + 공격인경우  
            {
                var buffIndex = skill.skillParams[0].num;
                skillParam = skill.skillParams[1];
                Debug.Log("스킬 파라미터가 2개 이상입니다." + skill.skillParams[0].num + " " + skill.skillParams[1].num);

                Debug.Log("상태이상 파라미터 ID " + buffIndex);
                if (buffIndex == 14)
                {
                    Debug.Log("스턴"); // debuff
                }
                else if (buffIndex == 15)
                {
                    Debug.Log("중독(도트대미지)");
                }
                else if (buffIndex == 16)
                {
                    Debug.Log("빙결(이동불가)");
                }
                else if (buffIndex == 17)
                {
                    Debug.Log("출혈(도트대미지)");
                }
                else if (buffIndex == 18)
                {
                    Debug.Log("수면(피해받으면 깨어남)");
                }
            }
            else
            {
                skillParam = skill.skillParams[0];
            }

            // num -> skill_basic 인덱스
            if (skillParam.num == 1)
            {
                Debug.Log("대미지 (atk기반)");
                yield return null;
            }
            else if (skillParam.num == 2)
            {
                Debug.Log("대미지(atk % 기반)");
                Debug.Log(character.tmpPosition);
                character.tmpPosition = battle.SkillPosition;
                var enemy = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, touchPosition);
                uiContents.fixedPanel.HideStatsPanel();
                uiContents.fixedPanel.ShowVsPanel(character, enemy, true);
                uiContents.fixedPanel.ShowBattleView();

                if (character.tmpPosition != -1)  // 캐릭터 이동하여 스킬 쓴 경우
                    MoveCharacter(character, character.go, character.tmpPosition, true);
                
                // NOTE: 스킬도 반격 받을 수 있다면 처리

                var damageRate = skillParam.value / 100;
                Character deadCharacter;
                deadCharacter = BattleManager.GiveDamage(character, enemy, damageRate, false);
                if (deadCharacter != null)
                {
                    // yield return WaitForSpineAnimationComplete(deadCharacter.go, "dead");
                    DeadCharacterCallback(deadCharacter);
                }
                RequestCharacterAttack(character.objectId, character.position, character.Hp.Value(), enemy.objectId, enemy.Hp.Value());

                yield return new WaitForSeconds(2.0f);
                uiContents.fixedPanel.HideBattleView();
                uiContents.fixedPanel.HideVsPanel();
            }
            else if (skillParam.num == 5)
            {
                Debug.Log("적군의 atk 감소"); // debuff
                var enemy = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, touchPosition);
                enemy.Atk.AddValue(-skillParam.value, false);
            }
            else if (skillParam.num == 6)
            {
                Debug.Log("적군의 atk % 감소"); // debuff
                var enemy = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, touchPosition);
                enemy.Atk.AddValue(-enemy.Atk.Value() * skillParam.value / 100, false);
            }
            else if (skillParam.num == 9)
            {
                Debug.Log("적군의 def 감소"); // debuff
                var enemy = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, touchPosition);
                enemy.Def.AddValue(-skillParam.value, false);
            }
            else if (skillParam.num == 10)
            {
                Debug.Log("적군의 def % 감소"); // debuff
                var enemy = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, touchPosition);
                enemy.Def.AddValue(-enemy.Atk.Value() * skillParam.value / 100, false);
            }
            else if (skillParam.num == 11)
            {
                Debug.Log("move_range 증가"); // buff
                character.moveRange += skillParam.value;
            }
            else if (skillParam.num == 12)
            {
                Debug.Log("move_range 감소"); // debuff
                var enemy = CharacterManager.GetCharacterByPosition(battle.allies, battle.enemies, touchPosition);
                enemy.moveRange -= skillParam.value;
            }
            else if (skillParam.num == 13)
            {
                Debug.Log("반격(일반공격에 한해서)"); // debuff
            }

            else if (skillParam.num == 19)
            {
                if (skillParam.turn != 0)
                {
                    Debug.Log(skillParam.turn + " 턴 동안 스턴 저항 수치 상승");
                }
                else
                {
                    Debug.Log("스턴저항 수치 영구 상승");
                    character.StunResist.AddValue(skillParam.value, false);
                }
            }
            else if (skillParam.num == 20)
            {
                if (skillParam.turn != 0)
                {
                    Debug.Log(skillParam.turn + " 턴 동안 독저항 수치 상승");
                }
                else
                {
                    Debug.Log("독저항 수치 영구 상승");
                    character.PoisonResist.AddValue(skillParam.value, false);
                }
            }
            else if (skillParam.num == 21)
            {
                if (skillParam.turn != 0)
                {
                    Debug.Log(skillParam.turn + " 턴 동안 빙결저항 수치 상승");
                }
                else
                {
                    Debug.Log("빙결저항 수치 영구 상승");
                    character.IceResist.AddValue(skillParam.value, false);
                }
            }
            else if (skillParam.num == 22)
            {
                if (skillParam.turn != 0)
                {
                    Debug.Log(skillParam.turn + " 턴 동안 출혈저항 수치 상승");
                }
                else
                {
                    Debug.Log("출혈저항 수치 영구 상승");
                    character.BleedingResist.AddValue(skillParam.value, false);
                }
            }
            else if (skillParam.num == 23)
            {
                if (skillParam.turn != 0)
                {
                    Debug.Log(skillParam.turn + " 턴 동안 수면저항 수치 상승");
                }
                else
                {
                    Debug.Log("수면저항 수치 영구 상승");
                    character.SleepResist.AddValue(skillParam.value, false);
                }
            }
            else if (skillParam.num == 24)
            {
                if (skillParam.turn != 0)
                {
                    Debug.Log(skillParam.turn + " 턴 동안 상태이상저항 수치 상승");
                }
                else
                {
                    Debug.Log("상태이상저항 수치 영구 상승");
                    character.AllResist.AddValue(skillParam.value, false);
                }
            }
            else if (skillParam.num == 25)
            {
                Debug.Log("관통(방어무시)");
            }
            else if (skillParam.num == 26)
            {
                Debug.Log("힐(나의atk %)");
            }
            else if (skillParam.num == 27)
            {
                Debug.Log("힐(대상hp %)");
            }
            else if (skillParam.num == 28)
            {
                Debug.Log("도트힐");
            }
            else if (skillParam.num == 29)
            {
                Debug.Log("부활");
            }
            else if (skillParam.num == 30)
            {
                Debug.Log("공격유도(도발, pve전용)");
            }
            else if (skillParam.num == 31)
            {
                Debug.Log("희생(대신맞기)");
            }
            else if (skillParam.num == 32)
            {
                Debug.Log("밀쳐냄");
            }
            else if (skillParam.num == 33)
            {
                Debug.Log("한번 더 이동");
            }
            else if (skillParam.num == 34)
            {
                Debug.Log("흡혈");
            }
            else if (skillParam.num == 35)
            {
                Debug.Log("흡혈(%)");
            }
            else
            {
                Debug.Log("스킬 ID에 문제가 있다.");
            }
            
            EndTurn(character);

            yield return null;
        }

        public void ShowSingleSkillRange(Character character, int position, string skillType, int range)
        {
            if (character.skillCandi == null)
                character.skillCandi = new Dictionary<int, GameObject>();

            var candidates = CheckSkill(position, character.go, character, skillType);
            for (int i = 1; i < range; i++)
            {
                var childCandidates = new Dictionary<int, GameObject>();
                foreach (var candi in candidates)
                {
                    var tempCandidates = CheckSkill(candi.Key, candi.Value, character, skillType);
                    foreach (var tempCandi in tempCandidates)
                    {
                        if (childCandidates.ContainsKey(tempCandi.Key) == false)
                            childCandidates.Add(tempCandi.Key, tempCandi.Value);
                    }
                }
                candidates = childCandidates;
            }
        }

        public Dictionary<int, GameObject> CheckSkill(int position, GameObject obj, Character character, string skillType) // TODO: 스킬 사용이 불가능(타일, 적) 등 예외처리 추가, 파라미터 정리
        {
            var mapWidth = gameData.mapWidth;
            var mapHeight = gameData.mapHeight;
            var mapTilePadding = GameConsts.mapTilePadding;
            var startPosX = GameConsts.startPosX;
            var startPosY = GameConsts.startPosY;

            var gamePanel = transform.Find("GamePanel");
            var skillAreaObject = transform.Find("GamePanel/SkillArea").gameObject;
            var candidates = new Dictionary<int, GameObject>();

            if ((position % mapWidth) - 1 >= 0)
            {
                int nextPosition = position - 1;
                if (character.skillCandi.ContainsKey(nextPosition) == false && position != nextPosition)
                {
                    TileType tileType = gameData.mapData[nextPosition].type;
                    var newPosition = new Vector2(startPosX + mapTilePadding * (nextPosition % mapWidth), startPosY - mapTilePadding * (nextPosition / mapWidth));
                    var skillArea = InstantiateObject(skillAreaObject, gamePanel, newPosition);
                    bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                    if (skillType == "ally")
                    {
                        bool hasAlly = CharacterManager.HasAllyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                        if (hasAlly)
                        {
                            // skillArea.transform.GetChild(0).gameObject.SetActive(true); 버프 아이콘
                            AddEventTrigger(skillArea, "skill_buff", nextPosition);
                        }
                    }
                    else if (skillType == "enemy")
                    {
                        bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                        if (hasEnemy)
                        {
                            skillArea.transform.GetChild(0).gameObject.SetActive(true);
                            AddEventTrigger(skillArea, "skill_attack", nextPosition);
                        }
                    }
                    else
                    {
                        AddEventTrigger(skillArea, "skill_area", nextPosition);
                    }
                    character.skillCandi.Add(nextPosition, skillArea);
                    candidates.Add(nextPosition, skillArea);
                }
            }

            if ((position % mapWidth) + 1 < mapWidth)
            {
                int nextPosition = position + 1;
                if (character.skillCandi.ContainsKey(nextPosition) == false && position != nextPosition)
                {
                    TileType tileType = gameData.mapData[nextPosition].type;
                    var newPosition = new Vector2(startPosX + mapTilePadding * (nextPosition % mapWidth), startPosY - mapTilePadding * (nextPosition / mapWidth));
                    var skillArea = InstantiateObject(skillAreaObject, gamePanel, newPosition);
                    bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                    if (skillType == "ally")
                    {
                        bool hasAlly = CharacterManager.HasAllyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                        if (hasAlly)
                        {
                            // skillArea.transform.GetChild(0).gameObject.SetActive(true); 버프 아이콘
                            AddEventTrigger(skillArea, "skill_buff", nextPosition);
                        }
                    }
                    else if (skillType == "enemy")
                    {
                        bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                        if (hasEnemy)
                        {
                            skillArea.transform.GetChild(0).gameObject.SetActive(true);
                            AddEventTrigger(skillArea, "skill_attack", nextPosition);
                        }
                    }
                    else
                    {
                        AddEventTrigger(skillArea, "skill_area", nextPosition);
                    }
                    character.skillCandi.Add(nextPosition, skillArea);
                    candidates.Add(nextPosition, skillArea);
                }
            }

            if (position + mapWidth < mapWidth * mapHeight)
            {
                int nextPosition = position + gameData.mapWidth;
                if (character.skillCandi.ContainsKey(nextPosition) == false && position != nextPosition)
                {
                    TileType tileType = gameData.mapData[nextPosition].type;
                    var newPosition = new Vector2(startPosX + mapTilePadding * (nextPosition % mapWidth), startPosY - mapTilePadding * (nextPosition / mapWidth));
                    var skillArea = InstantiateObject(skillAreaObject, gamePanel, newPosition);
                    bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                    if (skillType == "ally")
                    {
                        bool hasAlly = CharacterManager.HasAllyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                        if (hasAlly)
                        {
                            // skillArea.transform.GetChild(0).gameObject.SetActive(true); 버프 아이콘
                            AddEventTrigger(skillArea, "skill_buff", nextPosition);
                        }
                    }
                    else if (skillType == "enemy")
                    {
                        bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                        if (hasEnemy)
                        {
                            skillArea.transform.GetChild(0).gameObject.SetActive(true);
                            AddEventTrigger(skillArea, "skill_attack", nextPosition);
                        }
                    }
                    else
                    {
                        AddEventTrigger(skillArea, "skill_area", nextPosition);
                    }
                    character.skillCandi.Add(nextPosition, skillArea);
                    candidates.Add(nextPosition, skillArea);
                }
            }

            if (position - mapWidth >= 0)
            {
                int nextPosition = position - mapWidth;
                if (character.skillCandi.ContainsKey(nextPosition) == false && position != nextPosition)
                {
                    TileType tileType = gameData.mapData[nextPosition].type;
                    var newPosition = new Vector2(startPosX + mapTilePadding * (nextPosition % mapWidth), startPosY - mapTilePadding * (nextPosition / mapWidth));
                    var skillArea = InstantiateObject(skillAreaObject, gamePanel, newPosition);
                    bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                    if (skillType == "ally")
                    {
                        bool hasAlly = CharacterManager.HasAllyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                        if (hasAlly)
                        {
                            // skillArea.transform.GetChild(0).gameObject.SetActive(true); 버프 아이콘
                            AddEventTrigger(skillArea, "skill_buff", nextPosition);
                        }
                    }
                    else if (skillType == "enemy")
                    {
                        bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, nextPosition);
                        if (hasEnemy)
                        {
                            skillArea.transform.GetChild(0).gameObject.SetActive(true);
                            AddEventTrigger(skillArea, "skill_attack", nextPosition);
                        }
                    }
                    else
                    {
                        AddEventTrigger(skillArea, "skill_area", nextPosition);
                    }
                    character.skillCandi.Add(nextPosition, skillArea);
                    candidates.Add(nextPosition, skillArea);
                }
            }

            return candidates;
        }

        public void ShowSkillAreaRange(Character character, int position, string skillType, int range) // 광역형 스킬 범위
        {
            Debug.Log("광역으로 보여줄 범위" + range); // TODO: 범위 원거리 공격 처리
            if (character.skillCandi == null)
                character.skillCandi = new Dictionary<int, GameObject>();

            var mapWidth = gameData.mapWidth;
            var mapTilePadding = GameConsts.mapTilePadding;
            var startPosX = GameConsts.startPosX;
            var startPosY = GameConsts.startPosY;

            var gamePanel = transform.Find("GamePanel");
            var skillAreaObject = transform.Find("GamePanel/SkillArea").gameObject;

            var width = range * 2 + 1;
            var middleValue = (width * width - 1) / 2;
            var startPos = position - range * mapWidth - range;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var pos = startPos + (i * mapWidth) + j;
                    // Debug.Log("검사할 좌표 : " + pos);
                    var newPosition = new Vector2(startPosX + mapTilePadding * (pos % mapWidth), startPosY - mapTilePadding * (pos / mapWidth));
                    var skillArea = InstantiateObject(skillAreaObject, gamePanel, newPosition);

                    bool isMyTeam = character.key == CharacterKey.ALLY ? true : false;
                    if (skillType == "ally")
                    {
                        bool hasAlly = CharacterManager.HasAllyInPosition(battle.allies, battle.enemies, isMyTeam, pos);
                        if (hasAlly)
                        {
                            uiContents.fixedPanel.skillBtn.gameObject.SetActive(true);
                            AddEventTrigger(skillArea, "skill_attack", pos);
                        }
                    }
                    else if (skillType == "enemy")
                    {
                        bool hasEnemy = CharacterManager.HasEnemyInPosition(battle.allies, battle.enemies, isMyTeam, pos);
                        if (hasEnemy)
                        {
                            skillArea.transform.GetChild(0).gameObject.SetActive(true);
                            uiContents.fixedPanel.skillBtn.gameObject.SetActive(true);
                            AddEventTrigger(skillArea, "skill_attack", pos);
                        }
                    }
                    else
                    {
                        AddEventTrigger(skillArea, "skill_area", pos);
                    }
                    character.skillCandi.Add(pos, skillArea);
                }
            }
        }

        public void HideSkillRange(Character character)
        {
            if (character.skillCandi == null)
                return;

            foreach(var candi in character.skillCandi)
            {
                Destroy(candi.Value);
            }

            character.skillCandi = new Dictionary<int, GameObject>();
        }

        public void RequestCharacterMove(int characterObjectId, int characterPosition, int characterHp)
        {
            StartCoroutine(CO_RequestCharacterMove(characterObjectId, characterPosition, characterHp));
        }

        public IEnumerator CO_RequestCharacterMove(int characterObjectId, int characterPosition, int characterHp)
        {
            var req = UnityWebRequest.Post($"{LuaMain.Instance.CurrentState["ServerUri"]}/srpg/move", new Dictionary<string, string>
            {
                { "data", $"{{\"uid\":{LuaMain.Instance.CurrentState["User.uid"]},\"mover\":{characterObjectId},\"mover_hp\":{characterHp},\"to\":{characterPosition},\"target\":{0}}}" }
            });
            yield return req.SendWebRequest();
            req.Dispose();
        }

        public void RequestCharacterAttack(int characterObjectId, int characterPosition, int characterHp, int enemyObjectId, int enemyHp)
        {
            StartCoroutine(CO_RequestCharacterAttack(characterObjectId, characterPosition, characterHp, enemyObjectId, enemyHp));
        }

        public IEnumerator CO_RequestCharacterAttack(int characterObjectId, int characterPosition, int characterHp, int enemyObjectId, int enemyHp)
        {
            var req = UnityWebRequest.Post($"{LuaMain.Instance.CurrentState["ServerUri"]}/srpg/move", new Dictionary<string, string>
            {
                { "data", $"{{\"uid\":{LuaMain.Instance.CurrentState["User.uid"]},\"mover\":{characterObjectId},\"mover_hp\":{characterHp},\"to\":{characterPosition},\"target\":{enemyObjectId},\"target_hp\":{enemyHp}}}" }
            });
            yield return req.SendWebRequest();
            req.Dispose();
        }

        public static void DestroyObject(GameObject go)
        {
            Destroy(go);
        }

        public static GameObject InstantiateObject(GameObject obj, Transform parent, Vector2 position)
        {
            GameObject go = Instantiate(obj, parent);
            go.GetComponent<RectTransform>().anchoredPosition = position;
            go.SetActive(true);

            return go;
        }

        public static void ChangeImageAlpha(GameObject obj, float alpha = 0.5f)
        {
            if (obj == null)
                return;

            var image = obj.GetComponent<Image>();
            if (image)
                image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }

        public static void SetAnimation(GameObject obj, string name, bool loop = true)
        {
            SkeletonGraphic sg = obj.transform.GetChild(0).GetComponent<SkeletonGraphic>();
            if (sg)
                sg.AnimationState.SetAnimation(0, name, loop);
        }

        public static void ShowVFX(string name, Transform parent)
        {
            var vfx = LuaAssets.Instantiate(name);
            vfx.transform.SetParent(parent);
        }

        public static void ShowVFX(string name, Transform parent, Vector3 pos)
        {
            var vfx = LuaAssets.Instantiate(name);
            vfx.transform.SetParent(parent);
            vfx.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        public static IEnumerator WaitForSpineAnimationComplete(GameObject obj, string name)
        {
            if (obj == null)
            {
                Debug.Log("캐릭터 오브젝트가 없습니다" + name);
                yield break;
            }
                
            Debug.Log("공격 애니메이션을 보여줍니다." + name);
            SkeletonGraphic sg = obj.transform.GetChild(0).GetComponent<SkeletonGraphic>();
            if (sg)
            {
                TrackEntry track = sg.AnimationState.SetAnimation(0, name, false);
                if (track != null)
                    yield return new WaitForSpineAnimationComplete(track);
            }

            yield break;
        }

        public bool IsAutoClicked()
        {
            return battle.isAutoPlay;
        }
    }
}