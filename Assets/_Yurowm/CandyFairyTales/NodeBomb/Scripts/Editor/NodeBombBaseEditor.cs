using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Effects;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class NodeBombBaseEditor : ObjectEditor<NodeBombBase> {
        public override void OnGUI(NodeBombBase bomb, object context = null) {
            Edit("Hit", bomb.hit);
        }
    }
    
    public class NodeBombHitEditor: ObjectEditor<NodeBombHit> {
        
        public override void OnGUI(NodeBombHit hit, object context = null) {
            BaseTypesEditor.SelectAsset<EffectBody>(hit, nameof(hit.nodeEffect));

            Edit("Explosion", hit.explosion);
        }
    }
}