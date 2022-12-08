using UnityEngine;
using System.Linq;
using UnityEditor;
using Yurowm.GUIHelpers;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    [CustomEditor(typeof(SetSpriteRepaint))]
    public class SetSpriteColorEditor : UnityEditor.Editor {

        SetSpriteRepaint component;
        
        StorageSelector<LevelContent> colorSetSelector;
        
        ItemColorPalette highlightsPalette;

        void OnEnable() {
            component = target as SetSpriteRepaint;
            if (component != null) {
                foreach (int colorID in ItemColorInfo.IDs)
                    if (component.sprites.All(x => x.colorID != colorID))
                        component.sprites.Add(new SetSpriteRepaint.SpriteInfo {colorID = colorID});

                component.sprites.Sort((x, y) => x.colorID.CompareTo(y.colorID));
            }
        }

        public override void OnInspectorGUI() {
            Undo.RecordObject(component, "SetSpriteColor changed");
            
            component.setSpriteMask = EditorGUILayout.Toggle("Set Sprite Mask", component.setSpriteMask);
            
            if (colorSetSelector == null)
                colorSetSelector = new StorageSelector<LevelContent>(LevelContent.storage,
                        c => c?.ID,
                        c => c is ItemColorPalette);

            colorSetSelector.Select(c => c.ID == highlightsPalette?.ID);
            colorSetSelector.Draw("Highlight Palette", c => 
                highlightsPalette = (ItemColorPalette) c);

            if (component.sprites.Count == 0)
                OnEnable();
                
            foreach (SetSpriteRepaint.SpriteInfo info in component.sprites)
                using (GUIHelper.Horizontal.Start())
                using (GUIHelper.BackgroundColor.Start(highlightsPalette?.Get(info.colorID) ?? Color.white))
                    info.sprite = (Sprite) EditorGUILayout.ObjectField(info.colorID.ToString(), info.sprite,
                        typeof(Sprite), false);

            EditorUtility.SetDirty(component);
        }
    }
}