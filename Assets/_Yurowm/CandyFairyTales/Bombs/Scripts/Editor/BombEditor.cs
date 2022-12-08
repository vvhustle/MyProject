using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;
using Yurowm.Help;
using Yurowm.Utilities;

namespace Yurowm.Editors {
    public class BombEditor : ObjectEditor<Bomb> {
        public override void OnGUI(Bomb bomb, object context = null) {
            Edit("Hit", bomb.hit);
            EditorTips.PopLastRectByID("lc.bomb.hit");
        }
    }
    
    public class BombHitEditor : ObjectEditor<BombHit> {
        public override void OnGUI(BombHit hit, object context = null) {
            hit.distance = EditorGUILayout.IntField("Distance", hit.distance).ClampMin(1);
            EditorTips.PopLastRectByID("bombhit.distance");
            
            hit.distanceType = (int2.DistanceType) EditorGUILayout.EnumPopup("Distance Type", hit.distanceType);
            EditorTips.PopLastRectByID("bombhit.distanceType");
            
            Edit("Explosion", hit.explosion);
            EditorTips.PopLastRectByID("bombhit.explosion");
        }
    }
}