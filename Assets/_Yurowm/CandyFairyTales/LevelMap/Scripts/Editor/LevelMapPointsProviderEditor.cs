using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;

namespace YMatchThree.Editor {
	[CustomEditor(typeof(LevelMapPointsProvider), true)]
    public class LevelMapPointsProviderEditor : UnityEditor.Editor {
        
        SerializedProperty property_levelPoints;
        LevelMapPointsProvider provider;
        
        void OnEnable() {
	        provider = target as LevelMapPointsProvider;
            property_levelPoints = serializedObject.FindProperty("points");
        }

        void OnSceneGUI() {

	        if (property_levelPoints.arraySize == 0) {
                property_levelPoints.InsertArrayElementAtIndex(0);
				ApplyChanges();   
            }
            
            
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(
	            Enumerator.For(0, property_levelPoints.arraySize - 1, 1)
		            .Select(i =>  property_levelPoints.GetArrayElementAtIndex(i).vector2Value)
		            .Select(p => provider.TransformPoint(p))
		            .ToArray());

            using (GUIHelper.Change.Start(ApplyChanges)) {

                if (Event.current.control) {
	                DrawRemovePointPosition(ref property_levelPoints, 1);
                } else {
                    for (int i = 0; i < property_levelPoints.arraySize; i++) {
                        var position = property_levelPoints.GetArrayElementAtIndex(i);
                        DrawUpdatePointPosition(ref position, (i + 1).ToString());
                    }
                    
                    if (Event.current.shift)
	                    DrawInbetweenButtons(ref property_levelPoints);
                }
            }
        }
        
        
        void ApplyChanges() {
	        serializedObject.ApplyModifiedProperties();
        }
        
        void DrawRemovePointPosition(
            ref SerializedProperty positions,
            int minPoints) {
	        
            for (int i = 0; i < positions.arraySize; i++) {
                var element = positions.GetArrayElementAtIndex(i);
				
                var worldPosition = provider.TransformPoint(element.vector2Value);

                float handleSize = HandleUtility.GetHandleSize(worldPosition) * 0.1f;

                if (!Handles.Button(worldPosition, Quaternion.identity, handleSize, handleSize,
                        DrawRemovePointHandle) || positions.arraySize <= minPoints) continue;
				
                // shift other points
                for (int j = i; j < positions.arraySize - 1; j++)
                    positions.GetArrayElementAtIndex(j).vector2Value =
                        positions.GetArrayElementAtIndex(j + 1).vector2Value;

                positions.DeleteArrayElementAtIndex(positions.arraySize - 1);

                GUI.changed = true;
            }
        }
        
        void DrawUpdatePointPosition(ref SerializedProperty position, string label) {
	        var worldPosition = provider.TransformPoint(position.vector2Value);

	        float size = HandleUtility.GetHandleSize(worldPosition);
	        
            var draggedPosition = provider.InverseTransformPoint(
                Handles.FreeMoveHandle(
                    worldPosition,
                    Quaternion.identity,
                    size * .1f,
                    Vector3.zero,
                    DrawPointHandle
                )
            );
            
            if (!label.IsNullOrEmpty())
				Handles.Label(worldPosition + new Vector3(0, size * .4f, 0), label);

            if (draggedPosition != position.vector2Value)
	            position.vector2Value = draggedPosition;
        }
        
        void DrawInbetweenButtons(ref SerializedProperty positions) {
	        
			Handles.color = Color.red;

			for (int i = positions.arraySize - 2; i >= 0; i--) {

				var element = positions.GetArrayElementAtIndex(i);
				var elementNext = positions.GetArrayElementAtIndex(i + 1);
				
				Vector3 worldPosition = (element.vector2Value + elementNext.vector2Value) * 0.5f;

				worldPosition = provider.TransformPoint(worldPosition);

				var handleSize = HandleUtility.GetHandleSize(worldPosition) * 0.08f;

				if (!Handles.Button(worldPosition, Quaternion.identity, handleSize, handleSize,
					DrawAddPointHandle)) continue;
				
				positions.InsertArrayElementAtIndex(positions.arraySize);

				// shift other points
				for (int j = positions.arraySize - 1; j > i; j--)
					positions.GetArrayElementAtIndex(j).vector2Value =
						positions.GetArrayElementAtIndex(j - 1).vector2Value;

				positions.GetArrayElementAtIndex(i + 1).vector2Value = 
					provider.InverseTransformPoint(worldPosition);

				GUI.changed = true;
			}
        }
        
        static void DrawPointHandle(int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType){
	        Handles.color = Color.black;

	        Handles.DrawSolidDisc(position, Vector3.forward, size * 1.4f);

	        Handles.color = Color.white;
	        Handles.DrawSolidDisc(position, Vector3.forward, size);
	        Handles.CircleHandleCap(controlId, position, rotation, size, eventType);

	        Handles.color = Color.black;
	        Handles.DrawSolidDisc(position, Vector3.forward, size * 0.8f);
        }

        static void DrawRemovePointHandle(int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType){
	        Handles.color = Color.black;

	        Handles.DrawSolidDisc(position, Vector3.forward, size * 1.4f);
	        Handles.CircleHandleCap(controlId, position, rotation,  size * 1.4f, eventType);

	        Handles.color = Color.red;
	        Handles.DrawSolidDisc(position, Vector3.forward, size);

	        Handles.color = Color.black;
	        Handles.DrawSolidDisc(position, Vector3.forward, size * 0.8f);
        }

        static void DrawAddPointHandle(int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType){
	        Handles.color = Color.black;
	        Handles.CircleHandleCap(controlId, position, rotation, size, eventType);
	        Handles.DrawSolidDisc(position, Vector3.forward, size);

	        Handles.color = Color.white;
	        Handles.DrawSolidDisc(position, Vector3.forward, size * 0.2f);
        }
    }
}