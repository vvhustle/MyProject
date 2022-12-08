using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public abstract class FieldBoosterSetBase : LevelExtension {
        public abstract bool Filter(FieldBooster booster);
    }
    
    public class FieldBoosterSetAll : FieldBoosterSetBase {
        public override bool Filter(FieldBooster booster) {
            return true;
        }
    }
    
    public class FieldBoosterSet : FieldBoosterSetBase {
        
        string[] boosterIDs;
        
        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(BoosterSetVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            if (variable  is BoosterSetVariable bsv)
                boosterIDs = bsv.IDs.ToArray();
        }

        public override bool Filter(FieldBooster booster) {
            return boosterIDs.Contains(booster.ID);
        }
    }
}