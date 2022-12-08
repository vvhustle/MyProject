using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    [SerializeShort]
    public class LevelStars : ISerializable {
        
        public int First = 50;
        public int Second = 100;
        public int Third = 200;
        
        public int GetValue(int star) {
            switch (star) {
                case 1: return First;
                case 2: return Second;
                case 3: return Third;
                default: return 0;
            }
        }
        public int GetCount(int score) {
            if (score >= Third) return 3;
            if (score >= Second) return 2;
            if (score >= First) return 1;
            
            return 0;
        }
        
        public void Serialize(Writer writer) {
           writer.Write("1", First); 
           writer.Write("2", Second); 
           writer.Write("3", Third); 
        }

        public void Deserialize(Reader reader) {
            reader.Read("1", ref First);
            reader.Read("2", ref Second);
            reader.Read("3", ref Third);
        }
    }
}