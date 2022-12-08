using System;
using UnityEditor;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Utilities;
using Object = UnityEngine.Object;

namespace Yurowm.UI {
    [CustomEditor(typeof(UIOrientationRect))]
    public class UIOrientationRectEditor : Editor {
        
        
        [MenuItem("Yurowm/Tools/Apply all rects for current orientation")]
        static void ApplyAll() {
            FindObjectsOfType<UIOrientationBase>(true)
                .ForEach(uib => uib.OnScreenResize());
        }
        

        UIOrientationRect orientationRect;
        
        UIOrientationBase.Orientation orientation = UIOrientationBase.Orientation.Landscape;
        
        void OnEnable() {
            orientationRect = target as UIOrientationRect;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            foreach (var rect in orientationRect.rects) {
                using (GUIHelper.Horizontal.Start()) {
                    EditorGUILayout.PrefixLabel(rect.orientation.ToText());
                    
                    if (GUILayout.Button("Apply", EditorStyles.miniButton)) 
                        rect.Apply(orientationRect.rectTransform);

                    if (GUILayout.Button("Rewrite", EditorStyles.miniButton)) 
                        rect.Write(orientationRect.rectTransform);

                    if (GUILayout.Button("Remove", EditorStyles.miniButton)) {
                        orientationRect.rects.Remove(rect);
                        break;
                    }
                }
            }
                
            using (GUIHelper.Horizontal.Start()) {
                orientation = (UIOrientationBase.Orientation) EditorGUILayout
                    .EnumFlagsField(orientation, GUILayout.Width(EditorGUIUtility.labelWidth));
                using (GUIHelper.Lock.Start(orientation == 0)) 
                    if (GUILayout.Button("Write", EditorStyles.miniButton)) {
                        var rect = new UIOrientationRect.Rect();
                        rect.orientation = orientation;
                        rect.Write(orientationRect.rectTransform);
                        orientationRect.rects.Add(rect);
                    }
            }
            if (GUIHelper.Button(null, "Apply Actual", EditorStyles.miniButton)) 
                orientationRect.OnScreenResize();

        }

    }
}