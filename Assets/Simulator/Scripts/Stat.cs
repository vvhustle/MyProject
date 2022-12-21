using System;
using UnityEngine;

namespace Simulator
{
    public enum StatType
    {
        hp,
        atk,
        def,

        all_resist, // 모든 저항 (상태이상면역)
        stun_resist, //기절 저항
        poison_resist, // 중독 저항
        bleeding_resist, // 출혈 저항
        ice_resist, // 빙결 저항
        sleep_resist, // 수면 저항
        
        attack_resist, // 근거리 공격피해저항
        range_attack_resist, // 원거리 공격피해저항
        magic_attack_resist, // 마법 공격 피해저항
        hp_turn_heal, // 턴당 생명력회복

        pierce, // 방어구 관통
        life_steal, // 흡혈
        block, // 막음
        evade, // 회피

        Count,
    }

    public class Stat
    {
        public static int MaxInt = 999999;
        public static int MaxPercent = 100;

        public int MaxValue;
        public int MaxValuePercent;

        private int _value;
        private int _valuePercent;

        public void AddValue(int value, bool AddMax = false)
        {
            var v = _value + value;
            if (v < 0)
                v = 0;
            if (AddMax)
            {
                MaxValue = v;
            }
            else
            {
                if (v > MaxValue)
                    v = MaxValue;
            }
            _value = v;
        }

        public void AddValuePercent(int valuePercent)
        {
            var v = _valuePercent + valuePercent;
            if (v < 0)
                v = 0;
            if (v > MaxValuePercent)
                v = MaxValuePercent;
            _valuePercent = v;
        }

        public int Value()
        {
            var v = _value;
            v += (int)(Math.Floor(v * (_valuePercent * 0.01f)));
            if (v < 0)
                v = 0;
            if (v > MaxValue)
                v = MaxValue;
            return v;
        }

        public float Ratio
        {
            get
            {
                return (float)_value / (float)MaxValue;
            }
        }

        public Stat(int value)
        {
            Init(value, value, MaxPercent);
        }

        public Stat(int value, int max)
        {
            Init(value, max, MaxPercent);
        }

        public void Init(int value, int max, int maxPercent)
        {
            MaxValue = max;
            MaxValuePercent = maxPercent;
            _value = value;
        }
    }
}
