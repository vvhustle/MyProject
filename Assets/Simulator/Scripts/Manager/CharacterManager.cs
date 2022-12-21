using System.Collections.Generic;
using UnityEngine;

namespace Simulator
{
    public static class CharacterManager
    {
        public static Dictionary<int, CharacterStatData> characterStatDict;

        public static CharacterStatData GetCharacterStatData(int key)
        {
            if (characterStatDict == null)
            {
                return null;
            }
            if (!characterStatDict.ContainsKey(key))
            {
                return null;
            }
            return characterStatDict[key];
        }

        public static int GetCharacterCombatData(int attackerJobId, int targetJobId)
        {
            var attacker = attackerJobId.ToString()[0];
            var target = targetJobId.ToString()[0];

            var combatRow = GM.LuaMain.Instance.CurrentState.GetTable("GameData.character_job_combat")[attacker.ToString()] as LuaInterface.LuaTable;
            return int.Parse(combatRow[target.ToString()].ToString());
        }

        public static string GetChracterName(int id)
        {
            return GM.L.Get($"CHARACTER_NAME.{id}");
        }

        public static string GetCharacterJobName(int jobId)
        {
            return GM.L.Get($"JOB_NAME.{jobId}");
        }

        public static Character GetCharacterByObject(List<Character> allyList, List<Character> enemyList, GameObject go)
        {
            foreach (var ally in allyList)
                if (ally.go == go)
                    return ally;

            foreach (var enemy in enemyList)
                if (enemy.go == go)
                    return enemy;

            return null;
        }

        public static Character GetCharacterByPosition(List<Character> allyList, List<Character> enemyList, int position)
        {
            foreach (var ally in allyList)
                if (ally.position == position && ally.state != CharacterState.DEAD)
                    return ally;

            foreach (var enemy in enemyList)
                if (enemy.position == position && enemy.state != CharacterState.DEAD)
                    return enemy;

            return null;
        }

        public static Character GetEnemyByPosition(List<Character> allyList, List<Character> enemyList, CharacterKey key, int position)
        {
            if (key == CharacterKey.ALLY)
            {
                foreach (var enemy in enemyList)
                    if (enemy.position == position)
                        return enemy;
            }
            else if (key == CharacterKey.ENEMY)
            {
                foreach (var ally in allyList)
                    if (ally.position == position)
                        return ally;
            }

            return null;
        }

        public static Character GetCharacterByClicked(List<Character> allyList, List<Character> enemyList)
        {
            foreach (var character in allyList)
                if (character.state == CharacterState.SELF_CLICK || character.state == CharacterState.MOVE_CLICK || character.state == CharacterState.ATTACK_CLICK)
                    return character;

            foreach (var enemy in enemyList)
                if (enemy.state == CharacterState.SELF_CLICK || enemy.state == CharacterState.MOVE_CLICK || enemy.state == CharacterState.ATTACK_CLICK)
                    return enemy;

            return null;
        }

        public static bool IsCharacterInPosition(List<Character> allyList, List<Character> enemyList, CharacterKey key, int position)
        {
            bool isMyTeam = key == CharacterKey.ALLY ? true : false;
            if (isMyTeam)
            {
                foreach (var ally in allyList)
                    if (ally.state != CharacterState.DEAD && ally.position == position)
                        return true;
            }
            else
            {
                foreach (var enemy in enemyList)
                    if (enemy.state != CharacterState.DEAD && enemy.position == position)
                        return true;
            }

            return false;
        }

        // position에 캐릭터 (아군 / 적군)이 존재하는지 검사
        public static bool HasCharacterInPosition(List<Character> allyList, List<Character> enemyList, int position)
        {
            foreach (var ally in allyList)
                if (ally.state != CharacterState.DEAD && ally.position == position)
                    return true;

            foreach (var enemy in enemyList)
                if (enemy.state != CharacterState.DEAD && enemy.position == position)
                    return true;

            return false;
        }

        // position에 아군이 존재하는지 검사
        public static bool HasAllyInPosition(List<Character> allyList, List<Character> enemyList, bool isMyTeam, int position)
        {
            if (isMyTeam)
            {
                foreach (var ally in allyList)
                    if (ally.state != CharacterState.DEAD && ally.position == position)
                        return true;
            }
            else
            {
                foreach (var enemy in enemyList)
                    if (enemy.state != CharacterState.DEAD && enemy.position == position)
                        return true;
            }

            return false;
        }

        // position에 적군이 존재하는지 검사
        public static bool HasEnemyInPosition(List<Character> allyList, List<Character> enemyList, bool isMyTeam, int position)
        {
            if (isMyTeam)
            {
                foreach (var enemy in enemyList)
                    if (enemy.state != CharacterState.DEAD && enemy.position == position)
                        return true;
            }
            else
            {
                foreach (var ally in allyList)
                    if (ally.state != CharacterState.DEAD && ally.position == position)
                        return true;
            }

            return false;
        }
        

        public static List<int> GetMoveableTileList(Character character)
        {
            var moveableList = new List<int>();
            foreach (var candi in character.moveCandi)
            {
                if (candi.Key != character.position)
                    moveableList.Add(candi.Key);
            }

            return moveableList;
        }

        public static List<int> GetAttackableEnemyList(List<Character> enemies, Character character)
        {
            List<int> attackableList = new List<int>();

            List<int> enemyList = new List<int>();
            foreach (var enemy in enemies)
            {
                if (enemy.state != CharacterState.DEAD)
                    enemyList.Add(enemy.position);
            }

            foreach (var candi in character.attackCandi)
            {
                if (enemyList.Contains(candi.Key))
                    attackableList.Add(candi.Key);
            }

            return attackableList;
        }
    }
}
