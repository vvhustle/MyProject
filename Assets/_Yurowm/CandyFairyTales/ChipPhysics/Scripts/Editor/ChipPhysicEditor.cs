using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class ChipPhysicEditor : ObjectEditor<ChipPhysic> {
        public override void OnGUI(ChipPhysic physic, object context = null) {
            physic.acceleration = EditorGUILayout.FloatField("Acceleration", physic.acceleration).ClampMin(.1f);
            EditorTips.PopLastRectByID("lc.physic.acceleration");
            
            physic.speedInitial = EditorGUILayout.FloatField("Speed Initial", physic.speedInitial).ClampMin(0);
            EditorTips.PopLastRectByID("lc.physic.speedinitial");
            
            physic.speedMax = EditorGUILayout.FloatField("Speed Max", physic.speedMax).ClampMin(physic.speedInitial);
            EditorTips.PopLastRectByID("lc.physic.speedmax");
            
            physic.bouncing = EditorGUILayout.Toggle("Bouncing", physic.bouncing);
            EditorTips.PopLastRectByID("lc.physic.bouncing");
            if (physic.bouncing) {
                physic.impulsMin = EditorGUILayout.FloatField("Impuls Min", physic.impulsMin).ClampMin(0.1f);
                EditorTips.PopLastRectByID("lc.physic.impulsmin");
                
                physic.impulsMax = EditorGUILayout.FloatField("Impuls Max", physic.impulsMax).ClampMin(0.1f);
                EditorTips.PopLastRectByID("lc.physic.impulsmax");
                
                Edit("Land Bouncing", physic.landBouncing);
                EditorTips.PopLastRectByID("lc.physic.land");
            }
        }
    }
}