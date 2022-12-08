using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class MatchClickGameplayEditor : ObjectEditor<MatchClickGameplay>  {
        
        StorageSelector<LevelContent> bombSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is Chip);
        
        public override void OnGUI(MatchClickGameplay gameplay, object context = null) {
            
            Edit("Click Bouncing", gameplay.clickBouncing);
            Edit("Match Bouncing", gameplay.matchBouncing);
            
            
            #region Combinations
            {
                GUILayout.Label("Combinations", Styles.title);
                using (GUIHelper.Horizontal.Start()) {
                    GUILayout.Label("Priority", Styles.centeredMiniLabel, GUILayout.Width(45));
                    GUILayout.Label("Count", Styles.centeredMiniLabel, GUILayout.Width(40));
                    GUILayout.Label("Vert.", Styles.centeredMiniLabel, GUILayout.Width(40));
                    GUILayout.Label("Horiz.", Styles.centeredMiniLabel, GUILayout.Width(40));
                    GUILayout.Label("Bomb", Styles.centeredMiniLabel);
                }

                int index = 0;
                foreach (var combination in gameplay.combinations) {
                    using (GUIHelper.Horizontal.Start()) {
                        GUILayout.Label($"{index + 1}. ", Styles.centeredMiniLabel, GUILayout.Width(45));
                        
                        combination.count =  Mathf.Max(EditorGUILayout.IntField(combination.count, GUILayout.Width(40)), 4);
                        combination.vertCount =  Mathf.Max(EditorGUILayout.IntField(combination.vertCount, GUILayout.Width(40)), 1);
                        combination.horizCount =  Mathf.Max(EditorGUILayout.IntField(combination.horizCount, GUILayout.Width(40)), 1);
                        
                        bombSelector.Select(b => b.ID == combination.bomb);
                        bombSelector.Draw(null, b => combination.bomb = b.ID);
                        
                        if (GUILayout.Button("^", GUILayout.Width(20)) && index > 0) {
                            gameplay.combinations.RemoveAt(index);
                            gameplay.combinations.Insert(index - 1, combination);
                            break;
                        }
                        if (GUILayout.Button("v", GUILayout.Width(20)) && index < gameplay.combinations.Count - 1) {
                            gameplay.combinations.RemoveAt(index);
                            gameplay.combinations.Insert(index + 1, combination);
                            break;
                        }
                        if (GUILayout.Button("x", GUILayout.Width(20))) {
                            gameplay.combinations.RemoveAt(index);
                            break;
                        }
                        
                        index++;
                    }
                }
                
                if (GUILayout.Button("New Combintion", GUILayout.Width(150)))
                    gameplay.combinations.Add(new MatchClickGameplay.Combination());
            }
            #endregion
            
            Edit(gameplay.mixes);
            
        }
    }
}