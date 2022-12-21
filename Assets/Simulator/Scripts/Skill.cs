using System.Collections.Generic;
using UnityEngine;

namespace Simulator
{
    public class SkillParam
    {
        public int num;
        public int value;
        public int turn;
        public int range;
        public int percent;
        
        public List<int> pos; // TODO : Delete
    }

    public class Skill
    {
        public int dataId;
        public string icon;
        public string type; // 패시브 or 액티브
        public string targetType; // 아군 or 적군 or 자신
        public int targetRange; // 사용 범위
        public string motion; // 스파인 애니메이션 이름
        public string fxUser; // 시전자 이펙트
        public string fxTarget; // 타겟 이펙트

        public List<SkillParam> skillParams = new List<SkillParam>();
        public List<int> activePositions = new List<int>();
        public List<int> targetPositions = new List<int>();

        public void AddSkillParam(int param, int value, int turn, int range, int percent)
        {
            if (param == 0)
                return;

            skillParams.Add(new SkillParam()
            {
                num = param,
                value = value,
                turn = turn,
                range = range,
                percent = percent,
            });
        }

        public void ApplyPassive(Character character) // 패시브 스킬을 적용한다.
        {
            foreach(var skillParam in skillParams)
            {
                if (skillParam.num == 3)
                {
                    Debug.Log("atk 증가 시킨다" + skillParam.value);
                    character.Atk.AddValue(skillParam.value, false);
                }
                if (skillParam.num == 4)
                {
                    Debug.Log("atk % 증가 시킨다" + skillParam.value);
                    character.Atk.AddValue(character.Atk.Value() * skillParam.value / 100, false);
                }
                if (skillParam.num == 7)
                {
                    Debug.Log("def 증가 시킨다" + skillParam.value);
                    character.Def.AddValue(skillParam.value, false);
                }
                if (skillParam.num == 8)
                {
                    Debug.Log("def % 증가 시킨다" + skillParam.value);
                    character.Def.AddValue(character.Def.Value() * skillParam.value / 100, false);
                }
            }
        }

        public void UseActive(Character character, int touchPosition, SkillParam skillParam, int buffIndex = 0) // 액티브 스킬을 사용한다. target_type으로 분기 액티브 안에서 (데미지공격, 버프-디버프)
        {
            Debug.Log("사용한 스킬의 id :" + dataId);
            Debug.Log("스킬데이터 파람" + skillParam.num);
            Debug.Log("터치한 위치" + touchPosition);

            if (buffIndex != 0)
            {
                Debug.Log("적용할 상태이상 인덱스"+ buffIndex);
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

            if (skillParam.num == 1)
            {
                // BattleManager.GiveDamage()
                Debug.Log("대미지 (atk기반)");
            }
            else if (skillParam.num == 2)
            {
                Debug.Log("대미지(atk % 기반)");
            }
            else if (skillParam.num == 5)
            {
                Debug.Log("atk 감소"); // debuff
            }
            else if (skillParam.num == 6)
            {
                Debug.Log("atk % 감소"); // debuff
            }
            else if (skillParam.num == 9)
            {
                Debug.Log("def 감소"); // debuff
            }
            else if (skillParam.num == 10)
            {
                Debug.Log("def % 감소"); // debuff
            }
            else if (skillParam.num == 11)
            {
                Debug.Log("move_range 증가"); // buff
                character.moveRange += skillParam.value;
            }
            else if (skillParam.num == 12)
            {
                Debug.Log("move_range 감소"); // debuff
            }
            else if (skillParam.num == 13)
            {
                Debug.Log("반격(일반공격에 한해서)"); // debuff
            }
            
            else if (skillParam.num == 19)
            {
                Debug.Log("스턴저항");
                character.StunResist.AddValue(skillParam.value, false);

            }
            else if (skillParam.num == 20)
            {
                Debug.Log("독저항");
                character.PoisonResist.AddValue(skillParam.value, false);
            }
            else if (skillParam.num == 21)
            {
                Debug.Log("빙결저항");
                character.IceResist.AddValue(skillParam.value, false);
            }
            else if (skillParam.num == 22)
            {
                Debug.Log("출혈저항");
                character.BleedingResist.AddValue(skillParam.value, false);
            }
            else if (skillParam.num == 23)
            {
                Debug.Log("수면저항");
                character.SleepResist.AddValue(skillParam.value, false);
            }
            else if (skillParam.num == 24)
            {
                Debug.Log("모든상태이상면역");
                character.AllResist.AddValue(skillParam.value, false);
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

            if (targetType == "self")
            {
                Debug.Log("타겟이 self이면 나를 선택해야한다.");
            }

            else if (targetType == "ally")
            {
                Debug.Log("타겟이 ally이면 아군을 클릭해야한다.");

            }
            
            else if (targetType == "enemy")
            {
                Debug.Log("타겟이 enemy이면 적군을 클릭해야한다.");
            }
        }

        public void UseBuff(Character character, SkillParam skillParam, int buffIndex = 0) // 생각해보니까 사실상이건 필요 없는 듯 ;; 
        {
            if (skillParam.num == 3)
            {
                Debug.Log("atk 증가 시킨다" + skillParam.value);
                character.Atk.AddValue(skillParam.value, false);
            }
            else if(skillParam.num == 4)
            {
                Debug.Log("atk % 증가 시킨다" + skillParam.value);
                character.Atk.AddValue(character.Atk.Value() * skillParam.value / 100, false);
            }
            else if(skillParam.num == 7)
            {
                Debug.Log("def 증가 시킨다" + skillParam.value);
                character.Def.AddValue(skillParam.value, false);
            }
            else if(skillParam.num == 8)
            {
                Debug.Log("def % 증가 시킨다" + skillParam.value);
                character.Def.AddValue(character.Def.Value() * skillParam.value / 100, false);
            }
            else if (skillParam.num == 13)
            {
                Debug.Log("반격 준비");
            }
            else if (skillParam.num == 30)
            {
                Debug.Log("공격유도(도발, pve전용)");
            }
            else if (skillParam.num == 33)
            {
                Debug.Log("한번 더 이동");
            }
        }
    }
}