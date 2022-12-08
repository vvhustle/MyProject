using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class LevelContent : SpacePhysicalItem, ISerializableID, IPuzzleSimulationSensitive {
        
        #region Storage
        [PreloadStorage]
        public static readonly Storage<LevelContent> storage = 
            new Storage<LevelContent>("LevelContent", TextCatalog.StreamingAssets);

        public static C GetItem<C>(string ID) where C : LevelContent {
            return storage.CastIfPossible<C>().FirstOrDefault(c => c.ID == ID);
        }
        
        public static C GetItem<C>() where C : LevelContent {
            return storage.CastIfPossible<C>().FirstOrDefault();
        }
        
        [LocalizationKeysProvider]
        static IEnumerable GetKeys() {
            return storage.items.CastIfPossible<ILocalized>();
        }
        
        #endregion
        
        public string ID {get; set;}
        
        public LevelContentAnimator lcAnimator;
        
        protected LevelEvents events;
        protected LevelScriptEvents scriptEvents;
        protected LevelGameplay gameplay;

        public Field field;
        
        public bool visible => enabled;
        
        public override void OnAddToSpace(Space space) {
            events = context.GetArgument<LevelEvents>();
            scriptEvents = space.context.GetArgument<LevelScriptEvents>();
            
            gameplay = context.Get<LevelGameplay>();
            base.OnAddToSpace(space);
            
            lcAnimator = new LevelContentAnimator(this);
            lcAnimator.simulation = simulation;
        }
        
        public void Hide() {
            if (!enabled) return;
            
            Hiding().Run(field.coroutine);
        }
        
        public virtual void HideAndKill() {
            HidingAndKill().Run(field.coroutine);
        }
        
        public virtual IEnumerator HidingAndKill() {
            yield return Hiding();
            Kill();
        }
        public virtual IEnumerator Hiding() {
            if (!IsAlive()) yield break;
            yield return lcAnimator.PlayClipAndWait("Hide");
            enabled = false;
        }
        
        public virtual void Show() {
            enabled = true;
            lcAnimator.PlayClip("Awake");
        }

        public override void OnEnable() {
            base.OnEnable();
            
            if (!body || !field) 
                return;
            
            if (gameplay && gameplay.IsPlaying())
                lcAnimator.PlayClip("Awake");
            else
                lcAnimator.animator?.RewindEnd("Awake");

            Blinking().Run(field.coroutine);
        }

        public override void OnKill() {
            field?.RemoveContent(this);
            base.OnKill();
        }

        public abstract Type GetContentBaseType();
        
        IEnumerator Blinking() {
            while (IsAlive() && enabled && lcAnimator.HasClip("Blink")) {
                if (!lcAnimator.IsPlaying())
                    yield return lcAnimator.PlayClipAndWait("Blink");
                    
                yield return time?.Wait(YRandom.main.Range(2f, 6f));
            }    
        }
        
        #region Variables

        public IEnumerable<Type> GetVariables() {
            var types = GetVariblesTypes().Collect<Type>();
            while (types.MoveNext())
                yield return types.Current;
        }
        
        public IEnumerable<ContentInfoVariable> EmitVariables() {
            return GetVariables()
                .Select(Activator.CreateInstance)
                .CastIfPossible<ContentInfoVariable>();
        }
        
        public virtual IEnumerator GetVariblesTypes() {
            yield break;
        }
        
        public virtual void SetupVariable(ISerializable variable) {}
        
        public void ApplyDesign(ContentInfo design) {
            design.Variables().ForEach(SetupVariable);
        }
        #endregion

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("ID", ID);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            ID = reader.Read<string>("ID");
        }

        #endregion
        
        #region IPuzzleSimulationSensitive

        protected PuzzleSimulation simulation;
        
        public void OnChangeSimulation(PuzzleSimulation simulation) { 
            this.simulation = simulation;         
            
            if (lcAnimator != null)
                lcAnimator.simulation = simulation;
        }

        #endregion
        
        public static void VibrateWithPower(float power) {
            Vibrator.AndroidVibrate(.02f);
            Vibrator.iOSVibrate(power <.7f ? Vibrator.iOSVibrateType.Pop : Vibrator.iOSVibrateType.Peek);
        }
        
        public void ShowScoreEffect(int points, ItemColorInfo colorInfo) {
            if (gameplay.scoreEffect.IsNullOrEmpty()) return;
            
            var effect = Effect.Emit(field, gameplay.scoreEffect, position, 
                new RepaintEffectLogicProvider.Callback() {
                    colorInfo = colorInfo
                });
            
            if (effect.body.SetupChildComponent(out TextMeshPro label)) {
                label.text = points.ToString();
            }
        }
    }
}