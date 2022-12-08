using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public abstract class LevelBoosterSetBase : LevelScriptExtension {
        public abstract bool Filter(LevelBooster booster);
    }
    
    public class LevelBoosterSetAll : LevelBoosterSetBase {
        public override bool Filter(LevelBooster booster) {
            return true;
        }
    }
    
    public class LevelBoosterSet : LevelBoosterSetBase {
        
        string[] boosterIDs;
        
        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(BoosterSetVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            if (variable  is BoosterSetVariable  bsv)
                boosterIDs = bsv.IDs.ToArray();
        }

        public override bool Filter(LevelBooster booster) {
            return boosterIDs.Contains(booster.ID);
        }
    }
}