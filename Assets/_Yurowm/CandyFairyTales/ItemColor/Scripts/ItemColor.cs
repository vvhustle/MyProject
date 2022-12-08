using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using YMatchThree.Core;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public enum ItemColor {
        None = -1,
        KnownRandom = 0,
        Known = 1,
        Universal = 2
    }
    
    [SerializeShort]
    public struct ItemColorInfo : ISerializable {
        public static readonly int[] IDs;
        
        public static readonly ItemColorInfo Unknown = new ItemColorInfo { type = ItemColor.KnownRandom };
        public static readonly ItemColorInfo None = new ItemColorInfo { type = ItemColor.None };
        public static readonly ItemColorInfo Universal = new ItemColorInfo { type = ItemColor.Universal };
        
        static ItemColorInfo() {
            IDs = new int[10];
            for (int i = 0; i < IDs.Length; i++)
                IDs[i] = i;
        }
        
        public ItemColor type;
        public int ID;

        public void Serialize(Writer writer) {
            writer.Write("t", type);
            if (type == ItemColor.Known)
                writer.Write("ID", ID);
        }

        public void Deserialize(Reader reader) {
            reader.Read("t", ref type);
            if (type == ItemColor.Known)
                reader.Read("ID", ref ID);
        }

        public bool IsMatchableColor() {
            return type == ItemColor.Known || type == ItemColor.Universal;
        }

        public static ItemColorInfo ByID(int colorID) {
            return new ItemColorInfo {
                ID = colorID,
                type = ItemColor.Known
            };
        }

        public bool IsKnown() {
            return type == ItemColor.Known || type == ItemColor.KnownRandom;
        }

        public bool IsMatchWith(int colorID) {
            if (type == ItemColor.Universal) return true;
            if (type == ItemColor.Known && ID == colorID) return true;
            return false;
        }
        
        public bool IsMatchWith(ItemColorInfo colorInfo) {
            if (!IsMatchableColor() || !colorInfo.IsMatchableColor())
                return false;
            if (type == ItemColor.Universal || colorInfo.type == ItemColor.Universal)
                return true;
            if (type == ItemColor.Known && colorInfo.type == ItemColor.Known)
                return ID == colorInfo.ID;
            
            return false;
        }

        public bool Equals(ItemColorInfo info) {
            if (type != info.type) 
                return false;
            
            if (type == ItemColor.Known)
                return ID == info.ID;
            
            return true;
        }

        public override bool Equals(object obj) {
            if (obj is ItemColorInfo ici)
                return Equals(ici);
            return false;
        }
        
        public static bool operator ==(ItemColorInfo a, ItemColorInfo b) {
            return a.Equals(b);
        }

        public static bool operator !=(ItemColorInfo a, ItemColorInfo b) {
            return !(a == b);
        }

        public override string ToString() {
            if (type == ItemColor.Known)
                return $"{type}_{ID}";
            
            return type.ToString();
        }
    }
}