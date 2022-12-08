using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Controls;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class Slot : LevelContent, ICastable {
        
        public int2 coordinate;
        
        public const float Offset = 0.7f;
        
        #region Slots
        
        public Slots slots;
        
        public Dictionary<Side, Slot> nearSlot = new Dictionary<Side, Slot> (); // Nearby slots dictionary

        public Slot this[Side side] {
            get => nearSlot.Get(side);
            set => nearSlot[side] = value;
        }

        #endregion
        
        #region Content
        
        static Dictionary<Type, Type> baseTypesDictionary = new Dictionary<Type, Type>();
        static Type[] baseContentTypeOrder;
        Dictionary<Type, List<SlotContent>> content;
        
        void InitializeBaseContent() {
            if (baseContentTypeOrder == null)
                baseContentTypeOrder = storage.Items<SlotContent>()
                    .OrderBy(c => c.GetType().GetAttribute<BaseContentOrderAttribute>()?.order ?? int.MaxValue)
                    .Select(c => c.GetContentBaseType())
                    .Distinct()
                    .ToArray();
            
            content = baseContentTypeOrder
                .ToDictionaryValue(t => new List<SlotContent>());
        }
        
        Type GetBaseType<T>() where T : SlotContent {
            var type = typeof(T);
           
            if (baseTypesDictionary.TryGetValue(type, out var result))
                return result;
            
            result = storage.items.CastOne<T>()?.GetContentBaseType() ?? typeof (SlotContent);
            
            baseTypesDictionary.Add(type, result);
            
            return result;
        }
        
        public SlotContent GetCurrentContent() {
            return content.Values.FirstOrDefault(l => l.Count > 0)?.FirstOrDefault();
        }
        
        public ItemColorInfo GetCurrentColor() {
            return GetAllContent().CastOne<IColored>()?.colorInfo ?? ItemColorInfo.None;
        }
        
        public bool HasContent<T>() where T : SlotContent {
            if (content.TryGetValue(GetBaseType<T>(), out var list))
                return list.CastOne<T>() != null;
            
            return false;
        }
        
        public bool HasBaseContent<T>() where T : SlotContent {
            if (content.TryGetValue(typeof (T), out var list))
                return list.Count > 0;
            
            return false;
        }

        public IEnumerable<SlotContent> GetAllContent() {
            foreach (var list in content.Values)
                foreach (var c in list)
                    yield return c;
        }

        public IEnumerable<C> GetAllContent<C>() where C : SlotContent {
            if (content.TryGetValue(typeof (C), out var list))
                return list.CastIfPossible<C>();
            
            return null;
        }
        
        List<SlotContent> GetContentList(SlotContent item) {
            var baseType = item.GetContentBaseType();
            
            if (!content.TryGetValue(baseType, out var list)) {
                list = new List<SlotContent>();
                content.Add(baseType, list);
            }
            
            return list;
        }

        public C GetContent<C>() where C : SlotContent {
            if (content.TryGetValue(GetBaseType<C>(), out var list))
                return list.FirstOrDefault() as C;
            
            return null;
        }
        
        public Action<SlotContent> onAddContent = delegate {};
        public Action<SlotContent> onRemoveContent = delegate {};
        public Action onCalculateFallingSlot = delegate {};

        public void AddContent(SlotContent content) {
            var list = GetContentList(content);
            
            if (content.IsUniqueContent()) {
                list.ForEach(c => c.slotModule.Remove(this));
                list.Clear();
            }

            content.slotModule.Slots().ForEach(s => s.RemoveContent(content));
            
            list.Add(content);
            
            content.slotModule.SetCenterSlot(this);
            content.OnChangeSlot();
            onAddContent.Invoke(content);
        }
        
        public void LinkContent(SlotContent content) {
            var list = GetContentList(content);
            
            list.Add(content);
        } 
        
        public void RemoveContent(SlotContent content) {
            var list = GetContentList(content);

            if (list == null) return;
            
            content.slotModule.Remove(this);
            list.Remove(content);
            onRemoveContent.Invoke(content);
        }
        
        public bool IsFilled() {
            return HasBaseContent<Chip>() || HasBaseContent<Block>();
        }
        
        #endregion

        #region Falling Slots

        public Dictionary<Side, Slot> falling = new Dictionary<Side, Slot>();
        
        bool IsActiveSlot() {
            if (!IsAlive())
                return false;
            
            if (!visible)
                return false;
            
            if (HasBaseContent<Block>())
                return false; 
            
            return true;
        }

        public IEnumerable<Slot> GetFallingSlots() {
            if (!IsActiveSlot())
                yield break;

            foreach (var s in falling.Values.NotNull())
                if (s.IsActiveSlot())
                    yield return s;
        }
        
        public void CalculateFallingSlot() {
            var fieldArea = context.GetArgument<Level>().Area;

            foreach (var side in falling.Keys.ToArray()) {
                falling[side] = null;
                
                if (this[side] == null) continue;
                
                var coord = coordinate;
                
                while (fieldArea.Contains(coord)) {
                    coord += side;
                    
                    Block.BlockingType blockingType;

                    if (!slots.all.TryGetValue(coord, out var slot)) {
                        blockingType = gameplay.nullSlotBlockingType;
                        if (blockingType == Block.BlockingType.Hole) continue;
                        if (blockingType == Block.BlockingType.Obstacle) break;
                        break;
                    }
                    
                    if (slot.IsBlocked(out blockingType)) {
                        if (blockingType == Block.BlockingType.Hole) continue;
                        if (blockingType == Block.BlockingType.Obstacle) break;
                    } else {
                        falling[side] = slot;
                        break;
                    }
                }
                
                
                // if (!slots.all.ContainsKey(coordinate + side)) {
                //     int2 coord = coordinate;
                //     while (fieldArea.Contains(coord)) {
                //         coord += side;
                //         if (slots.all.TryGetValue(coord, out var slot)) {
                //             if (slot.IsBlocked(out var blockingType)) {
                //                 if (blockingType == Block.BlockingType.Hole) continue;
                //                 if (blockingType == Block.BlockingType.Obstacle) break;
                //             } else
                //                 falling[side] = slot;
                //             break;
                //         }
                //     }
                // } else if (this[side])
                //     falling[side] = slots.all[coordinate + side];
            }
            
            onCalculateFallingSlot();
        }

        #endregion

        public override Type GetContentBaseType() {
            return typeof (Slot);
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            falling.Add(Side.Bottom, null);
            
            context.SetupItem(out gameplay);
            InitializeBaseContent();
        }

        public override void OnRemoveFromSpace(Space space) {
            GetAllContent().ToArray().ForEach(c => c.HideAndKill());
            base.OnRemoveFromSpace(space);
        }
        
        public override void OnEnable() {
            base.OnEnable();
            GetAllContent().ForEach(c => c.enabled = true);
        }

        public override void OnDisable() {
            base.OnDisable();
            GetAllContent().ForEach(c => c.Hide());
        }

        public override IEnumerator HidingAndKill() {
            yield return GetAllContent()
                .ToArray()
                .Select(c => c.HidingAndKill())
                .Parallel();
            yield return base.HidingAndKill();
        }

        public override IEnumerator Hiding() {
            yield return GetAllContent()
                .ToArray()
                .Select(c => c.Hiding())
                .Parallel();
            yield return base.Hiding();
        }

        public bool IsInteractable() {
            return field?.IsInteractable(this) ?? false;
        }

        public bool IsBlocked(out Block.BlockingType blockingType) {
            var block = GetContent<Block>();
            
            if (block) {
                blockingType = block.blockingType;
                return true;
            }
            
            blockingType = default;
            return false;
        }

        LevelGameplay gameplay;
        ChipPhysic physic;

        #region Hitting

        public int Hit(HitContext context = null) {
            var current = GetCurrentContent();
            
            if (!current) return 0;
            
            if (current is Block block)
                return block.Hit(context);
            
            int score = GetAllContent<SlotModifier>()?.Sum(x => x.Hit(context)) ?? 0;

            if (current is Chip chip)
                score += chip.Hit(context);

            return score;
        }

        public void HitAndScore(HitContext hitContext = null) {    
            var colorInfo = GetCurrentColor();

            var points = Hit(hitContext);
            if (points <= 0) return;
            
            gameplay.score.AddScore(points);
            
            ShowScoreEffect(points, colorInfo);
        }

        #endregion

        #region ICastable

        public bool IsAvaliableForClick(int mode) {
            return enabled;
        }

        #endregion

        #region Pressing

        bool isSelected = false;
        
        public void OnSelect() {
            if (isSelected) return;
            
            isSelected = true;
            
            GetCurrentContent().OnSelect();
        }

        public void OnUnselect() {
            if (!isSelected) return;
            
            isSelected = false;
            
            GetCurrentContent().OnUnselect();
        }
        
        #endregion
        
        TempSlotModule slotModule = null;
        
        public SlotModule GetTempModule() {
            if (slotModule == null)
                slotModule = new TempSlotModule(this);
            return slotModule;
        }
        
        public Rect GetRect() {
            var result = new Rect(position, Vector2.zero).Resize(Offset);
            
            GetAllContent()
                .CastIfPossible<ISlotRectModifier>()
                .ForEach(m => result = m.ModifySlotRect(result));
            
            return result;
        }
    }    
    
    public interface ISlotRectModifier {
        Rect ModifySlotRect(Rect slotRect);
    }
}


