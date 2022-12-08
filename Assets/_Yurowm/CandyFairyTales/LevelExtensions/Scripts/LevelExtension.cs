using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YMatchThree.Core {
    public abstract class LevelExtension : LevelContent {
        public virtual bool IsUnique { get; } = true;
        
        
        public override Type GetContentBaseType() {
            return typeof(LevelExtension);
        }
    }
    
    public abstract class LevelScriptExtension : LevelContent {
        public virtual bool IsUnique { get; } = true;
        
        public override Type GetContentBaseType() {
            return typeof(LevelExtension);
        }
    }
}