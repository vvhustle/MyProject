using UnityEditor;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;
using Yurowm.Sounds;

namespace Yurowm.Editors {
    public class SoundVolumeModifierEditor : ObjectEditor<SoundVolumeModifier> {
        public override void OnGUI(SoundVolumeModifier modifier, object context = null) {
            EditorGUILayout.MinMaxSlider("Volume", ref modifier.min, ref modifier.max, 0f, 1f);
            using (GUIHelper.IndentLevel.Start()) {
                modifier.min = EditorGUILayout.FloatField("Min", modifier.min).Round(.01f);
                modifier.max = EditorGUILayout.FloatField("Max", modifier.max).Round(.01f);
            }
        }
    }
}