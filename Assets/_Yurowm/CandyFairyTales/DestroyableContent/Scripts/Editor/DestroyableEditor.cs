using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class DestroyableEditor : ObjectEditor<IDestroyable> {

        public override void OnGUI(IDestroyable destroyable, object context = null) {
            destroyable.scoreReward = Mathf.Max(0, EditorGUILayout.IntField("Score Reward", destroyable.scoreReward));
            EditorTips.PopLastRectByID("lc.destroyable.score");
            
            BaseTypesEditor.SelectAsset<EffectBody>("Destroying Effect", destroyable, nameof(destroyable.destroyingEffect));
            EditorTips.PopLastRectByID("lc.destroyable.effect");
        }
    }
    
    public class LayeredEditor : ObjectEditor<ILayered> {

        public override void OnGUI(ILayered layered, object context = null) {
            layered.layersCount = Mathf.Max(2, EditorGUILayout.IntField("Layers Count", layered.layersCount));
            EditorTips.PopLastRectByID("lc.layered.count");
            
            layered.layerScoreReward = Mathf.Max(0, EditorGUILayout.IntField("Layers Score Reward", layered.layerScoreReward));
            EditorTips.PopLastRectByID("lc.layered.score");
            
            layered.layer = DrawSingle(layered.layer, layered.layersCount);
            EditorTips.PopLastRectByID("lc.layered.layer");
            
            BaseTypesEditor.SelectAsset<EffectBody>("Layer Down Effect", layered, nameof(layered.layerDownEffect));
            EditorTips.PopLastRectByID("lc.layered.effect");
        }
        
        public int DrawSingle(int layer, int max) {
            return EditorGUILayout.IntSlider("Layer", layer, 1, max);
        }
    }
    
    public class LayeredSelectionEditor : ContentSelectionEditor<ILayered> {
        LayeredEditor layeredEditor = new LayeredEditor();

        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {
            EUtils.DrawMixedProperty(selection,
                mask: c => c.Reference is ILayered,
                getValue: c => c.GetVariable<LayeredVariable>().count,
                setValue: (c, value) => c.GetVariable<LayeredVariable>().count 
                    = Mathf.Clamp(value, 1, (c.Reference as ILayered).layersCount),
                drawSingle: (c, value) => layeredEditor.DrawSingle(value, (c.Reference as ILayered).layersCount));
        }
    }
}