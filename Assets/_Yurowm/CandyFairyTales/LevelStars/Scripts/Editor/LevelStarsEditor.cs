using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class LevelStarsEditor : ObjectEditor<LevelStars> {
        public override void OnGUI(LevelStars stars, object context = null) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent("Stars"));
            rect.xMin = rect2.x;

            Rect fieldRect = new Rect(rect);
            fieldRect.width /= 3;

            stars.First = Mathf.Max(EditorGUI.IntField(fieldRect, stars.First), 1);
            fieldRect.x += fieldRect.width;

            stars.Second = Mathf.Max(EditorGUI.IntField(fieldRect, stars.Second), stars.First + 1);
            fieldRect.x += fieldRect.width;

            stars.Third = Mathf.Max(EditorGUI.IntField(fieldRect, stars.Third), stars.Second + 1);
        }
    }
}