using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Colors;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class SlotContent : LevelContent {
        
        [Flags]
        public enum EmitType {
            Unknown = 0,
            Matching = 1 << 0,
            Generated = 1 << 1,
            Script = 1 << 2
        }
        
        public EmitType emitType = EmitType.Unknown;
        
        public int birthDate = -1;
        
        public string miniIcon;
        public override Type BodyType => typeof(SlotContentBody);
        
        public Vector2 localPosition {
            get => position - slotModule.Position;
            set => position = slotModule.Position + value;
        }
        
        public string shortName;
        
        public SlotModule slotModule;
        
        public Color color = Color.white;
        
        public abstract bool IsUniqueContent();

        public override SpaceObject EmitBody() {
            if (simulation?.AllowBodies() ?? true) 
                return base.EmitBody();
            return null;
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            if (this is ILayered layered)
                LayerTrigger.Trigger(body.gameObject, layered.layer);

            if (this is IBigSlotContent)
                slotModule = AddModule<MultipleSlotModule>();
            else
                slotModule = AddModule<SingleSlotModule>();
            
            if (this is IColored colored && colored.colorInfo.IsKnown()) {
                
                var colorSettings = context.GetArgument<LevelColorSettings>();
                
                if (colorInfo.type == ItemColor.KnownRandom)
                    colorInfo = ItemColorInfo.ByID(colorSettings.GetRandomColorID(random));
                
                if (body) {
                    Repaint(gameplay.colorPalette, this, colored.colorInfo);
                    SetSpriteRepaint.Repaint(body.gameObject, colored.colorInfo.ID);
                }
            } else
                body?.gameObject.Repaint(color);
            
            if (this is INearHitDestroyable)
                events.onHit += OnNearHit;
        }

        public override void OnRemoveFromSpace(Space space) {
            BreakParent();
            if (this is INearHitDestroyable)
                events.onHit -= OnNearHit;
            base.OnRemoveFromSpace(space);
        }

        public virtual void OnChangeSlot() {}
        
        public void BreakParent() {
            slotModule.Slots()
                .ToArray()
                .ForEach(s => s.RemoveContent(this));
        }

        public override void HideAndKill() {
            MarkAsDestroyed();
            BreakParent();
            base.HideAndKill();
        }

        #region ILineBreaker

        public bool breakLines = false;

        public virtual bool BreakTheLine() {
            return breakLines;
        }
        
        #endregion
        
        #region Destroying
        
        public string destroyingEffect { get; set; }
        
        public float destroyingDelay = .2f;
        
        HitContext hitContext;
        
        public int Hit(HitContext hitContext) {
            if (!IsAlive()) return 0;
            
            if (!(this is IDestroyable destroyable)) 
                return 0;
            
            if (destroyable.destroying || destroyed) 
                return 0;
                
            if (birthDate >= gameplay.matchDate && hitContext.reason != HitReason.Player) return 0;
                
            if (!CanBeHit(hitContext))
                return 0;

            this.hitContext = hitContext;

            if (this is ILayered layered) {
                layered.layer--;
                if (layered.layer > 0) {
                    LayerTrigger.Trigger(body.gameObject, layered.layer);
                    Effect.Emit(field, layered.layerDownEffect, position, new RepaintEffectLogicProvider.Callback {
                        colorInfo = colorInfo
                    });
                    layered.OnChangeLayer(layered.layer);
                    return layered.layerScoreReward;
                }
            }

            DestroyingBase().Run(field.coroutine);
            
            return destroyable.scoreReward;
        }
        
        protected virtual bool CanBeHit(HitContext hitContext) {
            return true;
        }
        
        void OnNearHit(HitContext hitContext) {
            if (!visible || hitContext.reason != HitReason.Matching) return;
            
            foreach (var hitSlot in hitContext.group) {
                foreach (var slot in slotModule.Slots()) {
                    if (slot.coordinate.FourSideDistanceTo(hitSlot.coordinate) != 1) continue;
                    if (slotModule.Has(hitSlot)) continue;
                    if (hitSlot.GetCurrentContent() is Chip) {
                        HitAndScore();
                        return;
                    }
                }
            }
        }

        public void HitAndScore(HitContext hitContext = null) {    
            var points = Hit(hitContext);
            if (points <= 0) return;
            
            gameplay.score.AddScore(points);
            
            ShowScoreEffect(points, colorInfo);
        }

        public bool destroying {
            private set;
            get;
        }
        
        bool destroyed;
        
        public void MarkAsDestroyed() {
            destroyed = true;
        }

        IEnumerator DestroyingBase() {
            if (!(this is IDestroyable destroyable)) 
                yield break;

            if (destroying || destroyed)
                yield break;
            
            MarkAsDestroyed();
            destroying = true;

            using (MatchingPool.Get(context).Use()) {
                if (destroyingDelay > 0) {
                    if (simulation.AllowToWait())
                        yield return time.Wait(destroyingDelay);
                    else
                        yield return null;
                }
                
                lcAnimator.StopAllImmediate();
                
                OnStartDestroying();
                
                lcAnimator.PlaySound("Destroying");

                yield return null;
                
                events.onStartDestroying.Invoke(this, hitContext);

                if (simulation.AllowEffects() && !destroyingEffect.IsNullOrEmpty()) {
                    var effect = Effect.Emit(field, destroyingEffect, position, 
                        GetDestroyingEffectCallbacks()
                            .Collect<IEffectCallback>()
                            .ToArray());
                    
                    if (effect)
                        OnCreateDestroyingEffect(effect);
                }

                lcAnimator.StopAllImmediate();
                
                yield return destroyable.Destroying();
                
                OnEndDestroying();

                BreakParent();
            
                events.onSlotContentDestroyed.Invoke(this);
                events.onSomeContentDestroyed.Invoke();
                
                Kill();
            }
        }

        public virtual void OnStartDestroying() {}
        public virtual void OnEndDestroying() {}
        public virtual void OnCreateDestroyingEffect(Effect effect) {}
        public virtual IEnumerable GetDestroyingEffectCallbacks() {
            yield return new RepaintEffectLogicProvider.Callback {
                colorInfo = colorInfo
            };
        }
        
        #endregion

        #region Reapint
        
        public static void Repaint(ItemColorPalette colorPalette, SlotContent coloredObject, ItemColorInfo colorInfo) {
            if (!(coloredObject is IColored colored)) return;
        
            colored.colorInfo = colorInfo;
        
            if (!coloredObject.body) return;

            coloredObject.body.gameObject.Repaint(colorPalette, colorInfo);
            
            coloredObject.OnRepaint(colorPalette);
            
            SetSpriteRepaint.Repaint(coloredObject.body.gameObject, colored.colorInfo.ID);
        }

        protected virtual void OnRepaint(ItemColorPalette colorPalette) { }
        
        #endregion

        #region Animations
        
        LevelContentAnimator.OpenLoopCloseLayer flashingAnimationLayer;

        public void Flashing() {
            if (flashingAnimationLayer != null)
                return;
            
            flashingAnimationLayer = lcAnimator
                .PlayOpenLoopClose(null, "Flashing", null, 2);
        }

        public void StopFlashing() {
            if (flashingAnimationLayer == null)
                return;
            
            lcAnimator.Stop(flashingAnimationLayer);
            flashingAnimationLayer = null;
        }

        #endregion
        
        #region Variables
        
        public virtual ItemColorInfo colorInfo { get; set; }
       
        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            if (this is IColored) yield return typeof(ColoredVariable);
            if (this is ILayered) yield return typeof(LayeredVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case ColoredVariable c: {
                    colorInfo = c.info;
                } break;
                case LayeredVariable l: { 
                    if (this is ILayered layered) 
                        layered.layer = l.count;
                } break;
            }
        }

        #endregion
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("shortName", shortName);
            writer.Write("miniIcon", miniIcon);
            if (this is IDestroyable destroyable) {
                writer.Write("destroyingEffect", destroyable.destroyingEffect);
                writer.Write("scoreReward", destroyable.scoreReward);
                writer.Write("destroyingDelay", destroyingDelay);
                if (this is ILayered layered) {
                    writer.Write("layer", layered.layer);
                    writer.Write("layersCount", layered.layersCount);
                    writer.Write("layerScoreReward", layered.layerScoreReward);
                    writer.Write("layerDownEffect", layered.layerDownEffect);
                }
            }
            if (!(this is IColored colored))
                writer.Write("color", color);
            if (this is ILineBreaker)
                writer.Write("breakLines", breakLines);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("shortName", ref shortName);
            reader.Read("miniIcon", ref miniIcon);
            if (this is IDestroyable destroyable) {
                destroyable.destroyingEffect = reader.Read<string>("destroyingEffect");
                destroyable.scoreReward = reader.Read<int>("scoreReward");
                reader.Read("destroyingDelay", ref destroyingDelay);
                if (this is ILayered layered) {
                    layered.layer = reader.Read<int>("layer");
                    layered.layersCount = reader.Read<int>("layersCount");
                    layered.layerScoreReward = reader.Read<int>("layerScoreReward");
                    layered.layerDownEffect = reader.Read<string>("layerDownEffect");
                }
            }
            
            if (!(this is IColored colored))
                reader.Read("color", ref color);
            if (this is ILineBreaker)
                reader.Read("breakLines", ref breakLines);
        }

        #endregion
        
        #region Selection
        
        LevelContentAnimator.OpenLoopCloseLayer selectionAnimationLayer;
        
        public virtual void OnSelect() {
            if (selectionAnimationLayer != null)
                return;
            
            selectionAnimationLayer = lcAnimator
                .PlayOpenLoopClose("OnSelect", "Selected", "OnUnselect", 1);
        }

        public virtual void OnUnselect() {
            if (selectionAnimationLayer == null)
                return;
            
            lcAnimator.Stop(selectionAnimationLayer);
            selectionAnimationLayer = null;
        }
        
        #endregion
    }
    
    public interface IDefaultSlotContent {
        bool IsSuitableForNewSlot(Level level, SlotInfo slot);
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BaseContentOrderAttribute : Attribute {
        public readonly int order;
        public BaseContentOrderAttribute(int order) {
            this.order = order;
        }
    }
}

