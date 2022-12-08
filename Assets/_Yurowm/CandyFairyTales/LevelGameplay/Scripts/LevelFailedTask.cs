using System.Collections;
using Yurowm.ContentManager;
using Yurowm.Features;
using Yurowm.Integrations;
using Yurowm.Localizations;

namespace YMatchThree.Core {
    public class LevelFailedTask : LevelGameplay.InternalTask {
        public override IEnumerator Logic() {
            
            if (gameplay.IsLevelComplete() || gameplay.IsLevelFailed()) 
                yield break;
            
            Integration.Get<LifeSystem>()?.BurnLife();
            
            gameplay.SetState(LevelGameplay.State.LevelFailed, true);

            events.onLevelFailed.Invoke();
            events.onLevelEnd.Invoke();
            
            gameplay.lcAnimator.PlaySound("Failed");
        }
    }
    
    public enum FailReason {
        Shuffle,
        Context
    }
    
    public interface IFailReason : ILiveContexted {
        string GetFailReason();
    }
}