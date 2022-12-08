using System.Collections;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.UI;

namespace YMatchThree.Core {
    public class WaitTask : LevelGameplay.InternalTask {
        
        LevelEvents events;
        
        bool waitMove;
        bool hintTriggered;

        public override void OnCreate() {
            base.OnCreate();
            events = context.GetArgument<LevelEvents>();
            events.onStartTask += OnStartTask;
        }

        void OnStartTask(LevelGameplay.Task task) {
            if (!hintTriggered || task == this) 
                return;
            
            hintTriggered = false;
            gameplay.HideHint();
        }
        
        public override IEnumerator Logic() {
            
            yield return Page.WaitAnimation();
            
            if (!gameplay.IsLevelComplete()) {
                #region Goal Checking
                
                if (gameplay.AreGoalsComplete()) {
                    events.onAllGoalsAreReached.Invoke();
                    gameplay.NextTask<LevelCompleteTask>();
                    yield break;
                }
                
                if (gameplay.IsGoalFailed() || !gameplay.AllowToMove()) {
                    gameplay.SetFailReason(FailReason.Context);
                    gameplay.NextTask<LevelFailedTask>();
                    yield break;
                }

                #endregion
                
                yield return gameplay.DoExternalTasks();
                
                if (gameplay.GetNextTask() != null)
                    yield break;
                
                #region Shuffle

                if (!gameplay.IsThereAnyPotentialTurns()) {
                    if (!gameplay.Shuffle()) {
                        gameplay.SetFailReason(FailReason.Shuffle);
                        events.onShuffleIsNotPossible.Invoke();
                        gameplay.NextTask<LevelFailedTask>();
                    } else {
                        yield return field.Feedback(gameplay.shuffleText);
                        gameplay.NextTask<GravityTask>();
                    }
                    
                    yield break;
                }

                #endregion
                
                waitMove = true;
            
                events.onStartWaitingNextMove.Invoke();

                Reactions.Get(field).Fill(Reaction.Type.Move);

                if (gameplay.hintDelay > 0)
                    ShowHintBase().Run(field.coroutine);
                
                void OnMoveSuccess() {
                    waitMove = false;
                }
                
                events.onMoveSuccess += OnMoveSuccess;
                
                gameplay.SetState(LevelGameplay.State.WaitTurn, true);

                while (waitMove && gameplay.GetNextTask() == null) {
                    
                    var externalTasks = gameplay.DoExternalTasks();
                    if (externalTasks != null) {
                        gameplay.SetState(LevelGameplay.State.WaitTurn, false);
                        
                        yield return externalTasks;
                        
                        gameplay.SetState(LevelGameplay.State.WaitTurn, true);
                    } else
                        yield return null;
                }
                
                gameplay.SetState(LevelGameplay.State.WaitTurn, false);
                
                events.onMoveSuccess -= OnMoveSuccess;
            } else {
                while (gameplay.GetNextTask() == null)
                    yield return gameplay.DoExternalTasks();
            }

            if (gameplay.GetNextTask() == null) 
                gameplay.NewTask<MatchingTask>();
        }

        IEnumerator ShowHintBase() {
            var hintDelay = gameplay.hintDelay;
            
            if (hintDelay <= 0) yield break;
            
            var time = gameplay.time;
            
            var showTime = time.AbsoluteTime + hintDelay;

            while (showTime > time.AbsoluteTime) {
                if (!waitMove) yield break;
                yield return null;
            }
            
            if (!gameplay.hintLockers.IsEmpty())
                yield break;
            
            hintTriggered = true;
            
            gameplay.ShowHint();
        }
    }
}