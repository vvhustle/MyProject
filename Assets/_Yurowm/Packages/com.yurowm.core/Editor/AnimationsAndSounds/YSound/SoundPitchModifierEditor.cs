using UnityEditor;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;
using Yurowm.Sounds;

namespace Yurowm.Editors {
    public class SoundPitchModifierEditor : ObjectEditor<SoundPitchModifier> {
        public override void OnGUI(SoundPitchModifier modifier, object context = null) {
            EditorGUILayout.MinMaxSlider("Pitch", ref modifier.min, ref modifier.max, .3f, 2f);
            using (GUIHelper.IndentLevel.Start()) {
                modifier.min = EditorGUILayout.FloatField("Min", modifier.min).Round(.01f);
                modifier.max = EditorGUILayout.FloatField("Max", modifier.max).Round(.01f);
            }
        }
    }
}