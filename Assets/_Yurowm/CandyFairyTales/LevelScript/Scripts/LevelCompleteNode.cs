using System;
using System.Collections;
using System.Linq;
using TMPro;
using YMatchThree.Meta;
using Yurowm;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.UI;
using Yurowm.Utilities;
using Page = Yurowm.UI.Page;

namespace YMatchThree.Core {
    public class LevelCompleteNode : LevelFinalNode {
        
        public string bonusID = "";
        public enum CompleteType {
            ReachGoals = 0,
            NoMoves = 1,
            Force = 2
        }
        public CompleteType completeType = CompleteType.ReachGoals;

        public override void OnCreate() {
            base.OnCreate();
            bonusID = LevelContent.storage.GetDefault<CompleteBonus>()?.ID ?? string.Empty;
        }

        public override void OnLauch() {
            base.OnLauch();
            Behaviour.GetByID<Button>("Field.FastForward")?.gameObject.SetActive(false);
        }

        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            if (!field) yield break;
            
            var gameplay = field.fieldContext.Get<LevelGameplay>();
            
            var time = gameplay.time;

            switch (completeType) {
                case CompleteType.ReachGoals:
                    while (!gameplay.IsLevelComplete())
                        yield return null;
                    break;
                case CompleteType.NoMoves: {
                    var events = field.fieldContext.GetArgument<LevelEvents>();
                    
                    bool wait = true;
                    void OnShuffleIsNotPossible() => wait = false;
                    
                    events.onShuffleIsNotPossible += OnShuffleIsNotPossible;
                    
                    while (wait)
                        yield return null;
                    
                    events.onShuffleIsNotPossible -= OnShuffleIsNotPossible;
                    
                    break;
                }
                case CompleteType.Force: break;
            }
            
            var bonus = LevelContent.storage
                .Items<CompleteBonus>()
                .FirstOrDefault(b => b.ID == bonusID)?.Clone();
            
            field.complete = true; 
            var fastForward = false;
            
            if (bonus) {

                #region GoalsAreReached
                {
                    var page = Page.Get("GoalsAreReached");
                    if (page != null) {
                        yield return Page.WaitAnimation();
                        yield return page.ShowAndWait();
                        Page.Back();
                    }
                }
                #endregion

                SetFastForwardButton(() => {
                    SetFastForwardButton(null);
                    
                    field.Hiding()
                        .ContinueWith(() => { 
                            field.time.Scale = 15;
                            if (field.IsAlive()) {
                                void OnStartTask(LevelGameplay.Task t) {
                                    field.SetSimulation(new FastForwardSimulation());
                                    fastForward = true;
                                    field.events.onStartTask -= OnStartTask;
                                };
                                    
                                field.events.onStartTask += OnStartTask;
                            }
                        })
                        .Run(field.space.coroutine);
                });
                
                var hasLimitation = field.context.Contains<LevelLimitation>();

                if (hasLimitation) { 
                    field.AddContent(bonus);
                    
                    if (!bonus.IsComplete()) 
                        yield return field.Feedback(bonus.feedback);
                    
                    var skipFrames = new DelayedAccess(.3f);
                    
                    while (!bonus.IsComplete()) {
                        while (!(gameplay.GetCurrentTask() is WaitTask)) {
                            if (fastForward && !skipFrames.GetAccess())
                                field.ForceUpdate();
                            else
                                yield return null;
                        }

                        using (var task = gameplay.NewExternalTask()) {
                            yield return task.WaitAccess();
                            yield return bonus.Logic();
                        }
                        
                        yield return null;
                    }
                }
                
                SetFastForwardButton(null);
            }

            var score = context.Get<Score>();
            var script = context.GetArgument<LevelScriptBase>();
            
            if (!fastForward)
                yield return field.Hiding();
            
            field.Kill();

            if (script is LevelScriptOrdered ordered) {
                
                var worldPropgress = PlayerData.levelProgress.GetWorldPropgress(ordered.worldName, true);
                var bestScore = worldPropgress.GetBestScore(script.ID);    
                if (bestScore < score.value) {
                    worldPropgress.SetBestScore(script.ID, score.value);
                    PlayerData.SetDirty();
                }
                
                Behaviour.GetAllByID<LabelFormat>("WorldName")
                    .ForEach(l => l["value"] = ordered.worldName);
                Behaviour.GetAllByID<LabelFormat>("LevelNumber")
                    .ForEach(l => l["value"] = ordered.order.ToString());
            }
            
            Behaviour.GetAllByID<LabelFormat>("Score")
                .ForEach(l => l["value"] = score.value.ToString());
            
            Behaviour.GetAll<LevelCompleteStar>()
                .ForEach(s => s.No());
            
            yield return Page.Get("LevelComplete").ShowAndWait();
            
            foreach (var g in Behaviour.GetAll<LevelCompleteStar>()
                .Where(s => score.StarIsReached(s.number))
                .OrderBy(s => s.number)
                .GroupBy(s => s.number)) {
                
                g.ForEach(s => s.Award());
                
                yield return new WaitTimeSpan(.25f);
            }
        }

        void SetFastForwardButton(Action action) {
            bool visible = action != null;
            
            var button = Behaviour.GetByID<Button>("Field.FastForward");
            
            if (button == null)
                return;
            
            button.onClick.SetSingleListner(action);
            
            if (button.SetupComponent(out ContentAnimator animator)) {
                if (visible != button.gameObject.activeSelf) {
                    if (visible) {
                        button.gameObject.SetActive(true);
                        animator.Play("Show") ;
                    } else
                        animator.PlayAndWait("Hide")
                            .ContinueWith(() => button.gameObject.SetActive(false))
                            .Run();
                }
            } else 
                button.gameObject.SetActive(visible);
        }
        
        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("bonus", bonusID);
            writer.Write("completeType", completeType);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("bonus", ref bonusID);
            reader.Read("completeType", ref completeType);
        }

        #endregion
    }
}