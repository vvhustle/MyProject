using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace Yurowm.Effects {
    public class Effect : LevelContent {

        public override Type BodyType => typeof(EffectBody);
        public EffectBody effectBody => body as EffectBody;

        IEffectCallback[] callbacks = null;
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
        
            if (effectBody != null)
                effectBody
                    .GetLogicProviders()
                    .Select(p => p.Logic(context, callbacks, this))
                    .Parallel()
                    .ContinueWith(Kill)
                    .Run(space.coroutine);
            else
                Kill();
        }

        public override Type GetContentBaseType() {
            return typeof(Effect);
        }
        
        static Effect Emit(string prefabName, Vector2 position, params IEffectCallback[] callbacks) {
            if (prefabName.IsNullOrEmpty())
                return null;
            
            var prefab = AssetManager.GetPrefab<EffectBody>(prefabName);
            if (!prefab) return null;
            
            var effect = New<Effect>(prefabName);
                
            effect.position = position;
            
            effect.callbacks = callbacks;
            
            return effect;
        }
        
        public static Effect Emit(Field field, string prefabName, Vector2 position, params IEffectCallback[] callbacks) {
            var effect = Emit(prefabName, position, callbacks);
            
            if (effect)
                field.AddContent(effect);
            
            return effect;
        }
        
        public static Effect Emit(Space space, string prefabName, Vector2 position, params IEffectCallback[] callbacks) {
            var effect = Emit(prefabName, position, callbacks);
            
            if (effect)
                space.AddItem(effect);
            
            return effect;
        }

        static readonly IEffectCallback[] emptyCallbacks = null;

        public static Effect Emit(Field field, string prefabName, Vector2 position) {
            return Emit(field, prefabName, position, emptyCallbacks);
        }
    }
}