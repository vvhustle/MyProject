using System.Collections;
using Yurowm.Features;
using Yurowm.Integrations;
using Yurowm.UI;

namespace YMatchThree.Core {
    public class LevelCompleteTask : LevelGameplay.InternalTask {
        public override IEnumerator Logic() {
            if (gameplay.IsLevelComplete()) 
                yield break;
            
            Integration.Get<LifeSystem>()?.UnlockLife();
            
            gameplay.SetState(LevelGameplay.State.LevelComplete, true);

            events.onLevelComplete.Invoke();
            events.onLevelEnd.Invoke();

            gameplay.NextTask<WaitTask>();
        }
    }
}