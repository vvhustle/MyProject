using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public interface IDestroyable {
        IEnumerator Destroying();
        void MarkAsDestroyed();
        
        int scoreReward { get; set; }
        bool destroying { get; }
        string destroyingEffect { get; set; }
    }
    
    public interface ILayered : IDestroyable {
        int layersCount { get; set; }
        int layer { get; set; }
        int layerScoreReward { get; set; }
        string layerDownEffect { get; set; }
        void OnChangeLayer(int layer);
    }
    
    public interface INearHitDestroyable : IDestroyable {}
    
    public class LayeredVariable : ContentInfoVariable  {
        public int count = 1;
        
        public LayeredVariable() {}
    
        public override void Serialize(Writer writer) {
            writer.Write("layers", count);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("layers", ref count);
        }
    }
}