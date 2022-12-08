using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.UI;
using Behaviour = Yurowm.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class LevelGoal : LevelExtension, IUIRefresh, IFailReason, ILocalized {
        
        public override Type BodyType => null;

        public override Type GetContentBaseType() {
            return typeof (LevelGoal);
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            EmitCounters();
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            counterBodies.ForEach(b => b.Hide());
        }

        #region Counter

        protected List<LevelGoalCounter> counterBodies = new List<LevelGoalCounter>();
        
        void EmitCounters() {
            var panels = Behaviour
                .GetAll<LevelGoalContainer>()
                .ToArray();
            
            if (panels.IsEmpty()) return;
            
            panels.ForEach(container => {
                var body = container.GetNewCounter();
                
                if (!body) return;
                
                body.SetIcon(GetIcon());
                
                counterBodies.Add(body);
                
                body.Show();
                
                OnCounterCreated(body);
            });
        }

        public abstract Sprite GetIcon();
        public virtual void OnCounterCreated(LevelGoalCounter counter) {}
        
        public abstract string GetCounterValue();
        
        bool isCounterComplete = false;
        
        public void Refresh() {
            if (counterBodies.IsEmpty() || isCounterComplete) return;
            
            var complete = IsComplete();
            
            if (complete)
                isCounterComplete = true;
            
            foreach (var body in counterBodies) {
                if (complete) {
                    if (body.SetupComponent(out ContentAnimator ca))
                        ca.Play(LevelGoalCounter.completeClip);
                    else
                        body.label.text = "v";    
                } else
                    body.label.text = GetCounterValue();
            }
        }

        #endregion
        
        public abstract bool IsFailed();
        public abstract bool IsComplete();
        
        public LocalizedText failReason = new LocalizedText();

        public IEnumerable GetLocalizationKeys() {
            yield return failReason;
        }

        public string GetFailReason() {
            if (IsFailed() || !IsComplete())
                return failReason.GetText();
            return null;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            
            writer.Write("failReason", failReason);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("failReason", ref failReason);
        }
    }
    
    public abstract class LevelGoalWithIcon : LevelGoal {
        
        public string iconName;

        Sprite icon;
        
        public override void OnAddToSpace(Space space) {
            if (!iconName.IsNullOrEmpty()) 
                icon = AssetManager.GetAsset<Sprite>(iconName);
            base.OnAddToSpace(space);
        }

        public override Sprite GetIcon() {
            return icon;
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("icon", iconName);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("icon", ref iconName);
        }

        #endregion
    }
}