using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Serialization;

namespace Yurowm.Sounds {
    public abstract class SoundModifier : ISerializable {
        public virtual void Serialize(Writer writer) { }

        public virtual void Deserialize(Reader reader) { }
        public abstract void Apply(AudioSource source);
    }
}