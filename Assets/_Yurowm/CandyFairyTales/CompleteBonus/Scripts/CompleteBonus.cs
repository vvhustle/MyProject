using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Spaces;

namespace YMatchThree.Core {
    public abstract class CompleteBonus : LevelContent, ILocalized {
        public LocalizedText feedback = new LocalizedText();

        public override Type GetContentBaseType() {
            return typeof(CompleteBonus);
        }

        public abstract bool IsComplete();
        public abstract IEnumerator Logic();

        public virtual IEnumerable GetLocalizationKeys() {
            yield return feedback;
        }
        
        public int GetBonusUnits(int max) {
            int result = 0;
            
            foreach (var limitation in space.context.GetAll<LevelLimitation>())
                result = result.ClampMin(limitation.GetUnits(max));
            
            return result;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("feedback", feedback);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("feedback", ref feedback);
        }
    }
}