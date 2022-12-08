using System;
using System.Collections;
using System.Linq;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class ChipMix : LevelContent {
        public Slot slot;
        
        public string effectName;
        
        public override Type GetContentBaseType() {
            return typeof(ChipMix);
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            BaseLogic().Run(field.coroutine);
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            mixedChips?.ForEach(c => c.Kill());
        }

        IEnumerator BaseLogic() {
            if (mixedChips == null || mixedChips.Length != 2) {
                Kill();
                yield break;
            }
            
            Prepare(mixedChips[0], mixedChips[1]);
            
            yield return gameplay.WaitForTask<MatchingTask>();
            
            using (MatchingPool.Get(context).Use()) {
                if (!effectName.IsNullOrEmpty()) {
                    var effect = Effect.Emit(field, effectName, position, 
                        GetEffectCallbacks()
                            .Collect<IEffectCallback>()
                            .ToArray());
                    
                    if (effect)
                        OnCreateEffect(effect);
                }
                
                yield return Logic();
             
                Kill();
            }
        }
        
        public virtual void OnCreateEffect(Effect effect) {}
        public virtual IEnumerable GetEffectCallbacks() {
            yield return new RepaintEffectLogicProvider.Callback {
                colorInfo = colorInfo
            };
        }

        public abstract IEnumerator Logic();
        
        Chip[] mixedChips;
        
        public void SetChips(Chip centerChip, Chip secondChip) {
            mixedChips = new [] {
                centerChip,
                secondChip
            };
        }
        
        protected abstract void Prepare(Chip centerChip, Chip secondChip);

        #region Color
        
        protected ItemColorInfo colorInfo = ItemColorInfo.None;
        
        protected void ApplyColor(SlotContent first, SlotContent second) {
            if (first.colorInfo.IsKnown()) {
                colorInfo = first.colorInfo;
                return;
            }
            
            if (second.colorInfo.IsKnown()) {
                colorInfo = second.colorInfo;
                return;
            }
            
            colorInfo = ItemColorInfo.Unknown;
        }

        #endregion
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("effectName", effectName);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("effectName", ref effectName);
        }

        #endregion
    }
    
    [SerializeShort]
    public class ChipMixRecipe : ISerializable {
        public string firstChip;
        public string secondChip;
        public string mix;
        public Pair<string> pair => new Pair<string>(firstChip, secondChip);

        public override bool Equals(object obj) {
            if (obj is ChipMixRecipe m)
                return pair == m.pair;
                    
            return false;
        }
        
        public bool Equals(string a, string b) {
            if (firstChip == a && secondChip == b) return true;
            if (firstChip == b && secondChip == a) return true;
                    
            return false;
        }

        public override int GetHashCode() {
            return pair.GetHashCode();
        }

        #region ISerializable

        public void Serialize(Writer writer) {
            writer.Write("first", firstChip);
            writer.Write("second", secondChip);
            writer.Write("mix", mix);
        }

        public void Deserialize(Reader reader) {
            reader.Read("first", ref firstChip);
            reader.Read("second", ref secondChip);
            reader.Read("mix", ref mix);
        }

        #endregion
    }
}