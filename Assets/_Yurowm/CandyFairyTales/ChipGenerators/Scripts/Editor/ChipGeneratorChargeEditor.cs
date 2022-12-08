using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    public class ChipGeneratorChargeEditor : ContentSelectionEditor<ChipGeneratorCharge> {
        
        static readonly Color removeColor = new Color(1, .5f, .5f);
        static readonly Color addColor = new Color(.5f, 1, 1);
        
        ChipGeneratorChargeVariable buffer;
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
                                    current.GetVariable<ChipGeneratorChargeVariable>()?.chips
                                        .Add(new ContentInfo(c));
                                });
                            }
                            if (menu.GetItemCount() > 0)
                                menu.ShowAsContext();
                        }
                }
                GUILayout.FlexibleSpace();

                using (GUIHelper.Lock.Start(selection.Length > 1))
                    if (GUILayout.Button("COPY", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
                        buffer = current.GetVariable<ChipGeneratorChargeVariable>();
                
                using (GUIHelper.Lock.Start(buffer == null))
                    if (GUILayout.Button("PASTE", EditorStyles.miniButtonRight, GUILayout.Width(50)))
                        selection.ForEach(i => i.GetVariable<ChipGeneratorChargeVariable>().chips = buffer.chips.Select(x => x.Clone())
                            .ToList());
            }

            if (selection.Length > 1) {
                GUILayout.Label("Multi-object editing is not supported");
                return;
            }
            
            var variable = current.GetVariable<ChipGeneratorChargeVariable>();
            
            for (int i = 0; i < variable.chips.Count; i++) {
                var c = variable.chips[i];
                
                using (GUIHelper.Horizontal.Start()) {
                    using (GUIHelper.Color.Start(removeColor))
                        if (GUILayout.Button("X", EditorStyles.miniButtonLeft, GUILayout.Width(20))) {
                            variable.chips.Remove(c);
                            GUI.FocusControl("");
                        }
                    
                    using (GUIHelper.Lock.Start(i >= variable.chips.Count - 1))
                        if (GUILayout.Button("v", EditorStyles.miniButtonMid, GUILayout.Width(20))) {
                            variable.chips.Remove(c);
                            variable.chips.Insert(i + 1, c);
                            GUI.FocusControl("");
                            return;
                        }
                    
                    using (GUIHelper.Lock.Start(i == 0))
                        if (GUILayout.Button("^", EditorStyles.miniButtonMid, GUILayout.Width(20))) {
                            variable.chips.Remove(c);
                            variable.chips.Insert(i - 1, c);
                            GUI.FocusControl("");
                            return;
                        }        
                    
                    if (GUILayout.Button("D", EditorStyles.miniButtonMid, GUILayout.Width(20))) {
                        variable.chips.Insert(i, c.Clone());
                        return;
                    }

                    if (GUILayout.Button($"{i + 1}. {c.Reference.ID}", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true))) {
                        selectionToEdit[0] = c;
                        GUI.FocusControl("");
                    }
                }

                if (selectionToEdit[0] == c)
                    Edit(selectionToEdit, fieldEditor);
            }
        }
    }
}