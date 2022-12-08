using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Controls;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.UI;
using Behaviour = Yurowm.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class LevelGameplay : LevelContent, ILocalized {
        
        public string scoreEffect;
        
        public string slotRenderer;
        public string slotHighlighter;

        public int matchDate = 0;
        public Block.BlockingType nullSlotBlockingType = Block.BlockingType.Obstacle;
        
        public LocalizedText shuffleText = new LocalizedText();
        public LocalizedText shuffleFailReason = new LocalizedText();

        public abstract ColorBake GetColorBake();
        
        public override Type GetContentBaseType() {
            return typeof (LevelGameplay);
        }
        
        public ItemColorPalette colorPalette;
        public Slots slots;
        public Score score;
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            random = field.random.NewRandom("Gameplay");
            
            GetColorBake().Bake(context);
            
            slots = New<Slots>(slotRenderer);
            field.AddContent(slots);
            field.fieldContext.SetArgument(slots);
            
            colorPalette = context.Get<ItemColorPalette>();

            space.context.SetupItem(out score);

            DebugPipeline().Run(field.coroutine);
            
            SetupControl();
        }

        public override void OnRemoveFromSpace(Space space) {
            ClearControl();
            base.OnRemoveFromSpace(space);
        }

        #region Controls
        
        TouchControl control;
        
        public void SetupControl() {
            ClearControl();
            space.context.Catch<CameraOperator>(o => {
                control = o.controls;
                SetupControl(control);
            });
        }

        protected abstract void SetupControl(TouchControl control);

        public void ClearControl() {
            if (control != null) {
                ClearControl(control);
                control = null;
            }
        }
        
        protected abstract void ClearControl(TouchControl control);
        
        #endregion

        #region Counters

        int turns = 0;
        public int Turns => turns;

        public void NextTurn() {
            turns++;
        }

        #endregion

        #region Pipeline
        
        public void Start() {
            if (taskProcessor != null)
                return;
            
            InternalTaskProcessor()
                .Run(field.coroutine);
        }
        
        IEnumerator taskProcessor;
        
        List<InternalTask> completeTasks = new List<InternalTask>();
        List<InternalTask> internalTasks = new List<InternalTask>();
        List<ExternalTask> externalTasks = new List<ExternalTask>();
        Task currentTask = null;
        
        public Task GetCurrentTask() {
            return currentTask;
        }
        
        T EmitTask<T>() where T : Task {
            var result = completeTasks.CastOne<T>();
            if (result == null) {
                result = Activator.CreateInstance<T>();
                result.gameplay = this;
                result.field = field;
                result.OnCreate();
            } else if (result is InternalTask it)
                completeTasks.Remove(it);
            
            return result;
        }
        
        public void NewTask<T>() where T : InternalTask {
            internalTasks.Add(EmitTask<T>());
        }
        
        public void NextTask<T>() where T : InternalTask {
            internalTasks.Insert(0, EmitTask<T>());
        }
        
        public ExternalTask NewExternalTask() {
            var result = EmitTask<ExternalTask>();
            externalTasks.Add(result);
            return result;
        }
        
        public Task GetNextTask() {
            return internalTasks.FirstOrDefault();
        }
        
        public IEnumerator WaitForTask<T>(Func<T, bool> filter = null) where T : InternalTask {
            while (!(currentTask is T t && (filter == null || filter.Invoke(t))))
                yield return null;
        }

        IEnumerator InternalTaskProcessor() {
            events.onLevelStart();
            
            internalTasks.Clear();
            
            SetState(State.Playing, true);
            
            NextTask<GravityTask>();
            
            while (true) {
                currentTask = internalTasks.Grab();
                if (currentTask != null) {
                    if (currentTask is InternalTask it) 
                        completeTasks.Add(it);
                    events.onStartTask(currentTask);
                    yield return currentTask.Logic();
                } else
                    yield return null;
            }
        }
        
        public IEnumerator DoExternalTasks() {
            if (externalTasks.IsEmpty())
                return null;
            
            return ExternalTaskProcessor();
        }

        IEnumerator ExternalTaskProcessor() {
            while (!externalTasks.IsEmpty()) {
                yield return externalTasks.Grab().Logic();
                yield return null;
            }
        }

        public abstract IEnumerator Matching();
        
        public abstract class Task {
            public Field field;
            public LevelGameplay gameplay;
            public LiveContext context => gameplay.context;
            public LevelEvents events;

            public virtual void OnCreate() {
                events = context.GetArgument<LevelEvents>();
            }
            
            public abstract IEnumerator Logic();

            public override string ToString() {
                return GetType().Name;
            }
        }
        
        public abstract class InternalTask : Task {
        }
        
        #endregion

        #region Debug

        IEnumerator DebugPipeline() {
            if (!DebugPanel.IsActive)
                yield break;
            
            while (IsAlive()) {
                DebugPanel.Log("Current Task", "Gameplay", currentTask);
                DebugPanel.Log("Internal Tasks", "Gameplay", internalTasks.Select(t => t.ToString()).Join("\n"));
                DebugPanel.Log("External Tasks", "Gameplay", externalTasks.Count);
                DebugPanel.Log("Score", "Gameplay", $"{score.value} ({score.StarIsReached(1)}, {score.StarIsReached(2)}, {score.StarIsReached(3)})");
                OnDebugPipeline();
                yield return null;
            }
        }
        
        protected virtual void OnDebugPipeline() {}

        #endregion

        #region Hints
        
        public float hintDelay = 10;
        
        public List<object> hintLockers = new List<object>();
        
        public virtual void ShowHint() { }
        
        public virtual void HideHint() { }

        #endregion

        #region Goals

        public bool IsLevelComplete() {
            return state.HasFlag(State.LevelComplete);
        }
        
        public bool IsLevelFailed() {
            return state.HasFlag(State.LevelFailed);
        }
        
        public bool IsGoalFailed() {
            return !state.HasFlag(State.GoalsComplete) && context.GetAll<LevelGoal>().Any(g => g.IsFailed());
        }
        
        public void SetFailReason(FailReason reason) {
            string text = string.Empty;

            switch (reason) { 
                case FailReason.Shuffle: text = shuffleFailReason.GetText(); break;
                case FailReason.Context: text = context
                    .GetAll<IFailReason>()
                    .Select(i => i.GetFailReason())
                    .FirstOrDefault(r => !r.IsNullOrEmpty()); break;
            }
            
            Behaviour
                .GetAllByID<LabelFormat>("LevelFailed.Reason")
                .ForEach(lf => lf["value"] = text);
        }
        
        public bool AreGoalsComplete() {
            if (state.HasFlag(State.GoalsComplete))
                return true;
            
            bool hasGoals = false;

            foreach (var goal in context.GetAll<LevelGoal>()) {
                hasGoals = true;
                if (!goal.IsComplete())
                    return false;
            }
            
            if (!hasGoals && AllowToMove())
                return false;

            SetState(State.GoalsComplete, true);
            return true;
        }

        #endregion

        #region Turns
        
        public abstract bool IsThereAnyPotentialTurns();
        
        public bool AllowToMove() {
            foreach (var limitation in space.context.GetAll<LevelLimitation>()) {
                if (!limitation.AllowToMove())
                    return false;
            }
            return true;
        }

        public Action onNextTurn = delegate {};
        
        public void OnNextTurn() {
            NextTurn();            
            events.onMoveSuccess();
            space.context.GetAll<LevelLimitation>().ForEach(l => l.OnBonusCreated());
            onNextTurn.Invoke();
        }

        #endregion

        #region Shuffle
        
        public List<object> shuffleLockers = new List<object>();

        public bool Shuffle(bool immediate = false) {
            return Shuffle(field.slots.all.Values
                .Select(s => s.GetCurrentContent())
                .CastIfPossible<Chip>()
                .Where(c => c is IShuffled && c is IColored colored && colored.colorInfo.type == ItemColor.Known)
                .ToArray(), immediate);
        }    
        
        public bool Shuffle(Chip[] chips, bool immediate = false) {    
            if (!shuffleLockers.IsEmpty())
                return false;

            chips = chips?
                .NotNull()
                .Distinct()
                .ToArray();

            if (chips.IsEmpty())
                return false;
            
            var defaultSlots = chips
                .ToDictionaryValue(c => c.slotModule.Slot());
            
            var slots = defaultSlots.Values.ToArray();

            for (int i = 0; i < 100; i++) {
                chips.ForEach(c => c.BreakParent());
                chips = chips.Shuffle(random, $"Shuffle_{i}").ToArray();
                for (int s = 0; s < slots.Length; s++)
                    slots[s].AddContent(chips[s]);
                if (!IsThereAnySolutions() && IsThereAnyPotentialTurns()) {
                    if (immediate) 
                        chips.ForEach(c => c.localPosition = Vector2.zero);
                    lcAnimator.PlaySound("Shuffle");
                    return true;
                }
            }
            
            chips.ForEach(c => c.BreakParent());
            chips.ForEach(c => defaultSlots[c].AddContent(c));

            return false;
        }

        public IEnumerable GetLocalizationKeys() { 
            yield return shuffleText;
            yield return shuffleFailReason;
        }

        #endregion

        #region Solutions

        public abstract bool IsThereAnySolutions();

        #endregion

        #region Playing Mode and State

        [Flags]
        public enum State {
            Default = 0,
            Playing = 1 << 0,
            WaitTurn = 1 << 1,
            LevelComplete = 1 << 2,
            LevelFailed = 1 << 3,
            GoalsComplete = 1 << 4
        }

        public bool IsPlaying() {
            return state.HasFlag(State.Playing);
        }

        public State state { get; private set;} = State.Default;
        
        public void SetState(State s, bool value) {
            if (value)
                state |= s;
            else 
                state &= ~s;
        }
        
        #endregion

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("hintDelay", hintDelay);
            writer.Write("nullSlotBlockingType", nullSlotBlockingType);
            writer.Write("slotRenderer", slotRenderer);
            writer.Write("scoreEffect", scoreEffect);
            writer.Write("slotHighlighter", slotHighlighter);
            writer.Write("shuffleText", shuffleText);
            writer.Write("shuffleFailReason", shuffleFailReason);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("hintDelay", ref hintDelay);
            reader.Read("nullSlotBlockingType", ref nullSlotBlockingType);
            reader.Read("slotRenderer", ref slotRenderer);
            reader.Read("scoreEffect", ref scoreEffect);
            reader.Read("slotHighlighter", ref slotHighlighter);
            reader.Read("shuffleText", ref shuffleText);
            reader.Read("shuffleFailReason", ref shuffleFailReason);
        }

        #endregion

        public bool IsWaitingForPlayerTurn() {
            return state.HasFlag(State.WaitTurn);
        }
    }
}