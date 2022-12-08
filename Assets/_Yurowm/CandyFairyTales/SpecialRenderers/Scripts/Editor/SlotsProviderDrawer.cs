using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Utilities;

namespace YMatchThree.Editors {
    [CustomPropertyDrawer(typeof(SlotRenderer.SlotsProvider))]
    public class SlotsProviderDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position = EditorGUI.PrefixLabel(position, label);
            
            area a = new area(int2.one, new int2(10, 10));

            var target = property.serializedObject.targetObject as SlotRenderer;
            
            Undo.RecordObject(target, null);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return 150;
        }
    }
}
