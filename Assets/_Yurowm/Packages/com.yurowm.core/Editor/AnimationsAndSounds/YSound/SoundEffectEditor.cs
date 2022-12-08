using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;
using Yurowm.Sounds;
using Yurowm.Utilities;

namespace Yurowm.Editors {
    public class SoundEffectEditor : ObjectEditor<SoundEffect> {
        Type[] modifierTypes;
        
        public SoundEffectEditor() {
            modifierTypes = Utils
                .FindInheritorTypes<SoundModifier>(true, false)
                .Where(t => !t.IsAbstract)
                .ToArray();
        }
        
        public override void OnGUI(SoundEffect soundEffect, object context = null) {
            void Edit(SoundModifier modifier) {
                ObjectEditor.Edit(modifier);
            }
            
            void AddNew() {
                var menu = new GenericMenu();

                foreach (var type in modifierTypes) {
                    if (soundEffect.modifiers.Any(m => type.IsInstanceOfType(m)))
                        continue;
                    menu.AddItem(new GUIContent(type.FullName.Replace('.', '/')), false, () => {
                        soundEffect.modifiers.Add(Activator.CreateInstance(type) as SoundModifier);
                    });
                }
                
                if (menu.GetItemCount() > 0)
                    menu.ShowAsContext();
            }
            
            EditList("Modifiers", soundEffect.modifiers, Edit, AddNew);
            
            if (GUIHelper.Button("Test", "Play")) 
                soundEffect.Play();
        }
    }
}