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
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class MatchThreeGameplay : LevelGameplay {

        public ExplosionParameters swapBouncing = new ExplosionParameters();
        public ExplosionParameters matchBouncing = new ExplosionParameters();
        public ExplosionParameters selectBouncing = new ExplosionParameters();

        public override ColorBake GetColorBake() {
            return new MatchThreeColorBake();
        }

        protected override void OnDebugPipeline() {
            base.OnDebugPipeline();
            DebugPanel.Log("Swapping", "Gameplay", swapping);
        }

        public override IEnumerator Matching() {
            var solutions = FindSolutions();
            if (solutions.Count > 0)
                yield return MatchSolutions(solutions);
        }
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            events.onStartTask += OnStartTask;
        }
        
        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            events.onStartTask -= OnStartTask;
        }

        void OnStartTask(Task task) {
            if (task is WaitTask)
                slots.ClearSelection();
        }
        
        #region Controls
        
        Dictionary<int, TouchToSwap> touchToSwap = new Dictionary<int, TouchToSwap>();
        
        protected override void SetupControl(TouchControl control) {
            control.onTouchBegin += OnTouchBegin;
            control.onTouch += OnTouch;
            control.onTouchEnd += OnTouchEnd;
        }

        protected override void ClearControl(TouchControl control) {
            control.onTouchBegin -= OnTouchBegin;
            control.onTouch -= OnTouch;
            control.onTouchEnd -= OnTouchEnd;
        }
        
        void OnTouchBegin(TouchStory touch) {
            touchToSwap.Set(touch.fingerId, new TouchToSwap(this));
        }
        
        void OnTouch(TouchStory touch) {
            touchToSwap.Get(touch.fingerId)?.OnTouch(touch);
        }
        
        void OnTouchEnd(TouchStory touch) {
            touchToSwap.Get(touch.fingerId)?.OnTouch(touch);
        }
        
        Chip selectedChip {
            get {
                if (slots.selection.IsEmpty())
                    return null;
                return slots.selection[0].GetCurrentContent() as Chip;
            }
        }
        
        void TouchClick(TouchStory touch) {
            TouchClickLogic(touch).Run(field.coroutine);
        }
        
        IEnumerator TouchClickLogic(TouchStory touch) {
            if (IsLevelComplete() || !IsWaitingForPlayerTurn()) 
                yield break;
            
            var clickedSlot = gameplay.space.clickables
                .Cast<Slot>(touch.currentWorldPosition, Slot.Offset);
            
            if (TurnBlocker(clickedSlot)) yield break;
            
            var clickedChip = clickedSlot?.GetCurrentContent() as Chip;
            
            if (!clickedChip) yield break;

            if (selectedChip) {
                var c = selectedChip;
                var s = slots.selection[0];
                slots.ClearSelection();
                    
                if (c == clickedChip) {
                    yield return ActivateByPlayer(BombActivationType.DoubleTap, 
                        OnNextTurn,
                        clickedSlot);
                    yield break;
                }

                if (clickedSlot.coordinate.FourSideDistanceTo(s.coordinate) == 1) {
                    SwapByPlayer(s, clickedSlot);
                    yield break;
                }
            } 
            
            slots.Select(clickedSlot);
            field.Explode(clickedSlot.position, selectBouncing);
        }
        
        class TouchToSwap {
            IEnumerator logic;
            TouchStory touch;
            MatchThreeGameplay gameplay;

            public TouchToSwap(MatchThreeGameplay gameplay) {
                this.gameplay = gameplay;
                logic = ControlLogic();
            }
            
            IEnumerator ControlLogic() {
                if (!gameplay)
                    yield break;
                
                var clickables = gameplay.space.clickables;
                
                Slot PickSlot() => clickables
                    .Cast<Slot>(touch.currentWorldPosition, Slot.Offset);
                
                var firstPickedSlot = PickSlot();

                if (!firstPickedSlot)
                    yield break;

                while (true) {
                    var offset = clickables.GetPoint(touch.currentWorldPosition) - firstPickedSlot.position;
                    
                    if (touch.IsComplete) {
                        if (firstPickedSlot == PickSlot()) {
                            gameplay.TouchClick(touch);
                            yield break;
                        }
                            
                    }
                    if (offset.MagnitudeIsGreaterThan(Slot.Offset / 2)) {
                        var side = Sides.straight.FirstOrDefault(s => Vector2.Angle(offset, s.ToVector2()) <= 45);
                        gameplay.SwapByPlayer(firstPickedSlot, side);
                        yield break;
                    }
                    yield return null;
                }                
            }
            
            public void OnTouch(TouchStory touch) {
                this.touch = touch;
                logic.MoveNext();
            }
        }

        #endregion

        #region Swapping
        
        public float swapDuration = 0.2f;
        public float swapOffset = 0.2f;
        
        bool swapping;
        
        List<Chip> lastSwaped = new List<Chip>();

        void SwapByPlayer(Slot slot, Side side) {
            SwapByPlayer(slot, slot[side]);
        }       
        
        void SwapByPlayer(Slot slotA, Slot slotB) {
            if (!IsLevelComplete() && IsWaitingForPlayerTurn()) 
                if (slotA && slotB)
                    SwapByPlayerRoutine(slotA, slotB).Run(field.coroutine);
        }
        
        bool TurnBlocker(params Slot[] slots) {
            if (!IsPlaying()) return true;

            if (!AllowToMove()) return true;
            
            if (slots.Any(s => !s || !s.IsInteractable()))
                return true;
            
            if (slots.Any(s => !(s.GetCurrentContent() is Chip)))
                return true;
            
            return false;
        }
        
        IEnumerator SwapByPlayerRoutine(Slot a, Slot b) {
            if (swapping) yield break;

            if (a == b) yield break;

            if (TurnBlocker(a, b))
                yield break;
            
            if (a.coordinate.FourSideDistanceTo(b.coordinate) != 1) 
                yield break;
            
            if (!a.nearSlot.GetKey(b).IsStraight() || !b.nearSlot.GetKey(a).IsStraight())
                yield break;
            
            var chipA = a.GetCurrentContent() as Chip;
            var chipB = b.GetCurrentContent() as Chip;
            
            var chipPair = new Pair<string>(chipA.ID, chipB.ID);
           
            swapping = true;
            
            bool success = false;
            
            chipA.lockPosition = true;
            chipB.lockPosition = true;
            
            var posA = a.position;
            var posB = b.position;

            float progress = 0;
            float time = 0;
            
            var mixInfo = mixes.FirstOrDefault(x => x.pair == chipPair);
            
            lcAnimator.PlaySound("SwapBegin");
            
            if (mixInfo != null) {

                #region Mixing

                field.Explode(posA, swapBouncing);
                
                while (progress < swapDuration) {
                    time = EasingFunctions.InOutQuad(progress / swapDuration);
            
                    chipA.position = Vector2.Lerp(posA, posB, time);
            
                    progress += Time.deltaTime;
            
                    yield return null;
                }
                
                var mix = storage.GetItemByID<ChipMix>(mixInfo.mix).Clone();
                mix.random = random.NewRandom();
                mix.position = chipB.position;
                mix.slot = chipB.slotModule.Slot();
                mix.SetChips(chipB, chipA);
                
                matchDate++;
                
                HideHint();
                
                field.AddContent(mix);
                
                #endregion

                success = true;
            } else {

                #region Swapping

                var normal = a.coordinate.X == b.coordinate.X ? Vector2.right : Vector2.up;
                
                field.Explode(posA, swapBouncing);
                
                while (progress < swapDuration) {
                    time = EasingFunctions.InOutQuad(progress / swapDuration);
                
                    chipA.position = Vector2.Lerp(posA, posB, time) + normal * (Mathf.Sin(Mathf.PI * time) * swapOffset);
                    chipB.position = Vector2.Lerp(posB, posA, time) - normal * (Mathf.Sin(Mathf.PI * time) * swapOffset);
                
                    progress += Time.deltaTime;
                
                    yield return null;
                }
                
                chipA.position = posB;
                chipB.position = posA;
                
                a.AddContent(chipB);
                b.AddContent(chipA);
                
                #endregion
                
                var activated = false;
                
                yield return ActivateByPlayer(BombActivationType.Swap,
                    () => {
                        activated = true;
                        success = true;
                    },
                    a, b);
                
                if (!activated) {
                    int count = MatchAnaliz(a)?.count ?? 0;
                    count += MatchAnaliz(b)?.count ?? 0;
                    
                    if (count == 0) {

                        #region Swap Cancelation
                        
                        lcAnimator.PlaySound("SwapFailed");
                        
                        field.Explode(posA, swapBouncing);
                        
                        while (progress > 0) {
                            time = EasingFunctions.InOutQuad(progress / swapDuration);
                            chipA.position = Vector2.Lerp(posA, posB, time) - normal * (Mathf.Sin(Mathf.PI * time) * swapOffset);
                            chipB.position = Vector2.Lerp(posB, posA, time) + normal * (Mathf.Sin(Mathf.PI  * time) * swapOffset);
                    
                            progress -= Time.deltaTime;
                    
                            yield return null;
                        }
                    
                        a.position = posA;
                        b.position = posB;
                    
                        b.AddContent(chipB);
                        a.AddContent(chipA);
                        
                        #endregion
                    
                    } else
                        success = true;
                }
            }
            
            if (success) {
                OnNextTurn();
                lastSwaped.Clear();
                lastSwaped.Add(chipA);
                lastSwaped.Add(chipB);
            }
            
            chipA.lockPosition = false;
            chipB.lockPosition = false;

            swapping = false;
        }

        IEnumerator ActivateByPlayer(BombActivationType activationType, Action onSuccess, params Slot[] slots) {
            if (TurnBlocker(slots))
                yield break;
            
            var bombs = slots
                .Select(s => s.GetCurrentContent())
                .CastIfPossible<BombChipBase>()
                .Where(b => b.activationType.HasFlag(activationType))
                .ToArray();
            
            if (bombs.IsEmpty())
                yield break;

            gameplay.NextTask<MatchingTask>();

            var pool = MatchingPool.Get(field.fieldContext);
            
            yield return pool.WaitOpen();
                
            using (pool.Use()) {
                var hitContext = new HitContext(context,
                    bombs.Select(b => b.slotModule.Slot()).ToArray(),
                    HitReason.Player);
                bombs.ForEach(b => 
                    b.HitAndScore(hitContext));
            }
            
            onSuccess?.Invoke();
        }
        
        #endregion

        #region Turns

        public override bool IsThereAnyPotentialTurns() {
            return FindMoves().Any();
        }

        public override bool IsThereAnySolutions() {
            return FindSolutions().Count > 0;
        }
        
        List<Solution> FindSolutions() {
            return slots.all.Values
                                   .Select(MatchAnaliz)
                                   .NotNull()
                                   .ToList();
        }
        
        #endregion

        #region Solutions

        Solution MatchAnaliz(Slot slot) {

            if (!slot) return null;

            SlotContent content = slot.GetCurrentContent();
            if (!content) return null;

            if (!(content is IColored colored))
                return null;

            var currentColorInfo = colored.colorInfo;

            if (currentColorInfo.type == ItemColor.Universal) { // multicolor
                var solutions = new List<Solution>();
                var colors = Sides.straight
                    .Select(s => slot[s])
                    .NotNull()
                    .Select(s => s.GetCurrentContent())
                    .CastIfPossible<IColored>()
                    .Where(c => c.colorInfo.IsKnown())
                    .Select(c => c.colorInfo);

                foreach (var color in colors) {
                    colored.colorInfo = color;
                    var solution = MatchAnaliz(slot);
                    if (solution != null) 
                        solutions.Add(solution);
                }

                colored.colorInfo = ItemColorInfo.Universal;
                return solutions.GetMax(x => x.potential);
            }

            var sides = new Dictionary<Side, List<SlotContent>>();
            var square = new List<SlotContent>();
            foreach (Side side in Sides.straight) {
                var count = 1;
                sides.Add(side, new List<SlotContent>());
                while (true) {
                    int2 key = slot.coordinate + side.ToInt2() * count;
                    if (!slots.all.TryGetValue(key, out Slot s)) break;

                    var currentContent = s.GetCurrentContent();
                    if (!currentContent) break;

                    if (currentContent is IColored icolored && icolored.colorInfo.IsMatchWith(colored.colorInfo)) {
                        sides[side].Add(currentContent);
                        count++;
                    } else
                        break;
                }
            }

            if (squaresMatch) {
                var zSqaure = new SlotContent[3];
                foreach (Side side in Sides.straight) {
                    for (int r = 0; r <= 2; r++) {
                        var key = slot.coordinate + side.Rotate(r).ToInt2();
                        if (!slots.all.TryGetValue(key, out var _slot))
                            break;

                        var currentContent = _slot.GetCurrentContent();
                        if (!currentContent)
                            break;

                        if (currentContent is IColored icolored && icolored.colorInfo.IsMatchWith(colored.colorInfo)) {
                            zSqaure[r] = currentContent;
                            if (r == 2)
                                square.AddRange(zSqaure);
                        } else 
                            break;
                    }
                }

                square = square.Distinct().ToList();
            } else
                square.Clear();

            SolutionType solutionType = 0;
            
            if (sides[Side.Right].Count + sides[Side.Left].Count >= 2)
                solutionType |= SolutionType.Horizontal;
            
            if (sides[Side.Top].Count + sides[Side.Bottom].Count >= 2)
                solutionType |= SolutionType.Vertical;
            
            if (square.Count > 0)
                solutionType |= SolutionType.Square;

            if (solutionType != 0) {

                var solutionContent = new List<SlotContent>();
                solutionContent.Add(slot.GetCurrentContent());

                if (solutionType.HasFlag(SolutionType.Horizontal)) {
                    solutionContent.AddRange(sides[Side.Right]);
                    solutionContent.AddRange(sides[Side.Left]);
                }
                
                if (solutionType.HasFlag(SolutionType.Vertical)) {
                    solutionContent.AddRange(sides[Side.Top]);
                    solutionContent.AddRange(sides[Side.Bottom]);
                }
                
                if (solutionType.HasFlag(SolutionType.Square))
                    solutionContent.AddRange(square);

                var solution = new Solution(solutionContent, context);

                solution.type = solutionType;

                solution.center = slot.coordinate;
                solution.colorInfo = currentColorInfo;

                foreach (var _ in solution.contents)
                    solution.potential += 1;
                
                return solution;
            }
            return null;
        }
        
        IEnumerator MatchSolutions(List<Solution> solutions) {
            if (!IsPlaying()) yield break;
            
            solutions.Sort((x, y) => y.potential.CompareTo(x.potential));
            
            var level = context.GetArgument<Level>();
            
            area levelArea = level.Area;

            var mask = new bool[levelArea.width, levelArea.height];

            slots.all.Values
                .Where(s => s.GetCurrentContent() is IColored)
                .ForEach(s => mask[s.coordinate.X, s.coordinate.Y] = true);
            
            var finalSolutions = new List<Solution>();

            foreach (var s in solutions) {
                var breaker = false;
                foreach (var c in s.contents) {
                    if (!mask[c.slotModule.Center.X, c.slotModule.Center.Y]) {
                        breaker = true;
                        break;
                    }
                }
                if (breaker) continue;

                finalSolutions.Add(s);

                s.contents.ForEach(c => mask[c.slotModule.Center.X, c.slotModule.Center.Y] = false);
            }

            matchDate++;
            int birthDate = matchDate;
            foreach (var solution in finalSolutions) {
                solution.bombSlot = solution.GetCenteredSlot();
                foreach (var combination in combinations) {
                    if (combination.count > solution.count) continue;
                    if (!combination.type.HasFlag(Combination.Type.Any)) {
                        
                        if (combination.type.HasFlag(Combination.Type.Square))
                            if (!squaresMatch || !solution.type.HasFlag(SolutionType.Square)) continue;
                        
                        if (combination.type.HasFlag(Combination.Type.Lined)) {
                            if (combination.type.HasFlag(Combination.Type.Horizontal) && !solution.type.HasFlag(SolutionType.Horizontal)) 
                                continue;
                            if (combination.type.HasFlag(Combination.Type.Vertical) && !solution.type.HasFlag(SolutionType.Vertical)) 
                                continue;
                            if (combination.type.HasFlag(Combination.Type.SingleLine) && 
                                !(solution.type.HasFlag(SolutionType.Vertical) ^ solution.type.HasFlag(SolutionType.Horizontal))) 
                                continue;
                        }
                    }

                    solution.bomb = storage.items.CastIfPossible<Chip>().FirstOrDefault(c => c.ID == combination.bomb);
                    
                    if (solution.bomb && !solution.bombSlot.IsDefault)
                        solution.bombSlot = solution.GetCenteredSlot(x => x.IsDefault);
                    
                    break;
                }
            }

            foreach (var solution in finalSolutions) {
                int score = 0;
                var hitContext = new HitContext(context, 
                    solution.contents.SelectMany(c => c.slotModule.Slots().Distinct()),
                    HitReason.Matching);
                foreach (var c in solution.contents) {
                    c.birthDate = -1;
                    score += c.slotModule.Slots().Sum(s => s.Hit(hitContext));
                }
                
                var centerSlot = solution.bombSlot ?? hitContext.group.GetRandom(random);
                    
                this.score.AddScore(score);
                
                events.onMatchSoultion(solution);
                
                centerSlot.ShowScoreEffect(score, solution.colorInfo);

                field.Highlight(hitContext.group, centerSlot);
            }

            if (finalSolutions.Count == 0)
                yield break;

            foreach (var solution in finalSolutions)
                if (solution.bomb && solution.bombSlot) {
                    var bomb = solution.bomb.Clone();
                    bomb.birthDate = birthDate;
                    bomb.position = solution.bombSlot.position;
                    if (bomb is IColored)
                        bomb.SetupVariable(new ColoredVariable {
                            info = solution.colorInfo
                        });
                    bomb.emitType = SlotContent.EmitType.Matching;
                    field.AddContent(bomb);
                    solution.bombSlot.AddContent(bomb);
                }
            
            if (simulation.AllowAnimations())
                for (var t = 0f; t <= 1f; t += time.Delta * 2) {
                    foreach (var solution in finalSolutions)
                    foreach (var ch in solution.contents
                        .Where(c => c.IsAlive() && c.IsDefault && c.slotModule.HasSlot()))
                        ch.position = Vector2.Lerp(
                            ch.slotModule.Slot().position, 
                            solution.bombSlot.position, 
                            t.Ease(EasingFunctions.Easing.InCubic));
                    yield return null;
                }

            foreach (var solution in finalSolutions)
                if (solution.bombSlot) 
                    field.Explode(solution.bombSlot.position, matchBouncing);
        }

        [Flags]
        public enum SolutionType {
            Horizontal = 1 << 1, // T + B + X >= 3
            Vertical = 1 << 2, //L + R + X >= 3
            Square = 1 << 3,
            Cross = Horizontal | Vertical
        }
        
        public class Solution {

            public int count => contents.Count;
            public int potential;
            public ItemColorInfo colorInfo;
            public List<SlotContent> contents;
            public Chip bomb;
            public Slot bombSlot;

            // center of solution
            public int2 center;

            public SolutionType type = 0;
            
            LiveContext context;
            
            public Solution(IEnumerable<SlotContent> contents, LiveContext context) {
                this.context = context;
                this.contents = contents.Distinct().ToList();
            }

            public Slot GetCenteredSlot(Func<SlotContent, bool> condition = null) {
                if (condition == null)
                    condition = NullCondition;
                
                context.SetupItem(out MatchThreeGameplay gameplay);
                
                SlotContent result = null;
                
                if (bomb) {
                    result = contents.FirstOrDefault(c => condition(c) && gameplay.lastSwaped.Contains(c));
                    if (result) 
                        return result.slotModule.Slot();
                }
                
                if (type.HasFlag(SolutionType.Square)) {
                    Slot slot = gameplay.slots.all[center];
                    
                    if (condition(slot.GetCurrentContent()))
                        return slot;
                    
                    var random = contents.FirstOrDefault()?.field.random;
                    
                    result = contents.Where(condition).GetRandom(random);
                        
                    if (!result) 
                        result = contents.GetRandom(random);
                        
                    return result?.slotModule.Slot();
                }

                int minDistance = int.MaxValue;
                
                foreach (SlotContent content in contents.Where(condition)) {
                    int2 coordA = content.slotModule.Slot().coordinate;
                    int distance = 0;
                    
                    foreach (SlotContent contentToCheck in contents.Where(condition)) {
                        if (content == contentToCheck) continue;
                        Slot slotB = contentToCheck.slotModule.Slot();
                        var coordB = slotB.coordinate;
                        int d = coordA.FourSideDistanceTo(coordB);
                        if (distance < d)
                            distance = d;
                    }
                    
                    if (minDistance > distance) {
                        minDistance = distance;
                        result = content;   
                    }
                }
                
                return result?.slotModule.Slot();
            }
            
            static bool NullCondition(SlotContent content) {
                return true;
            }
            
        }

        #endregion

        #region Moves

        public IEnumerable<Move> FindMoves() {
            foreach (Side asix in Utils.ForEachValues(Side.Right, Side.Top)) {
                foreach (Slot slot in slots.all.Values) {
                    if (!slot.IsInteractable()) continue;
                    
                    if (slot[asix] == null || !slot[asix].IsInteractable()) continue;
                    
                    if (!(slot.GetCurrentContent() is Chip chip) || !(slot[asix].GetCurrentContent() is Chip nextChip)) continue;

                    Move move = new Move {
                        A = slot.coordinate, 
                        B = slot[asix].coordinate
                    };

                    Pair<string> pair = new Pair<string>(chip.ID, nextChip.ID);

                    ChipMixRecipe mix = mixes.FirstOrDefault(x => x.pair == pair);
                    
                    if (mix != null) {
                        move.potencial = 1000;
                        move.solution = new Solution(new [] { chip, nextChip }, context);
                        yield return move;
                        continue;
                    }

                    if ((chip is BombChipBase bomb && bomb.activationType.HasFlag(BombActivationType.Swap)) ||
                        (nextChip is BombChipBase nextBomb && nextBomb.activationType.HasFlag(BombActivationType.Swap))) {
                        move.potencial = 100;
                        move.solution = new Solution(new [] { chip, nextChip }, context);
                        yield return move;
                        continue;
                    }

                    if (chip is IColored chipColored && nextChip is IColored nextChipColored && chipColored.colorInfo.IsMatchWith(nextChipColored.colorInfo))
                        continue;

                    TemporarySwap(move);

                    Dictionary<Slot, Solution> solutions = new Dictionary<Slot, Solution>();

                    Slot[] cslots = { slot, slot[asix] };
                    foreach (Slot cslot in cslots) {
                        solutions.Add(cslot, null);

                        var potential = 0;
                        var solution = MatchAnaliz(cslot);
                        if (solution != null) {
                            solutions[cslot] = solution;
                            potential = solution.potential;
                        }

                        move.potencial += potential;
                    }

                    if (solutions[cslots[0]] != null && solutions[cslots[1]] != null)
                        move.solution = solutions[cslots[0]].potential > solutions[cslots[1]].potential ? solutions[cslots[0]] : solutions[cslots[1]];
                    else
                        move.solution = solutions[cslots[0]] ?? solutions[cslots[1]];

                    TemporarySwap(move);

                    if (move.potencial > 0)
                        yield return move;
                }
            }
        }
        
        void TemporarySwap(Move move) {
            var slotA = slots.all[move.A];
            var chipA = slotA?.GetContent<Chip>();
            if (!chipA) return;
            
            var slotB = slots.all[move.B];
            var chipB = slotB?.GetContent<Chip>();
            if (!chipB) return;

            chipA.BreakParent();
            chipB.BreakParent();
            
            slotA.AddContent(chipB);
            slotB.AddContent(chipA);
        }
        
        public class Move {
            public int2 A;
            public int2 B;

            public Solution solution;
            public int potencial;
        }

        #endregion

        #region Hints
        
        public override void ShowHint() {
            var moves = FindMoves()
                    .OrderByDescending(m => m.potencial)
                    .ToList();

            if (moves.IsEmpty())
                return;
            
            var topMatchContent = new List<SlotContent>();
            
            moves.RemoveAll(m => {
                if (topMatchContent.Intersect(m.solution.contents).Any())
                    return true;
                topMatchContent.AddRange(m.solution.contents);
                return false;
            });
            
            moves.GetRandom(random)
                .solution.contents.ForEach(c => c?.Flashing());
        }

        public override void HideHint() {
            slots.all.Values
                .ForEach(s => s.GetCurrentContent()?.StopFlashing());
        }

        #endregion

        public List<ChipMixRecipe> mixes = new List<ChipMixRecipe>();
        
        #region Combinations

        public bool squaresMatch = true;
        public List<Combination> combinations = new List<Combination>();

        public class Combination : ISerializable {
            public string bomb;
            public Type type;
            public int count = 4;

            [Flags]
            public enum Type {
                Lined = 1 << 0,
                Horizontal = 1 << 1 | Lined,
                Vertical = 1 << 2 | Lined,
                SingleLine = 1 << 3 | Lined,
                Cross = Horizontal | Vertical,
                Square = 1 << 4,
                Any = Lined | Square
            }

            public void Serialize(Writer writer) {
                writer.Write("bomb", bomb);
                writer.Write("count", count);
                writer.Write("ctype", type);
                
            }

            public void Deserialize(Reader reader) {
                reader.Read("bomb", ref bomb);
                reader.Read("count", ref count);
                reader.Read("ctype", ref type);
            }
        }

        #endregion

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("squares", squaresMatch);
            writer.Write("swapDuration", swapDuration);
            writer.Write("swapOffset", swapOffset);
            writer.Write("swapBouncing", swapBouncing);
            writer.Write("matchBouncing", matchBouncing);
            writer.Write("selectBouncing", selectBouncing);
            writer.Write("combinations", combinations.ToArray());
            writer.Write("mixes", mixes.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("squares", ref squaresMatch);
            reader.Read("swapDuration", ref swapDuration);
            reader.Read("swapOffset", ref swapOffset);
            reader.Read("swapBouncing", ref swapBouncing);
            reader.Read("matchBouncing", ref matchBouncing);
            reader.Read("selectBouncing", ref selectBouncing);
            combinations = reader.ReadCollection<Combination>("combinations").ToList();
            mixes = reader.ReadCollection<ChipMixRecipe>("mixes").ToList();
        }

        #endregion
    }
}