using UnityEngine;

namespace Simulator
{
    public interface ITouchState
    {
        Simulator Simul { get; set; }
        void TouchObject(GameObject go = null, int position = 0);
    }
  
    public class TouchCharacter : ITouchState
    {
        public Simulator Simul { get; set; }
        public void TouchObject(GameObject go = null, int position = 0)
        {
            Debug.Log(position + " 포지션의 캐릭터를 선택하였습니다.");

            if (Simul.battle.SelectedSkill != null)
                return;

            var clickedPosition = Simul.battle.ClickedCharacterIdx;
            if (Simul.battle.ClickedCharacterIdx != -1)
            {
                var prevCharacter = CharacterManager.GetCharacterByPosition(Simul.battle.allies, Simul.battle.enemies, Simul.battle.ClickedCharacterIdx);
                prevCharacter.state = CharacterState.TURN_START;
                Simulator.SetAnimation(prevCharacter.go, "idle");
                Simul.HidePath(prevCharacter, prevCharacter.tmpPosition);
                Simul.RemoveCandi(prevCharacter);

                if (clickedPosition == position) // 나 자신을 클릭했을 경우에는 리턴
                {
                    Simul.uiContents.fixedPanel.HideStatsPanel();
                    Simul.uiContents.fixedPanel.HideSkillPanel();
                    return;
                }
            }

            var character = CharacterManager.GetCharacterByPosition(Simul.battle.allies, Simul.battle.enemies, position);
            if (character.state == CharacterState.TURN_END && character.key == CharacterKey.ALLY)
                return;

            character.state = CharacterState.SELF_CLICK;
            Simulator.SetAnimation(character.go, "walk");

            Simul.uiContents.fixedPanel.ShowSkillPanel(character);
            Simul.uiContents.fixedPanel.ShowStatsPanel(character);

            if (character.key == CharacterKey.ALLY)
            {
                Simul.uiContents.fixedPanel.turnBtn.gameObject.SetActive(true);
                Simul.uiContents.fixedPanel.turnBtnText.text = "턴종료";
                Simul.uiContents.fixedPanel.turnBtn.onClick.RemoveAllListeners();
                Simul.uiContents.fixedPanel.turnBtn.onClick.AddListener(() =>
                {
                    Simul.EndTurn(character);
                });
            }
            else
            {
                Simul.uiContents.fixedPanel.turnBtn.gameObject.SetActive(false);
            }

            Simul.battle.ClickedCharacterIdx = position;
            Simul.FindPath(position, go);
        }
    }

    public class TouchTile : ITouchState
    {
        public Simulator Simul { get; set; }
        public void TouchObject(GameObject go = null, int position = 0)
        {
            var character = CharacterManager.GetCharacterByClicked(Simul.battle.allies, Simul.battle.enemies);
            if (character == null)
                return;

            if (character.tmpPosition == position)
                return;

            if (character.skillCandi != null)
            {
                if (character.skillCandi.Count > 0)
                {
                    Debug.Log("리턴합니다");
                    return;
                }
            }

            Simul.uiContents.fixedPanel.turnBtn.gameObject.SetActive(false);
            if (!character.moveCandi.TryGetValue(position, out var temp))
            {
                Simulator.SetAnimation(character.go, "idle");
                Simul.uiContents.fixedPanel.HideStatsPanel();
                Simul.uiContents.fixedPanel.HideSkillPanel();

                character.state = CharacterState.TURN_START;
                Simul.HidePath(character, character.tmpPosition);
                Simul.RemoveCandi(character);
                return;
            }

            if (character.tmpPosition == position)
                return;

            if (character.tmpPosition != -1)
            {
                Simul.HidePath(character, character.tmpPosition);
            }

            character.state = CharacterState.MOVE_CLICK;

            if (character.key == CharacterKey.ENEMY)
            {
                Debug.Log("컴터는 조작이 불가능하다");
                return;
            }
            Simul.ShowPath(character, position);

            character.tmpPosition = position;

            Simul.uiContents.fixedPanel.turnBtn.gameObject.SetActive(true);
            Simul.uiContents.fixedPanel.turnBtnText.text = "대기";
            Simul.uiContents.fixedPanel.turnBtn.onClick.RemoveAllListeners();
            Simul.uiContents.fixedPanel.turnBtn.onClick.AddListener(() =>
            {
                Simul.ClickMoveBtn(character, go);
                Simul.uiContents.fixedPanel.turnBtn.gameObject.SetActive(false);
            });
        }
    }

