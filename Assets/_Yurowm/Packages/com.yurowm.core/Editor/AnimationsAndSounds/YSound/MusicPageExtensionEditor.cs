using UnityEditor;
using UnityEngine;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;
using Yurowm.Sounds;

namespace Yurowm.Editors {
    public class MusicPageExtensionEditor : ObjectEditor<MusicPageExtension> {
        public override void OnGUI(MusicPageExtension extension, object context = null) {
            extension.volume = EditorGUILayout.FloatField("Volume", extension.volume).Clamp01();
            
            if (GUIHelper.Button("Clip", extension.clip ?? "null")) {
                var menu = new GenericMenu();
                
                foreach (var music in SoundBase.storage.Items<Music>()) {
                    var ID = music.ID;
                    menu.AddItem(new GUIContent(music.ID), 
                        extension.clip == ID, 
                        () => extension.clip = ID);
                }
            
                if (menu.GetItemCount() > 0)
                    menu.ShowAsContext();
            }
        }
    }
}