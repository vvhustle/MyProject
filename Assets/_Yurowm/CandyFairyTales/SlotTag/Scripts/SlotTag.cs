using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    [SlotTag(ConsoleColor.Yellow, ConsoleColor.DarkRed)]
    public class SlotTag : SlotModifier {
        
        string[] tags;

        public bool HasTag(string tag) {
            if (tags == null) return false;
            return tags.Contains(tag);
        }

        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof (TagsVariable);
        }
        
        static readonly char[] tagSeparators = {';', ','};
        
        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case TagsVariable tv: {
                    tags = tv.tags.Split(tagSeparators)
                            .Select(t => t.Trim())
                            .ToArray();
                    return;
                }
            }
        }
    }
    
    public class TagsVariable : ContentInfoVariable {
        
        public string tags = "";
        
        public override void Serialize(Writer writer) {
            writer.Write("tags", tags);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("tags", ref tags);
        }
    }
}