    public class TouchEnemy : ITouchState
    {
        public Simulator Simul { get; set; }
        public void TouchObject(GameObject go = null, int position = 0)
        {
            var statsPanel = Simul.gameObject.transform.Find("FixedPanel/TopPanel/StatsPanel");
            statsPanel.gameObject.SetActive(false);

            var character = CharacterManager.GetCharacterByObject(Simul.battle.allies, Simul.battle.enemies, go);
            if (character == null)
                character = CharacterManager.GetCharacterByClicked(Simul.battle.allies, Simul.battle.enemies);

            if (character.state == CharacterState.TURN_END)
                return;
            
            var enemy = CharacterManager.GetEnemyByPosition(Simul.battle.allies, Simul.battle.enemies, character.key, position);
            if (enemy == null)
                return;
            
            character.state = CharacterState.ATTACK_CLICK;
            Simul.uiContents.fixedPanel.turnBtnText.text = "공격";

            if (character.key == CharacterKey.ENEMY)
                return;

            int movePosition = -1;
            // 적을 클릭하여 공격하는 경우는 3가지가 있다
            // 1. 임시로 이동한 위치에서 공격 범위 안에 있는 적을 클릭하는 경우
            // 2. 제 자리에서 이동 및 공격 범위 안에 있는 공격가능한 적을 클릭하는 경우
            // 3. 특정 위치로 이동하여 공격 범위 안에 있는 공격가능한 적을 클릭하는 경우

            // 1.
            if (character.tmpPosition != -1 && Simul.CheckAttackable(character.tmpPosition, character.attackRange, position, character.key) == true)
            {
                movePosition = character.tmpPosition;
            }

            // 2.
            else if (Simul.CheckAttackable(character.position, character.attackRange, position, character.key) == true)
            {
                movePosition = character.position;
            }

            // 3.
            else if (Simul.CheckAttackable(character.position, character.attackRange, position, character.key) == false)
            {
                Simul.HidePath(character, character.tmpPosition);
                movePosition = Simul.GetShortestPositionInMoveList(character, position);
                character.tmpPosition = movePosition;
                Simul.ShowPath(character, movePosition);
            }

            Simul.uiContents.fixedPanel.turnBtn.onClick.RemoveAllListeners();
            Simul.uiContents.fixedPanel.turnBtn.onClick.AddListener(() =>
            {
                Simul.ClickAtkBtn(character, enemy, movePosition, true);
                Simul.uiContents.fixedPanel.turnBtn.gameObject.SetActive(false);
            });
        }
    }

    public class TouchSkillArea : ITouchState
    {
        public Simulator Simul { get; set; }
        public void TouchObject(GameObject go = null, int position = 0)
        {
            Debug.Log("스킬 영역을 클릭하였다.");
        }
    }

    public class TouchSkillBuff : ITouchState
    {
        public Simulator Simul { get; set; }
        public void TouchObject(GameObject go = null, int position = 0)
        {
            Debug.Log("스킬 버프 영역을 클릭하였다.");
        }
    }

    public class TouchSkillAttack : ITouchState
    {
        public Simulator Simul { get; set; }
        public void TouchObject(GameObject go = null, int position = 0)
        {
            // 원래는 여기서 스킬을 구현해야하지만 오토플레이에서의 재활용을 위해 시뮬레이터에서 해결한다.
            Simul.uiContents.fixedPanel.skillBtn.gameObject.SetActive(true);
            Simul.uiContents.fixedPanel.skillBtn.onClick.RemoveAllListeners();
            Simul.uiContents.fixedPanel.skillBtn.onClick.AddListener(() =>
            {
                Simul.uiContents.fixedPanel.skillBtn.gameObject.SetActive(false);
                var character = CharacterManager.GetCharacterByClicked(Simul.battle.allies, Simul.battle.enemies);
                Simul.UseSkill(character, position);
            });
        }
    }

}