using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.Sounds {
    public abstract class SoundEffect : SoundBase {
        
        public List<SoundModifier> modifiers = new List<SoundModifier>();
        
        protected abstract string GetSoundPath();
        
        public override void Play() {
            var path = GetSoundPath();
            
            var logic = SoundController.GetClip(path, c => {
                if (modifiers.IsEmpty())
                    SoundController.PlayEffect(c);
                else {
                    SoundController.PlayEffectSpecialSource(c, out var source);
                    modifiers.ForEach(m => m.Apply(source));
                }
            });
                
            CoroutineCore coroutine = null;
            
            #if UNITY_EDITOR
            coroutine = EditorCoroutine.GetCore();
            #endif
            
            logic.Run(coroutine);
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("modifiers", modifiers.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            modifiers = reader.ReadCollection<SoundModifier>("modifiers").ToList();
        }
    }
}