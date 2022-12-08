using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.HierarchyLists;
using Yurowm.Icons;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.YDebug;

namespace YMatchThree.Editor {
    public class LevelScriptBaseParametersEditor : ObjectEditor<LevelScriptBase> {
        
        GUIHelper.Spoiler colorsSpoiler = new GUIHelper.Spoiler(true);
        
        YList<ContentInfo> extensionsList;

        public LevelScriptBaseParametersEditor() {
            var extensionIcon = EditorIcons.GetIcon("ExtensionListIcon");
            
            extensionsList = new YList<ContentInfo> {
                getName = e => e.Reference.ID,
                getIcon = e => extensionIcon,
                onContextMenu = OnListContextMenu
            };
        }
        
        public override void OnGUI(LevelScriptBase script, object context = null) {
            EditorGUILayout.LabelField("ID", script.ID);
            script.name = EditorGUILayout.TextField("Name", script.name);
            script.localizationKey = EditorGUILayout.TextField("Localization Key", script.localizationKey);
            Edit(script.stars);
            
            BaseTypesEditor.SelectAsset<SlotContentBody>("Chip Body", script, nameof(script.chipBody));
            if (script.chipBody.IsNullOrEmpty())
                EditorGUILayout.HelpBox("The default chips body will be used", MessageType.Info, false);
            
            
            const string suffix = "ColorSettings";
            using (colorsSpoiler.Start("Colors")) {
                if (colorsSpoiler.IsVisible()) {
                
                    var colorSettingsTypes = LevelEditorController.colorSettingsTypes;
                    
                    if (script.colorSettings == null) {
                        if (colorSettingsTypes.Length == 0)
                            return;
                        script.colorSettings = (LevelColorSettings) Activator.CreateInstance(colorSettingsTypes.FirstOrDefault());
                    }
                        
                    if (GUIHelper.Button("Color Type", script.colorSettings.GetType().Name.NameFormat(null, suffix, true), EditorStyles.popup)) {
                        var menu = new GenericMenu();

                        var currentType = script.colorSettings.GetType();
                        
                        foreach (var type in colorSettingsTypes) {
                            if (type == currentType)
                                menu.AddItem(new GUIContent(type.Name.NameFormat(null, suffix, true)), true, () => {});
                            else {
                                var t = type;
                                menu.AddItem(new GUIContent(type.Name.NameFormat(null, suffix, true)), false, () => {
                                    script.colorSettings = (LevelColorSettings) Activator.CreateInstance(t);
                                });
                            }
                        }
                        
                        if (menu.GetItemCount() > 0)
                            menu.ShowAsContext();
                    }
                    
                    Edit(script.colorSettings, context);
                }
            }    
            
            extensionsList.OnGUILayout("Extensions", script.extensions);
            using (GUIHelper.IndentLevel.Start())
                Edit(extensionsList.GetSelected(script.extensions).ToArray());
        }
        
        void OnListContextMenu(GenericMenu menu, List<ContentInfo> elements) {
            foreach (var extension in LevelContent.storage.Items<LevelScriptExtension>()) {
                if (extension.IsUnique && elements.Any(e => e.Reference == extension))
                    continue;

                var lse = extension;
                menu.AddItem(new GUIContent($"New/{extension.ID}"), false, () => {
                    elements.Add(new ContentInfo(lse));
                    extensionsList.SetDirty();
                });
            }
        }
    }
    
    public interface ILevelScriptPreset {
        string GetName();
        LevelScriptOrdered Emit(LevelEditorContext context);
    }
}