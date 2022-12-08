using System;
using YMatchThree.Meta;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Core;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.UI;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    [SerializeShort]
    public class LevelScriptOrdered : LevelScriptBase {
        
        public int order = 0;
        
        public string worldName = "";
        public string path = "";

        public override string LocalizationPath => worldName + "/" + base.LocalizationPath;
        
        public override string ToString() {
            return $"{order}. {base.ToString()}";
        }
        
        public void OnSelect() {
            Behaviour.GetAllByID<LabelFormat>("LevelNumber")
                .ForEach(l => l["value"] = order.ToString());
            
            var worldPropgress = PlayerData.levelProgress.GetWorldPropgress(worldName, true);
            var bestScore = worldPropgress.GetBestScore(ID);  
            
            Behaviour.GetAllByID<LabelFormat>("Score")
                .ForEach(l => l["value"] = bestScore.ToString());
        }
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            writer.Write("order", order);
            writer.Write("world", worldName);
            writer.Write("path", path);

            base.Serialize(writer);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("order", ref order);
            reader.Read("world", ref worldName);
            
            #if UNITY_EDITOR
            reader.Read("path", ref path);
            #endif
            
            base.Deserialize(reader);
        }

        #endregion
    }
}