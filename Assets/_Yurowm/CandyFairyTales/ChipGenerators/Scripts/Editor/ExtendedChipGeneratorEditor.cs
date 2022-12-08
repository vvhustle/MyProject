using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;
using ECGCase = YMatchThree.Core.ExtendedChipGenerator.ECGCase;

namespace YMatchThree.Editor {
    public class ExtendedChipGeneratorEditor : ContentSelectionEditor<ExtendedChipGenerator> {
        static readonly Color removeColor = new Color(1, .5f, .5f);
        static readonly Color addColor = new Color(.5f, 1, 1);
        
        ECGCasesVariable buffer;
        ECGCase currentCase;
        ContentInfo[] selectionToEdit = new ContentInfo[1];
        
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {
            
            if (selection.Length == 0) return;
            
            var current = selection.First();
            
            using (GUIHelper.Horizontal.Start()) {
                if (selection.Length == 1) {
                    using (GUIHelper.Color.Start(addColor))
                        if (GUILayout.Button("ADD...", EditorStyles.miniButton, GUILayout.Width(50))) {
                            GenericMenu menu = new GenericMenu();
                            foreach (var chip in LevelContent.storage.Items<Chip>()) {
                                var c = chip;
                                menu.AddItem(new GUIContent(chip.ID), false, () => {
                                    current.GetVariable<ECGCasesVariable>()?.value.Add(
                                        new ECGCase {
                                            info = new ContentInfo(c)
                                        });
                                });
                            }
                            if (menu.GetItemCount() > 0)
                                menu.ShowAsContext();
                        }
                }
                GUILayout.FlexibleSpace();

                using (GUIHelper.Lock.Start(selection.Length > 1))
                    if (GUILayout.Button("COPY", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
                        buffer = current.GetVariable<ECGCasesVariable>();
                
                using (GUIHelper.Lock.Start(buffer == null))
                    if (GUILayout.Button("PASTE", EditorStyles.miniButtonRight, GUILayout.Width(50)))
                        selection.ForEach(i => i.GetVariable<ECGCasesVariable>().value = buffer.value.Select(x => x.Clone()).ToList());
            }

            if (selection.Length > 1) {
                GUILayout.Label("Multi-object editing is not supported");
                return;
            }
            
            var variable = current.GetVariable<ECGCasesVariable>();
            
            float totalWeight = variable.value.Sum(c => c.weight);
            
            foreach (var c in variable.value) {

                using (GUIHelper.Horizontal.Start()) {
                    using (GUIHelper.Color.Start(removeColor))
                        if (GUILayout.Button("X", EditorStyles.miniButtonLeft, GUILayout.Width(20))) {
                            variable.value.Remove(c);
                            GUI.FocusControl("");
                        }

                    if (GUILayout.Button($"{c.info.Reference.ID} ({(100f * c.weight / totalWeight):F1}%)", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true))) {
                        currentCase = c;
                        selectionToEdit[0] = c.info;
                        GUI.FocusControl("");
                    }
                }

                if (currentCase == c) {
                    c.weight = Mathf.Max(1, EditorGUILayout.FloatField("Weight", c.weight));
                    Edit(selectionToEdit, fieldEditor);
                }
            }
        }
    }
}