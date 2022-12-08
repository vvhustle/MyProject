using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class ChipPhysic : LevelContent {
        public float acceleration = 20f;
        public float speedMax = 17f;
        public float speedInitial = 4f;
        
        public bool bouncing = false;
        public float impulsMin = .5f;
        public float impulsMax = 6;
        
        public const float offsetThreshold = .1f;
        
        public ExplosionParameters landBouncing = new ExplosionParameters();

        public abstract Slot GetFallingSlot(Slot slot);
        
        public virtual void OnStartGravity() {}

        public override Type BodyType => null;

        public override Type GetContentBaseType() {
            return typeof(ChipPhysic);
        }

        public static bool IsAvailableForFalling(Slot slot) {
            return slot && !slot.HasBaseContent<Chip>() && !slot.HasBaseContent<Block>();
        }      
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("acceleration", acceleration);
            writer.Write("speedMax", speedMax);
            writer.Write("speedInitial", speedInitial);
            
            writer.Write("bouncing", bouncing);
            if (bouncing) {
                writer.Write("impulsMin", impulsMin);
                writer.Write("impulsMax", impulsMax);
                
                writer.Write("landBouncing", landBouncing);
            }
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("acceleration", ref acceleration);
            reader.Read("speedMax", ref speedMax);
            reader.Read("speedInitial", ref speedInitial);
            
            reader.Read("bouncing", ref bouncing);
            if (bouncing) {
                reader.Read("impulsMin", ref impulsMin);
                reader.Read("impulsMax", ref impulsMax);
                
                reader.Read("landBouncing", ref landBouncing);
            }
        }

        #endregion
    }
}