using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Help;
using Yurowm.ObjectEditors;

namespace Yurowm.Localizations {
    public class LocalizedTextEditor : ObjectEditor<LocalizedText> {
        public override void OnGUI(LocalizedText text, object context = null) {
            text.localized = EditorGUILayout.Toggle("Localized", text.localized);
            EditorTips.PopLastRectByID("localization.localized");

            if (text.localized) {
                using (GUIHelper.Horizontal.Start()) {
                    text.key = EditorGUILayout.TextField("Localization Key", text.key);
                    EditorTips.PopLastRectByID("localization.key");
                    
                    if (GUILayout.Button("<", EditorStyles.miniButton, GUILayout.Width(18))) {
                        var menu = new GenericMenu();
                        LocalizationEditor
                            .GetKeyList()
                            .ForEach(k => menu.AddItem(new GUIContent(k), false, func: () => text.key = k));    
                        if (menu.GetItemCount() > 0) 
                            menu.ShowAsContext();
                        GUI.FocusControl("");
                    }
                    EditorTips.PopLastRectByID("localization.keyset");
                }
                
                if (!text.key.IsNullOrEmpty()) 
                    LocalizationEditor.EditOutside(text.key, true);
                EditorTips.PopLastRectByID("localization.value");
            } else {
                text.text = EditorGUILayout.TextArea(text.text);
                EditorTips.PopLastRectByID("localization.text");
            }
        }
    }
}