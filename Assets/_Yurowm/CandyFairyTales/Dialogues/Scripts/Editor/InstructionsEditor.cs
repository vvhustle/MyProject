using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using YMatchThree.Editor;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Localizations;
using Yurowm.Nodes.Editor;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;


namespace Yurowm.Dialogues {
    public class CharacterInstructionEditor : ObjectEditor<CharacterInstruction> {
        public override void OnGUI(CharacterInstruction instruction, object context = null) {
            instruction.side = (Dialogue.Side) EditorGUILayout.EnumPopup("Side", instruction.side);
        }
    }
    
    public class CharacterShowInstructionEditor : ObjectEditor<CharacterShowInstruction> {
        public override void OnGUI(CharacterShowInstruction instruction, object context = null) {
            BaseTypesEditor.SelectAsset<Character>(instruction, nameof(instruction.characterName));
        }
    }
    
    public class SayInstructionEditor : ObjectEditor<SayInstruction> {
        public override void OnGUI(SayInstruction instruction, object context = null) {
            
            var editor = context as NodeSystemEditor;

            instruction.localized = EditorGUILayout.Toggle("Localized", instruction.localized);
            
            if (instruction.localized) {
                if (editor?.nodeSystem is LevelScriptBase level) {
                    if (level.IsLocalized()) { 
                        instruction.localizationKey = EditorGUILayout.TextField("Localization Key", instruction.localizationKey);
                        
                        if (!instruction.localizationKey.IsNullOrEmpty())
                            LocalizationEditor.EditOutside(level.GetFullLocalizationKey(instruction.localizationKey));
                    } else
                        EditorGUILayout.HelpBox(
                            "The script doesn't have a general localization key. Open Parameters and write the key.",
                            MessageType.Error);
                }
            } else
                instruction.text = EditorGUILayout.TextField("Text", instruction.text);
        }
    }
}
