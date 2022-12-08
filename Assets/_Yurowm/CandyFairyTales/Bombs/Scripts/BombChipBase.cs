using System.Collections;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public abstract class BombChipBase : Chip, IDestroyable {
        
        public BombActivationType activationType = BombActivationType.Default;
        public string overlayID;

        public int scoreReward { get; set; }
        
        public abstract IEnumerator Exploding();

        public IEnumerator Destroying() {
            yield return Exploding();
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("overlay", overlayID);
            writer.Write("activationType", activationType);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader); 
            reader.Read("overlay", ref overlayID); 
            reader.Read("activationType", ref activationType); 
        }

        #endregion

    }
    
    public enum BombActivationType {
        Default = 0,
        DoubleTap = 1 << 0,
        Swap = 1 << 1
    }
}