using Yurowm;
using Yurowm.Serialization;
using Yurowm.UI;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    [SerializeShort]
    public class StarGoal : LevelGoalWithIcon, ISlotRatingProvider  {
        
        Score score;
        public int starCount = 3;
        LevelScriptEvents scriptEvents;

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            space.context.SetupItem(out score);
            scriptEvents = space.context.GetArgument<LevelScriptEvents>();
            scriptEvents.onReachedTheStar += OnReachedTheStar;
        }
        
        int stars = 0;
        
        void OnReachedTheStar() {
            stars++;
            if (stars >= starCount)
                scriptEvents.onReachedTheStar -= OnReachedTheStar;
            UIRefresh.Invoke();
        }

        public override string GetCounterValue() {
            return (starCount - stars)
                .ClampMin(0)
                .ToString();
        }

        public override bool IsFailed() {
            return false;
        }

        public override bool IsComplete() {
            return score.StarIsReached(starCount); 
        }
        
        public int RateSlot(Slot slot) {
            if (IsComplete()) return 0;
            
            var content = slot.GetCurrentContent();
            
            if (!content) return 0;
            
            if (content is IDestroyable destroyable)
                return destroyable.scoreReward;

            return 0;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("count", starCount);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("count", ref starCount);
        }
    }
}