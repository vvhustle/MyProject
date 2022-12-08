using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class NodeBomb : NodeBombBase, IMixableChip {
        
        protected override IEnumerable<IEffectCallback> NodeEffectSetup() {
            yield return new NodeEffectLinearInterpolator(random.Range(3f, 9f), 0);
        }

        protected override IEnumerable<SlotContent> GetNodeTargets() {
            return field.slots.all.Values
                .Select(s => s.GetCurrentContent())
                .NotNull()
                .Where(c => c != this && c is IDestroyable 
                    && c.birthDate < gameplay.matchDate
                    && TargetFilter(c));
        }
        
        ItemColorInfo targetColor = ItemColorInfo.None;
        
        protected virtual bool TargetFilter(SlotContent content) {
            if (targetColor == ItemColorInfo.None)
                targetColor = field.slots.all.Values
                    .Where(s => s.visible)
                    .Select(s => s.GetCurrentContent() as IColored)
                    .Where(c => c != null && c.colorInfo.IsMatchableColor())
                    .GetRandom(random)
                    .colorInfo;

            return content is IColored c && c.colorInfo.IsMatchWith(targetColor);
        }

        public override void OnReachedTheTarget(Effect effect, Slot target, HitContext hitContext) {
            target.HitAndScore(hitContext);
        }

        #region IMixableChip

        public int MixPriority { get; set; }

        Chip secondMixChip;

        public void PrepareMixWith(Chip secondChip) {
            secondMixChip = secondChip;
            if (secondChip is IColored c && c.colorInfo.IsMatchableColor())
                targetColor = c.colorInfo;
            
            hit.Initialize(secondMixChip.slotModule.Slot().GetTempModule(), context);
        }

        public IEnumerator MixLogic() {
                
            if (secondMixChip is NodeBombBase) {

                // Apply node effect to all the content
                
                var targets = context
                    .GetArgument<Slots>().all.Values
                    .Select(s => s.GetCurrentContent())
                    .NotNull()
                    .Where(c => c != this && c is IDestroyable && c.birthDate < gameplay.matchDate)
                    .ToArray();

                hit.Prepare(
                    targets: targets,
                    onReachTarget: OnReachedTheTarget,
                    nodeEffectSetup: NodeEffectSetup,
                    random: random);
                
                yield return hit.Logic();
                
            } else if (secondMixChip is BombChipBase bomb) {
                
                var bombInfo = new ContentInfo(bomb);
                
                var bombs = new List<BombChipBase>();
                var field = this.field;
                
                void onReachedTheTarget(Effect effect, Slot target, HitContext hitContext) {
                    var slot = target;

                    var newBomb = bombInfo.Reference.Clone() as BombChipBase;
                    
                    var color = target.GetCurrentColor();
                    
                    if (color.IsKnown())
                        newBomb.SetupVariable(new ColoredVariable {
                            info = color
                        });

                    var currentContent = target.GetCurrentContent();
                    
                    currentContent.HideAndKill();
                    
                    field.AddContent(newBomb);

                    slot.AddContent(newBomb);
                    
                    newBomb.localPosition = Vector2.zero;
                    
                    bombs.Add(newBomb);  
                }
                
                hit.Prepare(
                    targets: GetNodeTargets()
                        .Where(t => t != this && t != secondMixChip)
                        .ToArray(),
                    onReachTarget: onReachedTheTarget,
                    nodeEffectSetup: NodeEffectSetup,
                    random: random);
                
                yield return hit.Logic();

                var hc = new HitContext(context,
                    bombs.SelectMany(b => b.slotModule.Slots()).ToList(),
                    HitReason.BombExplosion);
                
                yield return time.Wait(.2f);
                
                while (!bombs.IsEmpty()) {
                    bombs.RemoveAll(b => b.destroying || !b.IsAlive());
                    bombs.GetRandom(random)?.HitAndScore(hc);
                    yield return time.Wait(.1f);
                }
            } else if (secondMixChip is IColored colored && colored.colorInfo.IsMatchableColor()) {
                targetColor = colored.colorInfo;
                
                hit.Prepare(
                    targets: GetNodeTargets()
                        .Where(t => t != this)
                        .ToArray(),
                    onReachTarget: OnReachedTheTarget,
                    nodeEffectSetup: NodeEffectSetup,
                    random: random);
                
                yield return hit.Logic();
            } else 
                yield return Destroying();
            
            if (!destroying) 
                HideAndKill();
        }
        
        #endregion

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("MixPriority", MixPriority);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            MixPriority = reader.Read<int>("MixPriority");
        }

        #endregion
    }
}