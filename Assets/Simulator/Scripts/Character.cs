using GM;
using System.Collections.Generic;
using UnityEngine;

namespace Simulator
{
    public enum CharacterKey
    {
        ALLY,
        ENEMY,
    }

    public enum CharacterState
    {
        TURN_START,
        TURN_END,
        
        SELF_CLICK,
        MOVE_CLICK,
        ATTACK_CLICK,
        SKILL_CLICK,

        DEAD,
    }

    public class CharacterStatData
    {
        public int data_id;
        public int level;
        public int job;
        public int img;
        public int attack_range;
        public int move_range;
        public int hp;
        public int atk;
        public int def;
        public int max_hp;
        public int[] skills;
    }

    public class Character
    {
        public GameObject go;
        public int Img { get; private set; }
        public List<Skill> skills = new List<Skill>();
        //public List<Buff> buffs = new List<Buff>();
        //public List<Buff> debuffs = new List<Buff>();

        public Stat Hp { get; private set; }
        public Stat Atk { get; private set; }
        public Stat Def { get; private set; }
        public Stat AllResist { get; private set; } // 상태 이상 면역
        public Stat StunResist { get; private set; } // 기절 저항
        public Stat PoisonResist { get; private set; } // 중독 저항
        public Stat IceResist { get; private set; } // 빙결 저항
        public Stat BleedingResist { get; private set; } // 출혈 저항
        public Stat SleepResist { get; private set; } // 수면 저항

        public Stat HpTurnHeal { get; private set; } // 도트 힐
        public Stat Pierce { get; private set; } // 관통
        public Stat LifeSteal { get; private set; } // 흡혈

        public int dataId;
        public int level;
        public int jobId;
        public int objectId;
        public int moveRange;
        public int attackRange;
        public CharacterKey key;
        public CharacterState state;

        public int position;
        public int tmpPosition = -1;

        public Dictionary<int, List<int>> moveRoute;
        public Dictionary<int, GameObject> moveCandi; // < 포지션, 오브젝트 >
        public List<GameObject> routeObject; // 임시 경로
        public Dictionary<int, GameObject> attackCandi;
        public Dictionary<int, GameObject> skillCandi;// 스킬 range에 따른 스킬 사용 가능 범위

        public Character(GameObject gameObject, int characterType, int pos, CharacterKey characterKey)
        {
            var statData = CharacterManager.GetCharacterStatData(characterType);
            go = gameObject;
            position = pos;
            objectId = characterType;
            key = characterKey;
            state = CharacterState.TURN_START;

            Img = statData.img;
            level = statData.level;
            Hp = new Stat(statData.hp, statData.max_hp);
            Atk = new Stat(statData.atk, 9999);
            Def = new Stat(statData.def, 9999);
            InitResistStats();

            if (statData.skills != null)
            {
                foreach (var skillIndex in statData.skills) // TODO: 자료형을 래핑하여 편하게 형변환해주는 클래스 및 메서드 만들기
                {
                    var skillData = LuaMain.Instance.CurrentState.GetTable("GameData.skill")[skillIndex.ToString()] as LuaInterface.LuaTable;
                    Skill skill = new Skill
                    {
                        dataId = skillIndex,
                        icon = skillData["icon"].ToString(),
                        type = skillData["type"].ToString(),
                        targetType = skillData["target_type"].ToString(),
                        targetRange = int.Parse(skillData["target_range"].ToString()),
                        motion = skillData["motion"].ToString(),
                        fxUser = skillData["fx_user"].ToString(),
                        fxTarget = skillData["fx_target"].ToString(),

                    };

                    skill.AddSkillParam(int.Parse(skillData["param_0"].ToString()), int.Parse(skillData["value_0"].ToString()), int.Parse(skillData["turn_0"].ToString()), int.Parse(skillData["range_0"].ToString()), int.Parse(skillData["prob_0"].ToString()));
                    if (skillData["param_1"] != null)
                        skill.AddSkillParam(int.Parse(skillData["param_1"].ToString()), int.Parse(skillData["value_1"].ToString()), int.Parse(skillData["turn_1"].ToString()), int.Parse(skillData["range_1"].ToString()), int.Parse(skillData["prob_1"].ToString()));
                    // TODO : 보다 체계적인 칼럼 형식을 만들거나
                    //skill.AddSkillParam(int.Parse(skillData["param_2"].ToString()), int.Parse(skillData["value_2"].ToString()), int.Parse(skillData["turn_2"].ToString()), int.Parse(skillData["prob_2"].ToString()));
                    //skill.AddSkillParam(int.Parse(skillData["param_3"].ToString()), int.Parse(skillData["value_3"].ToString()), int.Parse(skillData["turn_3"].ToString()), int.Parse(skillData["prob_3"].ToString()));
                    //skill.AddSkillParam(int.Parse(skillData["param_4"].ToString()), int.Parse(skillData["value_4"].ToString()), int.Parse(skillData["turn_4"].ToString()), int.Parse(skillData["prob_4"].ToString()));

                    skills.Add(skill);
                    if (skill.type == "passive")
                    {
                        Debug.Log("패시브 스킬을 적용한다.");
                        skill.ApplyPassive(this);
                    }
                }
            }

            dataId = statData.data_id;
            jobId = statData.job;
            moveRange = statData.move_range;
            attackRange = statData.attack_range;
        }

        public void InitResistStats()
        {
            AllResist = new Stat(0, 9999);
            StunResist = new Stat(0, 9999);
            PoisonResist = new Stat(0, 9999);
            IceResist = new Stat(0, 9999);
            BleedingResist = new Stat(0, 9999);
            SleepResist = new Stat(0, 9999);
        }
    }
}
