using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class LayoutBlock : Block {

        SlotRenderer renderer;
        SlotsBody slotsBody;

        public string slotRenderer = "";

        public override SpaceObject EmitBody() {
            return null;
        }

        public override void OnChangeSlot() {
            if (!slotsBody) 
                slotsBody = context.Get<SlotsBody>(sr => sr.name == slotRenderer);
            
            if (!slotsBody) {
                slotsBody = AssetManager.Emit<SlotsBody>(slotRenderer);
                context.Add(slotsBody);
                slotsBody.transform.SetParent(field.root);
                slotsBody.transform.Reset();
            }
            
            slotsBody?.slotRenderer.AddPoints(slotModule.Slots().Select(s => s.coordinate));
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("slotRenderer", slotRenderer);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("slotRenderer", ref slotRenderer);
        }

        #endregion
    }
}