using System.Linq;
using UnityEditor;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    public class ChipLimiterSelectionEditor : ContentSelectionEditor<ChipLimiter> {
        
        StorageSelector<LevelContent> contentSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is Chip && !c.IsDefault);
        
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {
            EUtils.DrawMixedProperty(selection
                    .Select(c => c.GetVariable<ContentIDVariable>()),
                variable => variable.id,
                (variable, value) => variable.id = value,
                (variable, value) => {
                    contentSelector.Select(c => c.ID == value);
                    contentSelector.Draw("Chip", l => variable.id = l?.ID);
                    return value;
                });
            
            EUtils.DrawMixedProperty(selection
                    .Select(c => c.GetVariable<CountVariable>()),
                variable => variable.value,
                (variable, value) => variable.value = value,
                (variable, value) => EditorGUILayout.IntField("Count", variable.value).ClampMin(1));
        }
    }
}