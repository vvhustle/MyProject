using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulator
{
    public enum TurnState
    {
        ALLY,
        ENEMY,

    }

    public class Battle
    {
        public List<Character> allies;
        public List<Character> enemies;

        public bool isAutoPlay = false;
        public TurnState turnState; // (아군 / 적군)의 턴
        private int _maxTurn = 99; // 전투의 최대 제한 턴 수
        public int allyCount = 0; // 생존한 아군의 수
        public int enemyCount = 0; // 생존한 적군의 수
        public int turnCount = 0; // 진행한 턴의 수
        public int ClickedCharacterIdx = -1; // 클릭된 캐릭터의 위치 인덱스
        public Skill SelectedSkill = null; // 전투에서 선택된 스킬
        public int SkillPosition = -1; // 스킬에서 시전할 위치

        public Battle(int maxTurn)
        {
            _maxTurn = maxTurn;
            turnState = TurnState.ALLY;
            allies = new List<Character>();
            enemies = new List<Character>();
        }

        public void Start()
        {
            allyCount = 1;
            enemyCount = 1;
            turnCount = 1;

        }

        public static readonly Lazy<Battle> instance;
        public static Battle Instance { get { return instance.Value; } }
    }
}
