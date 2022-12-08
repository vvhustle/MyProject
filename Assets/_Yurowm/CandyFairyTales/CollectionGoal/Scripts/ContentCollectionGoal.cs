using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class ContentCollectionGoal : CollectionGoal {
        public string contentID;

        public override bool IsUnique => false;

        Sprite icon;
        
        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(ContentIDVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            if (variable is ContentIDVariable c) {
                contentID = c.id ?? storage.GetItem<SlotContent>(c => c is IDestroyable)?.ID;
                var iconName = storage.GetItem<SlotContent>(ContentFilter)?.miniIcon;
                if (!iconName.IsNullOrEmpty())
                    icon = AssetManager.GetAsset<Sprite>(iconName);
                return;
            }
            base.SetupVariable(variable);
        }

        protected override bool ContentFilter(SlotContent content) {
            return content.ID == contentID;
        }
        
        #region ISerializable

        public override Sprite GetIcon() {
            return icon;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("contentID", contentID);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("contentID", ref contentID);
        }

        #endregion
    }
    
    public class ContentIDVariable : ContentInfoVariable {
        public string id;
        
        public override void Serialize(Writer writer) {
            writer.Write("id", id);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("id", ref id);
        }
    }
}