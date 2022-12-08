using Yurowm.ObjectEditors;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Serialization;
using Yurowm.Shapes;
using Yurowm.Spaces;

namespace Yurowm.YPlanets.Editor {
    public class MatchChainGameplayEditor : ObjectEditor<MatchChainGameplay> {
        
        StorageSelector<LevelContent> bombSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is Chip);
        
        public override void OnGUI(MatchChainGameplay gameplay, object context = null) {
            BaseTypesEditor.SelectAsset<YLine2D>("Line", gameplay, nameof(gameplay.lineName));
            
            Edit("Select Bouncing", gameplay.selectBouncing);
            Edit("Unselect Bouncing", gameplay.unselectBouncing);
            
            #region Combinations
            {
                EditorGUILayout.PrefixLabel("Combinations");
                
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
                        
                        combination.count =  EditorGUILayout
                            .IntField(combination.count, GUILayout.Width(40))
                            .ClampMin(4);
                        
                        combination.verticalCount =  EditorGUILayout
                            .IntField(combination.verticalCount, GUILayout.Width(40))
                            .ClampMin(1);
                        
                        combination.horizontalCount =  EditorGUILayout
                            .IntField(combination.horizontalCount, GUILayout.Width(40))
                            .ClampMin(1);
                        
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
                    gameplay.combinations.Add(new MatchChainGameplay.Combination());
            }
            #endregion
            
            Edit(gameplay.mixes);
        }
    }
}