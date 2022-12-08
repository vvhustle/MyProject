using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class ChipMixCollectionEditor : ObjectEditor<List<ChipMixRecipe>> {
        
        StorageSelector<LevelContent> mixSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is ChipMix);
        
        StorageSelector<LevelContent> chipASelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is Chip && c is IDestroyable);
        
        StorageSelector<LevelContent> chipBSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is Chip && c is IDestroyable);
        
        string chipA;
        string chipB;
        
        Color highlightColorA = new Color(0.4f, 0.53f, 1f);
        Color highlightColorB = new Color(0.8f, 0.49f, 1f);
        
        public ChipMixCollectionEditor() {
            chipA = chipASelector.Select()?.ID;
            chipB = chipBSelector.Select()?.ID;
        }
        
        public override void OnGUI(List<ChipMixRecipe> collection, object context = null) {
            EditorGUILayout.PrefixLabel("Chip Mixes");

            using (GUIHelper.Horizontal.Start()) {
                chipASelector.Draw(null, c => chipA = c.ID, GUILayout.Width(120));
                EditorTips.PopLastRectByID("lc.mixrecipe.a");
                
                GUILayout.Label("+", GUILayout.Width(12));
                
                chipBSelector.Draw(null, c => chipB = c.ID, GUILayout.Width(120));
                EditorTips.PopLastRectByID("lc.mixrecipe.b");
                
                GUILayout.Label("=>", GUILayout.Width(20));
                
                var recipe = collection.FirstOrDefault(m => m.pair == new Pair<string>(chipA, chipB));
                
                if (recipe == null) {
                    if (GUILayout.Button("+")) {
                        recipe = new ChipMixRecipe {
                            firstChip = chipA,
                            secondChip = chipB,
                            mix = mixSelector.Select()?.ID
                        };
                        collection.Add(recipe);
                    }
                    EditorTips.PopLastRectByID("lc.mixrecipe.new");
                } else {
                    mixSelector.Select(c => c.ID == recipe.mix);
                    mixSelector.Draw(null, c => recipe.mix = c.ID, GUILayout.Width(120));
                    EditorTips.PopLastRectByID("lc.mixrecipe.select");
                    
                    if (GUILayout.Button("X", GUILayout.Width(30)))
                        collection.Remove(recipe);
                    EditorTips.PopLastRectByID("lc.mixrecipe.remove");
                }
            }
            foreach (var mix in collection) {
                using (GUIHelper.Horizontal.Start()) {
                    if (GUILayout.Button("E", GUILayout.Width(30))) {
                        chipA = chipASelector.Select(c => c.ID == mix.firstChip)?.ID;                            
                        chipB = chipBSelector.Select(c => c.ID == mix.secondChip)?.ID;   
                        mix.mix = mixSelector.Select(c => c.ID == mix.mix)?.ID;
                    }
                    
                    EditorTips.PopLastRectByID("lc.mixrecipe.edit");
                    
                    var a = mix.firstChip;
                    var b = mix.secondChip;
                    
                    if (Event.current.type == EventType.Repaint) {
                        if (a == chipA) 
                            a = a.Colorize(highlightColorA);
                        else if (a == chipB)
                            a = a.Colorize(highlightColorB);
                        
                        if (b == chipA) 
                            b = b.Colorize(highlightColorA);
                        else if (b == chipB)
                            b = b.Colorize(highlightColorB);
                    }
                    
                    GUILayout.Label($"{a} + {b} => {mix.mix}", Styles.richLabel);
                }
            }
        }
    }
}