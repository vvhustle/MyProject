using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Yurowm.UI;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class LevelLimitation : LevelScriptExtension, IUIRefresh {
        
        public override Type GetContentBaseType() {
            return typeof(LevelLimitation);
        }
        
        List<LevelLimitationBody> bodies = new List<LevelLimitationBody>();

        public override Type BodyType => typeof(LevelLimitationBody);
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            UpdateCounters();
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            bodies.ForEach(b => b.Kill());
            bodies.Clear();
        }
        
        public override SpaceObject EmitBody() {
            foreach (var container in ObjectTag.GetAll<RectTransform>("Field.Limitation")) {
                var result = AssetManager.Emit<LevelLimitationBody>(bodyName, context);
                result.transform.SetParent(container);   
                result.rectTransform?.Maximize();
                result.item = this;
                bodies.Add(result);
            }

            return null;
        }

        void UpdateCounters() {
            bodies.ForEach(UpdateCounter);
        }
        
        public void Refresh() {
            UpdateCounters();
        }
        
        protected abstract void UpdateCounter(LevelLimitationBody body);

        public abstract bool AllowToMove();

        public abstract int GetUnits(int max);
        public abstract void OnMove();
        
        public abstract void OnBonusCreated();
    }
}