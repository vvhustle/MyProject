using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class ContentInfoVariable : ISerializable {
        public abstract void Serialize(Writer writer);

        public abstract void Deserialize(Reader reader);
    }
    
    public class CountVariable : ContentInfoVariable {
        public int value = 1;
        
        public override void Serialize(Writer writer) {
            writer.Write("count", value);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("count", ref value);
        }
    }
    
    public interface IFieldSensitiveVariable {
        void MoveSlot(int2 from, int2 to);
        void RemoveSlot(int2 from);
    }
}