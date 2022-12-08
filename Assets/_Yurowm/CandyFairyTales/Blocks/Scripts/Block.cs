using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {

    [BaseContentOrder(0)]
    public abstract class Block : SlotContent {
        
        public BlockingType blockingType = BlockingType.Obstacle;
        
        public enum BlockingType {
            Obstacle = 0,
            Hole = 1
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            if (!(this is IChipContainerBlock)) {
                slotModule.GetContent<Chip>().ForEach(chip => chip.Kill());
            }
        }

        public override Type GetContentBaseType() {
            return typeof(Block);
        }

        public override bool IsUniqueContent() {
            return true;
        }

        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("blockingType", blockingType);
            
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("blockingType", ref blockingType);
        }

        #endregion
    }
    
    public interface IChipContainerBlock {}
}