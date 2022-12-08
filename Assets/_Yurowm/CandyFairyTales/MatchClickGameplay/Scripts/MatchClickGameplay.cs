using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Controls;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class MatchClickGameplay : LevelGameplay {
        
        public ExplosionParameters clickBouncing = new ExplosionParameters();
        public ExplosionParameters matchBouncing = new ExplosionParameters();

        public override ColorBake GetColorBake() {
            return new LazyColorBake();
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            events.onStartTask += OnStartTask;
        }

        public override void OnRemoveFromSpace(Space space) {
            events.onStartTask -= OnStartTask;
            base.OnRemoveFromSpace(space);
        }

        void OnStartTask(Task task) {
            if (task is WaitTask)
                ShowOverlay();
            else
                HideOverelay();
            
            if (!(task is MatchingTask))
                matchingSlot = null;
        }
        
        #region Overlay
        
        void ShowOverlay() {
            var moves = FindMoves()
                .Select(x => x.Select(y => y.GetCurrentContent()).ToArray()).ToArray();
            foreach (var move in moves) {
                
                var combo = combinations
                    .FirstOrDefault(c => c.IsSuitable(move));
                
                if (combo == null) continue;
                
                var overlay = AssetManager
                    .GetAsset<Sprite>(GetItem<BombChipBase>(combo.bomb)?.overlayID);
                
                if (overlay)
                    move.CastIfPossible<IContentWithOverlay>()
                        .ForEach(co => co.SetOverlay(overlay));
            }
        }

        void HideOverelay() {
            context
                .GetAll<IContentWithOverlay>()
                .ForEach(co => co.SetOverlay(null));
        } 

        #endregion
        
        #region Controls
        
        protected override void SetupControl(TouchControl control) {
            control.onTouch += OnTouch;
            control.onTouchEnd += OnTouchEnd;
        }

        protected override void ClearControl(TouchControl control) {
            control.onTouch -= OnTouch;
            control.onTouchEnd -= OnTouchEnd;
        }
        
        void OnTouch(TouchStory touch) {
            if (touch.IsOverUI)
                return;
                
            if (!gameplay.IsLevelComplete() && gameplay.IsWaitingForPlayerTurn()) {
                var slot = space.clickables
                    .Cast<Slot>(touch.currentWorldPosition, Slot.Offset);
                
                if (slot && matchingSlot != slot) {
                    matchingSlot = slot;
                    field.Explode(matchingSlot.position, clickBouncing);
                }
            }
        }
        
        void OnTouchEnd(TouchStory touch) {
            if (touch.IsOverUI) 
                return;
            
            if (!gameplay.IsLevelComplete() && gameplay.IsWaitingForPlayerTurn()) {
                var slot = space.clickables
                    .Cast<Slot>(touch.currentWorldPosition, Slot.Offset);
                
                if (slot && matchingSlot == slot) 
                    gameplay.NextTask<MatchingTask>();
            }
        }

        #endregion
        
        #region Moves

        public override bool IsThereAnyPotentialTurns() {
            foreach (var slot in slots.all.Values) {
                var color = slot.GetCurrentColor();
                if (color.IsMatchableColor())
                    foreach (Side side in Sides.straight)
                        if (slot[side] && color.IsMatchWith(slot[side].GetCurrentColor()))
                            return true;
            }

            return slots.all.Values.Any(s => s.GetCurrentContent() is BombChipBase);
        }

        public override bool IsThereAnySolutions() {
            return false;
        }
        
        List<List<Slot>> FindMoves() {
            var result = new List<List<Slot>>();
            
            var pool = slots.all.Values
                .Select(x => x.GetCurrentContent())
                .NotNull()
                .Where(c => c is IColored)
                .SelectMany(c => c.slotModule.Slots())
                .Distinct()
                .ToList();

            while (pool.Count > 0) {
                var first = pool.First();
                var match = Matcher(new List<Slot>(), first);
                pool.RemoveAll(s => match.Contains(s));
                if (match.Count > 1)
                    result.Add(match);
            }

            return result;
        }

        #endregion

        #region Matching

        Slot matchingSlot;
        
        public override IEnumerator Matching() {
            if (matchingSlot == null)
                yield break;
            

            #region Mixing
            
            if (matchingSlot.GetCurrentContent() is BombChipBase centerBomb) {
                var pairs = matchingSlot.nearSlot.Where(x => x.Value && x.Key.IsStraight() && x.Value.GetCurrentContent() is Chip)
                    .Select(x => x.Value.GetContent<Chip>())
                    .GroupBy(x => new Pair<string>(centerBomb.ID, x.ID))
                    .ToDictionary(x => x.Key, x => x.First());

                var mixInfo = mixes.FirstOrDefault(a => pairs.ContainsKey(a.pair));

                if (mixInfo != null) {
                    OnNextTurn();

                    var chipA = pairs[mixInfo.pair];
                    var chipB = centerBomb;

                    const float duration = .2f;
                    
                    var posA = chipA.slotModule.Position;
                    var posB = chipB.slotModule.Position;

                    for (var t = 0f; t < .2f; t += Time.deltaTime) {
                        chipA.position = Vector2.Lerp(posA, posB, (t / duration).Ease(EasingFunctions.Easing.InOutQuad));
                        yield return null;
                    }

                    var mix = storage.GetItem<ChipMix>(m => m.ID == mixInfo.mix).Clone();
                    mix.random = random.NewRandom();
                    mix.position = chipB.position;
                    mix.slot = chipB.slotModule.Slot();
                    mix.SetChips(chipB, chipA);

                    matchDate++;

                    HideHint();
                    
                    field.AddContent(mix);

                    while (mix.IsAlive())
                        yield return null;

                    yield break;
                }
            }

            #endregion
            
            var matchingSlots = Matcher(new List<Slot>(), matchingSlot);

            if (matchingSlots.Count < 2)
                yield break;

            var hitContext = new HitContext(context, matchingSlots, HitReason.Matching);
            
            matchDate++;
            
            var matchingDefaultContent = matchingSlots
                .Select(x => x.GetCurrentContent())
                .Where(c => c.IsDefault)
                .ToArray();
            
            // if matchingSlot is not default (bomb, for example),
            // we are trying to replace the slot to a nearest other   
            if (!matchingDefaultContent.IsEmpty())
                if (!matchingSlot.GetCurrentContent().IsDefault)
                    matchingSlot = matchingDefaultContent
                        .Where(c => c.slotModule is SingleSlotModule)
                        .GetMin(c => c.slotModule.Center.FourSideDistanceTo(matchingSlot.coordinate)).slotModule
                        .Slots()
                        .FirstOrDefault();

            
            field.Explode(matchingSlot.position, matchBouncing);
            
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 7) {
                foreach (var chip in matchingDefaultContent)
                    if (chip && chip.slotModule.HasSlot()) 
                        chip.position = Vector2.Lerp(chip.slotModule.Position, matchingSlot.position, t);
                yield return null;
            }
            
            int points = matchingSlots.Sum(x => x.Hit(hitContext));
            if (points > 0) {
                score.AddScore(points);
                matchingSlot.ShowScoreEffect(points, matchingSlot.GetCurrentColor());
            }
            
            if (!matchingDefaultContent.IsEmpty()) {
                var allContent = matchingSlots.Select(x => x.GetCurrentContent()).ToList();
                foreach (var combination in combinations) {
                    if (combination.IsSuitable(allContent)) {
                        var bomb = storage.Items<Chip>()
                            .FirstOrDefault(b => b.ID == combination.bomb)?.Clone();
                        
                        bomb.birthDate = matchDate;
                        bomb.position = matchingSlot.position;
                        
                        if (bomb is IColored)
                            bomb.SetupVariable(new ColoredVariable {
                                info = matchingDefaultContent[0].colorInfo
                            });
                        
                        field.AddContent(bomb);
                        matchingSlot.AddContent(bomb);
                        
                        break;
                    }
                }
            }

            foreach (var chip in matchingDefaultContent)
                if (chip) 
                    chip.position = matchingSlot.position;

            OnNextTurn();
        }
        
        List<Slot> Matcher(List<Slot> all, Slot center) {
            if (!(center.GetCurrentContent() is IColored colored))
                return all; 

            var colorInfo = colored.colorInfo;

            all.Add(center);

            foreach (var s in center.nearSlot) {
                if (s.Key.IsSlanted()) 
                    continue;
                if (s.Value == null || all.Contains(s.Value))
                    continue;
                if (s.Value.GetCurrentContent() is IColored c && c.colorInfo.IsMatchWith(colorInfo))
                    all = Matcher(all, s.Value);
            }
            
            return all;
        }

        #endregion

        #region Mixes

        public List<ChipMixRecipe> mixes = new List<ChipMixRecipe>();
        
        #endregion

        #region Combinations

        public List<Combination> combinations = new List<Combination>();

        public class Combination : ISerializable {
            public string bomb;
            
            public int count = 4;
            public int vertCount = 1;
            public int horizCount = 1;
            
            public bool IsSuitable(IEnumerable<SlotContent> content) {
                int xmin = int.MaxValue;
                int ymin = int.MaxValue;
                int xmax = int.MinValue;
                int ymax = int.MinValue;
                int contentCount = 0;
                
                content.ForEach(c => {
                    xmin = Mathf.Min(xmin, c.slotModule.Center.X);
                    ymin = Mathf.Min(ymin, c.slotModule.Center.Y);
                    xmax = Mathf.Max(xmax, c.slotModule.Center.X);
                    ymax = Mathf.Max(ymax, c.slotModule.Center.Y);
                    contentCount++;
                });

                int horiz = xmax - xmin;
                int vert = ymax - ymin;

                return count <= contentCount &&
                       vertCount <= vert && horizCount <= horiz &&
                       !(vertCount != horizCount && 
                         vertCount >= horizCount != vert >= horiz);
            }
            
            public void Serialize(Writer writer) {
                writer.Write("bomb", bomb);
                writer.Write("count", count);
                writer.Write("vertCount", vertCount);
                writer.Write("horizCount", horizCount);
            }

            public void Deserialize(Reader reader) {
                reader.Read("bomb", ref bomb);
                reader.Read("count", ref count);
                reader.Read("vertCount", ref vertCount);
                reader.Read("horizCount", ref horizCount);
            }
        }
        
        #endregion
        
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("clickBouncing", clickBouncing);
            writer.Write("matchBouncing", matchBouncing);
            writer.Write("combinations", combinations.ToArray());
            writer.Write("mixes", mixes.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("clickBouncing", ref clickBouncing);
            reader.Read("matchBouncing", ref matchBouncing);
            combinations = reader.ReadCollection<Combination>("combinations").ToList();
            mixes = reader.ReadCollection<ChipMixRecipe>("mixes").ToList();
        }

        #endregion
    }
}