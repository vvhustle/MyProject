using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Simulator
{
    public static class BattleManager
    {
        public static Character GiveDamage(Character attacker, Character target, float damageRate = 1.0f, bool canCounter = false)
        {
            var tmpString = attacker.key == CharacterKey.ALLY ? "아군" : "적군";
            var tmpString2 = attacker.key == CharacterKey.ALLY ? "적군" : "아군";
            Debug.Log(tmpString + attacker.position + "이 " + tmpString2 + target.position + "을 공격합니다.");
            Debug.Log("DAMAGE RATE : " + damageRate);

            Debug.Log("공격자의 공격력 : " + attacker.Atk.Value());
            Debug.Log("버프 후의 공격력 : " + Mathf.FloorToInt(attacker.Atk.Value() * damageRate));
            Debug.Log("방어자의 방어력 : " + target.Def.Value());

            int damage = Mathf.FloorToInt(attacker.Atk.Value() * damageRate) - target.Def.Value();
            Debug.Log("데미지 : " + damage);
            if (damage <= 0)
            {
                Debug.Log("데미지가 0이하면 데미지 1로 보정합니다.");
                damage = 1;
            }

            Debug.Log(tmpString2 + target.position + "의 과거 체력 : " + target.Hp.Value());
            target.Hp.AddValue(-damage);
            Debug.Log(tmpString2 + target.position + "의 현재 체력 :" + target.Hp.Value());

            var hpImage = target.go.transform.Find("HpBar/Image").GetComponent<Image>();
            hpImage.fillAmount = target.Hp.Ratio;

            if (canCounter == true)
            {
                Debug.Log("반격 가능한 범위라서 " + tmpString2+ "이 반격을 합니다");
                GiveDamage(target, attacker, 1, false);
            }

            if (target.Hp.Value() <= 0)
                return target;

            return null;
        }

        public static bool InCounterAttackRange(Character attacker, Character target) // 타겟의 공격 범위 안에 공격자가 있으면 true를 리턴해 반격한다. 
        {
            // TODO: 궁수 같은 녀석의 공격 범위를 유동적으로 적용할 수 있도록 로직 추가
            if (attacker.attackRange <= target.attackRange)
                return true;

            return false;
        }

        public static float CalcDamageRate(int attackerJobId, int targetJobId)
        {
            float damageRate  = CharacterManager.GetCharacterCombatData(attackerJobId, targetJobId);
            return damageRate * 0.01f;
        }

        public static Character GetInTurnCharacter(List<Character> allies)
        {
            Character character = null;
            foreach (var ally in allies)
            {
                if (ally.state == CharacterState.SELF_CLICK ||
                    ally.state == CharacterState.MOVE_CLICK ||
                    ally.state == CharacterState.ATTACK_CLICK)
                {
                    character = ally;
                    break;
                }
            }
            return character;
        }
    }
}