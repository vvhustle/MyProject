using System;
using System.IO;
using System.Linq;
using UnityEngine;
using YMatchThree.Editor;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class LevelScriptOrderedFile : LevelScriptFile<LevelScriptOrdered> {

        public int Order {
            get => Script.order;
            set {
                #if UNITY_EDITOR
                if (Script.order != value) {
                    Script.order = value;
                    Save();
                }
                #endif
            }
        }
        
        public LevelScriptOrderedFile(FileInfo file) : base(file) { }

        public LevelScriptOrderedFile(LevelScriptOrdered script, string key) : 
            base(script, key) {}

        public string World {
            get {
                if (!IsLoaded()) Load(true);
                return script.worldName;
            }
            set {
                if (!IsLoaded() || script.worldName != value) {
                    Load(false);
                    if (script.worldName != value) {
                        script.worldName = value;
                        Save();
                    }
                }
            }
        }
    }
}