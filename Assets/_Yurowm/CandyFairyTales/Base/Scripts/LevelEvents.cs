using System;
using System.Collections.Generic;
using UnityEngine;
using YMatchThree.Core;

namespace YMatchThree {
    public class LevelEvents {
        public Action<HitContext> onHit = delegate {};
        public Action<SlotContent> onSlotContentDestroyed = delegate{};
        public Action onSomeContentDestroyed = delegate{};
        
        public Action<SlotContent, HitContext> onStartDestroying = delegate{};
        
        public Action<Chip> onGenerate = delegate{};
        
        
        public Action onLevelStart = delegate {};
        public Action onLevelComplete = delegate{};
        public Action onLevelFailed = delegate{};
        public Action onLevelEnd = delegate{};

        public Action onShuffleIsNotPossible = delegate{};
        
        public Action<LevelGameplay.Task> onStartTask = delegate {};
        public Action onMoveSuccess = delegate {};
        public Action onStartWaitingNextMove = delegate {};
        
        public Action onAllGoalsAreReached = delegate {};
        public Action<MatchThreeGameplay.Solution> onMatchSoultion = delegate {};
        
    }
    
    public class LevelScriptEvents {
        public Action<int> onAddScore = delegate {};
        public Action onReachedTheStar = delegate {};
        public Action onPlayerAttention = delegate {};
        
        public Action<Booster> onRedeemBooseter = delegate {};
    }
}