using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YMatchThree.Meta;
using YMatchThree.Seasons;
using Yurowm;
using Yurowm.ComposedPages;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Features;
using Yurowm.Integrations;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.UI;
using Yurowm.Utilities;
using Page = Yurowm.UI.Page;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class PuzzleSpace : Space {
        
        public LevelScriptBase nextScript;
        
        public LevelScriptEvents scriptEvents;

        public override IEnumerator Complete() {
            Launch().Run(coroutine);
            yield break;
        }

        IEnumerator Launch() {
            Page.Get("Loading").Show();
            
            Behaviour.GetAllByID<LabelFormat>("Field.Limitation")
                .ForEach(c => c["value"] = string.Empty);
            
            while (!InitialPoint.IsLevelsLoaded)
                yield return null;   
            
            yield return Page.WaitAnimation();

            var script = nextScript;
            nextScript = null;
            
            AddItem(new FieldRectController());
            AddItem(new SpaceCamera());
            AddItem(new CameraOperator {
                allowToRotate = false,
                allowToZoom = false,
                allowToMove = false
            });
            
            if (script.IsPreview)
                yield return script.LoadCompletely();
            
            if (script is LevelScriptOrdered ordered)
                ordered.OnSelect();

            context.SetArgument<LevelScriptBase>(script);
            context.SetArgument(script);

            #region Getting Default Chip

            {
                var defaultChip = LevelContent.storage
                    .GetDefault<SimpleChip>()
                    .Clone();
                
                var defaultChipBody = script?.chipBody;
            
                if (!defaultChipBody.IsNullOrEmpty())
                    defaultChip.bodyName = defaultChipBody;
            
            }

            #endregion
            
            if (!script.background.name.IsNullOrEmpty()) {
                var bg = GameEntity.New<LevelBackground>();
                bg.info = script.background;
                AddItem(bg);
            }
            
            OnLaunch(script);
                
            void OnPlayerAttention() {
                LevelBooster.ShowLevelBoosters(this)
                    .ContinueWith(() => Page.Get("Field")?.Show())
                    .Run(coroutine);
                
                scriptEvents.onPlayerAttention -= OnPlayerAttention;
            }
            
            scriptEvents.onPlayerAttention += OnPlayerAttention;
            
            script?.Launch(context);
            
            DebugPanel.Log("Destroy Space", "Gameplay", Destroy);
        }

        void OnLaunch(LevelScriptBase script) {
            AddItem(new LevelScriptPipeline {
                stars = script.stars
            });
            
            scriptEvents = context.GetArgument<LevelScriptEvents>();
            
            foreach (var extensionInfo in script.extensions) {
                var content = extensionInfo.Reference.Clone();

                content.ApplyDesign(extensionInfo);
                    
                AddItem(content);
            }
            
            if (script is LevelScriptOrdered lso)
                LevelMap.storage.items
                    .FirstOrDefault(s => s.worldName == lso.worldName)?
                    .SetupField(this);
        }

        public static void TryToStart(LevelScriptBase script) {
            if (PlayerData.IsSilentMode()) return;
            
            var lifeSystem = Integration.Get<LifeSystem>();
            
            if (lifeSystem != null) {
                if (!lifeSystem.HasLife()) { 
                    ComposedPage.ByID("Popup").Show(new NoLifePopup());
                    return;
                }
                
                lifeSystem.LockLife();
            } 
                
            Start(script);
        }
        
        public static void Start(LevelScriptBase script) {
            // find and destroy all PuzzleSpaces
            // Puzzle space is a kind of scene, but not Unity scene
            all.CastIfPossible<PuzzleSpace>()
                .ToArray()
                .ForEach(s => s.Destroy());
            
            // Here we create an event, so when new PuzzleSpace will be created,
            // we will attach the level script to the space
            BlindCatchSpace<PuzzleSpace>(s => {
                s.nextScript = script;
            });
            
            // And here we ask to create and show new PuzzleSpace, so it
            // will launch the script and all gameplay mechanics
            Show<PuzzleSpace>();
        }
        
        public static void Restart() {
            Restarting().Run();
        }
        
        static IEnumerator Restarting() {
            bool pass = false;
            
            yield return LoseLifeWarrning(r => pass = r);
            
            if (!pass)
                yield break;
            
            yield return Page.Get("Field").ShowAndWait();

            var space = all.CastOne<PuzzleSpace>();
            var script = space.context.GetArgument<LevelScriptBase>();
            
            space.Destroy();
            
            TryToStart(script);
        }
        
        public static void Close() {
            Closing().Run();
        }
        
        static IEnumerator Closing() {
            bool pass = false;
            
            yield return LoseLifeWarrning(r => pass = r);
            
            if (!pass)
                yield break;
            
            yield return Page.Get("Field").ShowAndWait();
            
            yield return Page.Get("Loading").ShowAndWait();
            
            var space = all.CastOne<PuzzleSpace>();
            
            space.Destroy();
            
            yield return null;
            
            Page.Get("Map").Show();
        }
        
        static IEnumerator LoseLifeWarrning(Action<bool> passCallback) {
            
            var lifeSystem = Integration.Get<LifeSystem>();
            
            if (lifeSystem != null) {
                if (lifeSystem.data.lifeLock) {
                    var loseLifePopup = new LoseLifePopup();
                    loseLifePopup.accept = lifeSystem.BurnLife;
                    
                    ComposedPage.ByID("Popup").Show(loseLifePopup);
                    
                    yield return loseLifePopup.Wait();

                    if (lifeSystem.data.lifeLock) {
                        passCallback?.Invoke(false);
                        yield break;
                    }
                }
            }
            
            passCallback?.Invoke(true);
        }
        
        
        public static void NextLevel() {
            NextLevelLogic().Run();
        }
        
        static IEnumerator NextLevelLogic() {
            var nextLevel = GetNextLevel();

            yield return Page.Get("Field").ShowAndWait();
            
            yield return Page.Get("Loading").ShowAndWait();
            
            all.CastOne<PuzzleSpace>()?.Destroy();

            yield return Page.Get("Map").ShowAndWait();
            
            if (nextLevel == null)
                yield break;
            
            yield return null;
            
            var mapSpace = all.CastOne<LevelMapSpace>();
            
            if (mapSpace != null) {
                mapSpace.levelMap.ShowCurrentLevel();
                
                mapSpace.context
                    .Get<LevelButton>(b => b.level == nextLevel)?
                    .OnClick(0);
            } else
                TryToStart(nextLevel);
        }
        
        public static LevelScriptOrdered GetNextLevel() {
            var space = all.CastOne<PuzzleSpace>();
            if (!space) 
                return null;
                
            if (!(space.context.GetArgument<LevelScriptBase>() is LevelScriptOrdered currentLevel))
                return null;
            
            return LevelWorld.all
                .FirstOrDefault(w => w.name == currentLevel.worldName)?.levels
                .FirstOrDefault(l => l.order == currentLevel.order + 1);
        }

        [ReferenceValue("HasNextLevel")]
        static int HasNextLevel() {
            return GetNextLevel() == null ? 0 : 1;
        }
    }
    
    public abstract class PuzzleSimulation {
        public abstract bool AllowSounds();
        public abstract bool AllowAnimations();
        public abstract bool AllowEffects();
        
        public abstract bool AllowToWait();
        
        public abstract bool AllowBodies();
    }
    
    public class GeneralPuzzleSimulation : PuzzleSimulation {
        public override bool AllowSounds() => true;

        public override bool AllowAnimations() => true;

        public override bool AllowEffects() => true;
        
        public override bool AllowToWait() => true;
    
        public override bool AllowBodies() => true;
    }
}