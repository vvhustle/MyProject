using System;
using UnityEditor;
using YMatchThree.Core;
using Yurowm;

namespace YMatchThree.Editor {
    public class SlotTagEditor : ContentSelectionEditor<SlotTag> {
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {
            EUtils.DrawMixedProperty(selection,
                mask: c => c.Reference is SlotTag,
                getValue: c => c.GetVariable<TagsVariable>().tags,
                setValue: (c, value) => c.GetVariable<TagsVariable>().tags = value,
                drawSingle: (c, value) => EditorGUILayout.TextField("Tags", value));
        }
    }
}