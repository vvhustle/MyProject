using Yurowm.ObjectEditors;
using Yurowm.Sounds;

namespace Yurowm.Editors {
    public class MusicEditor : ObjectEditor<Music> {
        public override void OnGUI(Music music, object context = null) {
            SoundEditorHelpers.EditClipPath("Path", music.path, p => music.path = p);
        }
    }
}