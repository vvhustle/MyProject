using Yurowm.ObjectEditors;
using UnityEditor;
using UnityEngine;
using Yurowm.GUIHelpers;
using Yurowm.Sounds;

namespace Yurowm.Editors {
    public class SoundShuffleEditor : ObjectEditor<SoundShuffle> {
        public override void OnGUI(SoundShuffle sound, object context = null) {
            void AddNew() {
                var menu = new GenericMenu();

                foreach (var p in SoundEditorHelpers.GetAllSoundPaths()) {
                    var _p = p;
                    var contains = sound.paths.Contains(p);
                    menu.AddItem(new GUIContent(p), contains, () => {
                        if (contains)
                            sound.paths.Remove(_p);
                        else
                            sound.paths.Add(_p);
                    });
                }

                if (menu.GetItemCount() > 0)
                    menu.ShowAsContext();
            }

            EditStringList("Paths", sound.paths, AddNew);
        }
    }
}