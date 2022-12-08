using System.Collections;
using Yurowm;
using Yurowm.Serialization;
using Yurowm.UI;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class CombinationsGoal : LevelGoalWithIcon {
        
        public int count = 12;
        
        LevelEvents events;
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            events = context.GetArgument<LevelEvents>();
            events.onMatchSoultion += OnMatchSoultion;
        }

        void OnMatchSoultion(MatchThreeGameplay.Solution solution) {
            count--;
            if (IsComplete())
                events.onMatchSoultion -= OnMatchSoultion;
            UIRefresh.Invoke();
        }


        public override bool IsFailed() {
            return false;
        }

        public override bool IsComplete() {
            return count <= 0;
        }

        public override string GetCounterValue() {
            return count.ClampMin(0).ToString();
        }

        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(CollectionSizeVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case CollectionSizeVariable size: {
                    count = size.count; 
                    return;
                }
            }
        }
    }
}