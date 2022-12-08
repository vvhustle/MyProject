using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ObjectEditors;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class LinedBombEditor : ObjectEditor<LinedBomb> {
        public override void OnGUI(LinedBomb bomb, object context = null) {
            Edit("Hit", bomb.hit);
        }
    }
    
    public class LinedBombHitEditor : ObjectEditor<LinedBombHit> {
        public override void OnGUI(LinedBombHit hit, object context = null) {
            hit.waveOffset = EditorGUILayout.IntSlider("Wave Offset", hit.waveOffset, 0, 2);
            
            hit.side = (Side) EditorGUILayout.EnumFlagsField("Sides", hit.side);
            
            Edit("Explosion", hit.explosion);
        }
    }
    
    public class ILineBreakerEditor : ObjectEditor<ILineBreaker> {
        public override void OnGUI(ILineBreaker breaker, object context = null) {
            if (breaker is SlotContent slotContent)
                slotContent.breakLines = EditorGUILayout.Toggle("Break Lines", slotContent.breakLines);
        }
    }
}