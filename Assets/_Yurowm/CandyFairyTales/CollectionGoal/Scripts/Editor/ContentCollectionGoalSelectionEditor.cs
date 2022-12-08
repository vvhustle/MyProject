using System.Linq;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    public class ContentCollectionGoalSelectionEditor : ContentSelectionEditor<ContentCollectionGoal> {
        
        StorageSelector<LevelContent> contentSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is SlotContent && c is IDestroyable);
        
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {
            EUtils.DrawMixedProperty(selection
                    .Select(c => c.GetVariable<ContentIDVariable>()),
                variable => variable.id,
                (variable, value) => variable.id = value,
                (variable, value) => {
                    contentSelector.Select(c => c.ID == value);
                    contentSelector.Draw("Content", l => variable.id = l?.ID);
                    return value;
                });
        }
    }
}