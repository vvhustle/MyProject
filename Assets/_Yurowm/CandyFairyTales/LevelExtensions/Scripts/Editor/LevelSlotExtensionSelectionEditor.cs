using System;
using YMatchThree.Core;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public abstract class LevelSlotExtensionSelectionEditor<C> : ObjectEditor where C : class {
        public override void OnGUI(object obj, object context = null) {
            var selection = (LevelSlotExtensionSelection) obj;
            
            if (selection.extension?.Reference is C 
                && selection.slots.Length > 0
                && context is LevelFieldEditor fieldEditor)
                OnGUI(selection.slots, selection.extension, fieldEditor);
        }

        public abstract void OnGUI(SlotInfo[] slots, ContentInfo extension, LevelFieldEditor fieldEditor);

        public override bool IsSuitableType(Type type) {
            return typeof(LevelSlotExtensionSelection).IsAssignableFrom(type);
        }
    }
    
    public struct LevelSlotExtensionSelection {
        public ContentInfo extension;
        public SlotInfo[] slots;
    }
}