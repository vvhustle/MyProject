using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using YMatchThree.Core;
using YMatchThree.Seasons;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Jobs;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.UI;
using Yurowm.Utilities;
using Behaviour = Yurowm.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree {
    public class Field : GameEntity, ISelfUpdate, ILiveContextHolder {
        public readonly LiveContext fieldContext;

        PuzzleSpace space;
        Level level;

        public SpaceCamera camera;
        public LevelEvents events;
        
        public LevelGameplay gameplay;
        
        public Slots slots;
        
        public Transform root;
        public bool complete = false;
        
        public Field(Level level, PuzzleSpace space) {
            this.level = level;
            this.space = space;

            if (level.randomSeed == 0)
                level.randomSeed = YRandom.main.Seed();
            
            random = new YRandom(level.randomSeed);
            
            fieldContext = new LiveContext("LevelField");
            
            fieldContext.SetArgument(this);
            
            fieldContext.SetArgument(level);
            
            fieldContext.SetArgument(space.time);
            
            fieldContext.SetArgument(space);
            fieldContext.SetArgument<Space>(space);
            
            events = new LevelEvents();
            fieldContext.SetArgument(events);
            
            SetSimulation(new GeneralPuzzleSimulation());
        }

        public area Area => level.Area;
        public SlotLayerBase interactionLayer = null;

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            root = new GameObject($"Field").transform;
            root.SetParent(space.root);
            root.Reset();
            root.position = Vector3.zero;
            root.localScale = Vector3.zero;
            context.Catch<SpaceCamera>(c => camera = c);
        }
        
        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            fieldContext.Destroy();
            root?.Destroy();
            RectClear();
            FieldBooster.ClearBoosterLists();
        }
        
        public void Start() {
            fieldContext
                .Get<LevelGameplay>()
                .Start();
        }
        
        public IEnumerator BuildStage () {
            root.gameObject.SetActive(false);
            
            var access = new DelayedAccess(1f / 40);

            var script = context.GetArgument<LevelScriptBase>();
            fieldContext.SetArgument(script);
            
            var colorSettings = script.colorSettings.Clone();
            
            if (colorSettings.colorPalette == null)
                colorSettings.colorPalette = LevelContent.storage.
                    GetDefault<ItemColorPalette>()?.Clone();

            colorSettings.Initialize();
            
            fieldContext.SetArgument(colorSettings);
            AddContent(colorSettings.colorPalette);

            gameplay = LevelContent.GetItem<LevelGameplay>(level.gamePlay).Clone();
            AddContent(gameplay);
            
            slots = gameplay.slots;
            
            AddContent(LevelContent.GetItem<ChipPhysic>(level.physic).Clone());
            
            AddContent(new ChipGeneratorController());
            
            #region Creating new empty slot
            
            var defaultLayer = level.layers.FirstOrDefault(l => l.isDefault);
            
            foreach (var slotInfo in level.slots.Where(s => defaultLayer.Contains(level, s))) {
                CreateSlot(slotInfo);

                if (access.GetAccess()) yield return null;
            }
            
            #endregion
            
            foreach (var slotInfo in level.slots) {
                if (slots.all.TryGetValue(slotInfo.coordinate, out var slot)) {
                    FillSlot(slot, slotInfo);

                    if (access.GetAccess()) yield return null;
                }
            }
            
            slots.FirstBake();
            
            foreach (var extensionInfo in level.extensions) {
                var content = extensionInfo.Reference.Clone();

                content.ApplyDesign(extensionInfo);
                    
                AddContent(content);

                if (access.GetAccess()) yield return null;
            }
            
            root.gameObject.SetActive(true);

            space.clickables.SetCoordinateSystem(root);
        }
        
        public bool IsInteractable(Slot slot) {
            return interactionLayer?.Contains(level, slot.coordinate) ?? true;
        }

        #region Align to UI Rect
        
        bool animate;

        UIAreaRect[] uiAreaRects;
        
        void RectClear() {
            uiAreaRects?.ForEach(r => 
                r.onChanged.RemoveAllListeners());
            uiAreaRects = null;
        }

        public void OnUIRectChanged() {
            if (!animate && root && GetTransform(out var transform)) 
                transform.Apply(root);
        }
        
        IEnumerator Move(FieldTransform targetTransform, float duration,
            EasingFunctions.Easing easing = EasingFunctions.Easing.Linear) {
            
            while (animate || Page.IsAnimating) 
                yield return null;
            
            var currentTransform = new FieldTransform {
                position = root.position,
                scale = root.localScale.x
            };

            FieldTransform transform;

            using (Page.NewActiveAnimation()) {
                animate = true;
            
                for (var t = 0f; t < 1f; t += time.Delta / duration) {
                    
                    transform.position = Vector2.Lerp(
                        currentTransform.position,
                        targetTransform.position, t.Ease(easing));
                    
                    transform.scale = YMath.Lerp(
                        currentTransform.scale,
                        targetTransform.scale, t.Ease(easing));
                    
                    transform.Apply(root);
                    
                    yield return null;
                }
                
                targetTransform.Apply(root);
                
                animate = false;
            }
        }
        
        public IEnumerator RefreshPositionSmooth() {
            FieldTransform transform;

            while (!GetTransform(out transform))
                yield return null;
            
            yield return Move(transform, 1, EasingFunctions.Easing.InOutQuad);
        }
        
        bool GetTransform(out FieldTransform result, UIAreaRect area = null) {
            result = default;
            
            if (!slots)
                return false;
            
            if (!area || !area.isActiveAndEnabled)
                area = uiAreaRects?
                    .FirstOrDefault(r => r.isActiveAndEnabled);
            
            if (!area)
                return false;
            
            var worldRect = area.rectTransform.GetWorldRect();
            
            var fieldRect = slots.edge;
            
            result.scale = YMath.Min(
                worldRect.width / fieldRect.width,
                worldRect.height / fieldRect.height);
            
            result.scale = area.camSizeRange.Clamp(result.scale);

            result.position = worldRect.center - fieldRect.center * result.scale;
            
            return true;
        }

        struct FieldTransform {
            public Vector2 position;
            public float scale;

            public void Apply(Transform transform) {
                transform.position = position;
                transform.localScale = new Vector3(scale, scale, 1);
            }
        }
        
        #endregion
        
        #region Update
        
        public readonly CoroutineCore coroutine = new CoroutineCore();
        
        public void UpdateFrame(Updater updater) {
            coroutine.Update(CoroutineCore.Loop.Update);
        }

        public void ForceUpdate() {
            space.time.Update();
            UpdateFrame(null);
        }
        
        #endregion
        
        #region Content
        
        public Slot CreateAndFillSlot(SlotInfo slotInfo) {
            var result = CreateSlot(slotInfo);
            FillSlot(result, slotInfo);
            return result;
        }
        
        public void FillSlot(Slot slot, SlotInfo slotInfo) {
            foreach (var contentInfo in slotInfo.Content()) {
                if (contentInfo.Reference is SlotContent) {
                    var content = contentInfo.Reference.Clone();
                    content.ApplyDesign(contentInfo);
                    content.position = slot.position;
                    AddContent(content);
                    slot.AddContent(content as SlotContent);
                }
            }
        }

        public Slot CreateSlot(SlotInfo slotInfo) {
            var result = new Slot();
            
            result.coordinate = slotInfo.coordinate;
            result.position = (result.coordinate.ToVector2() + Vector2.one / 2) 
                            * Slot.Offset;
            
            slots.all.Add(result.coordinate, result);
            
            AddContent(result);
            
            return result;
        }
        
        public void AddContent(LevelContent content) {
            if (fieldContext.Add(content)) {
                content.space = space;
                content.field = this;
                content.random = random.NewRandom();
                
                if (content is IPuzzleSimulationSensitive pss)
                    pss.OnChangeSimulation(simulation);
                
                content.OnAddToSpace(space);
                space.onAddItem.Invoke(content);
                content.enabled = true;
                var body = content.body?.transform;
                if (body) {
                    body.SetParent(root);
                    body.Reset();
                }
            }
        }

        public void RemoveContent(LevelContent content) {
            if (content.space == space) {
                content.enabled = false;
                content.OnRemoveFromSpace(space);
                fieldContext.Remove(content);
                content.space = null;
                content.field = null;
            }
        }

        #endregion

        #region Show/Hide
        
        public IEnumerator Hiding() {
            yield return Page.WaitAnimation();
            
            FieldBooster.ClearBoosterLists();
            
            FieldTransform transform;
            
            while (!GetTransform(out transform))
                yield return null;
            
            RectClear();
            
            transform.position += Vector2.left * 10;
            
            yield return Move(transform, 1, EasingFunctions.Easing.InCubic);
        }
        
        public IEnumerator Showing() {
            yield return Page.WaitAnimation();
            
            uiAreaRects = Behaviour
                .GetAllByID<UIAreaRect>("FieldRect")
                .ToArray();
            
            root.position = Vector3.zero;
            root.localScale = Vector3.zero;
            
            FieldTransform transform;
            
            while (!GetTransform(out transform))
                yield return null;
            
            var startTransform = transform;
            startTransform.position += new Vector2(10, 0);
            
            startTransform.Apply(root);
            
            yield return Move(transform, 1, EasingFunctions.Easing.InCubic);

            FieldBooster.FillBoosterLists(this);
            
            uiAreaRects.ForEach(r => 
                r.onChanged.SetSingleListner(OnUIRectChanged));
            
            OnUIRectChanged();
        }
        
        #endregion

        #region Effects
        
        public void Explode(Vector2 center, ExplosionParameters parameters) {
            if (!simulation.AllowEffects() || parameters == null) 
                return;
            
            var radius = parameters.radius * Slot.Offset;
            
            foreach (var slot in slots.all.Values) {
                if (!(slot.GetCurrentContent() is Chip chip)) 
                    continue;
                if ((chip.position - center).MagnitudeIsGreaterThan(radius))
                    continue;
                var impuls = (chip.position - center) * parameters.force;
                impuls *= Mathf.Pow((radius - (chip.position - center).magnitude) / radius, 2);
                chip.AddImpuls(impuls);
            }
        }
        
        public void Highlight(Slot[] slots, Slot center) { 
            Highlight(slots, center, ItemColorInfo.None);
        }

        public void Highlight(Slot[] slots, Slot center, ItemColorInfo colorInfo) {
            if (!simulation.AllowEffects()) 
                return;
            if (gameplay.slotHighlighter.IsNullOrEmpty())
                return;
            Effect.Emit(this, gameplay.slotHighlighter, center.position, new SlotHighlighterLogicProvider.Callback {
                slots = slots,
                center = center,
                colorInfo = colorInfo
            });
        }
        
        public IEnumerator Feedback(LocalizedText text) {
            var label = ObjectTag.Get<TMP_Text>("Feedback");

            if (!label) yield break;
            
            label.text = text.GetText();

            if (label.SetupComponent(out ContentAnimator animator))
                yield return animator.PlayAndWait("Show");
            else
                yield return time.Wait(1);
        }
        
        #endregion

        #region Simulation
        
        public PuzzleSimulation simulation {private set; get;}

        public void SetSimulation(PuzzleSimulation simulation) {
            this.simulation = simulation;
            fieldContext
                .GetAll<IPuzzleSimulationSensitive>()
                .ForEach(pps => pps.OnChangeSimulation(simulation));
        }

        #endregion
        
        #region ILiveContextHolder
        
        public LiveContext GetContext() {
            return fieldContext;
        }

        #endregion
    }
    
    public interface IPuzzleSimulationSensitive : ILiveContexted {
        void OnChangeSimulation(PuzzleSimulation simulation);
    }
}