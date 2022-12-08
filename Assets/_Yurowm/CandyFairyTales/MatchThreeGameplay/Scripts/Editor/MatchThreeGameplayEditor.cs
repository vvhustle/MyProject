using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    public class MatchThreeGameplayEditor : ObjectEditor<MatchThreeGameplay> {
        
        StorageSelector<LevelContent> bombSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is Chip);
        
        public override void OnGUI(MatchThreeGameplay gameplay, object context = null) {
            gameplay.squaresMatch = EditorGUILayout.Toggle("Squares Match", gameplay.squaresMatch);
            gameplay.swapDuration = EditorGUILayout.Slider("Swap Duration", gameplay.swapDuration, 0.01f, 1f);
            gameplay.swapOffset = EditorGUILayout.FloatField("Swap Offset", gameplay.swapOffset);
           
            Edit("Swap Bouncing", gameplay.swapBouncing);
            Edit("Match Bouncing", gameplay.matchBouncing);
            Edit("Select Bouncing", gameplay.selectBouncing);

            #region Combinations
            {
                EditorGUILayout.PrefixLabel("Combinations"); 
                
                using (GUIHelper.Horizontal.Start()) {
                    GUILayout.Label("Priority", Styles.centeredMiniLabel, GUILayout.Width(45));
                    GUILayout.Label("Type", Styles.centeredMiniLabel, GUILayout.Width(80));
                    GUILayout.Label("Count", Styles.centeredMiniLabel, GUILayout.Width(40));
                    GUILayout.Label("Bomb", Styles.centeredMiniLabel);
                }

                int index = 0;
                foreach (var combination in gameplay.combinations) {
                    using (GUIHelper.Horizontal.Start()) {
                        GUILayout.Label($"{index + 1}. ", Styles.centeredMiniLabel, GUILayout.Width(45));
                        combination.type = (MatchThreeGameplay.Combination.Type) EditorGUILayout.EnumPopup(combination.type, GUILayout.Width(80));
                        combination.count =  EditorGUILayout.IntField(combination.count, GUILayout.Width(40)).Clamp(4, 9);
                        
                        bombSelector.Select(b => b.ID == combination.bomb);
                        bombSelector.Draw(null, b => combination.bomb = b.ID);
                        
                        if (GUILayout.Button("^", GUILayout.Width(20)) && index > 0) {
                            gameplay.combinations.RemoveAt(index);
                            gameplay.combinations.Insert(index - 1, combination);
                            GUIHelper.ClearControl();
                            break;
                        }
                        if (GUILayout.Button("v", GUILayout.Width(20)) && index < gameplay.combinations.Count - 1) {
                            gameplay.combinations.RemoveAt(index);
                            gameplay.combinations.Insert(index + 1, combination);
                            GUIHelper.ClearControl();
                            break;
                        }
                        if (GUILayout.Button("x", GUILayout.Width(20))) {
                            gameplay.combinations.RemoveAt(index);
                            GUIHelper.ClearControl();
                            break;
                        }
                        
                        index++;
                    }
                }
                
                if (GUILayout.Button("New Combintion", GUILayout.Width(150)))
                    gameplay.combinations.Add(new MatchThreeGameplay.Combination());
            }
            #endregion
            
            Edit(gameplay.mixes);
        }
    }
}
