using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using YMatchThree.Core;
using YMatchThree.Meta;
using YMatchThree.Seasons;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.UI;
using Yurowm.Utilities;
using Behaviour = Yurowm.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree {
    public class InitialPoint {
        [OnLaunch(1000)]
        static IEnumerator Logic() {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            #if UNITY_IOS
            if (OnceAccess.GetAccess("InitialPoint")) {
                Social.localUser.Authenticate(result => Debug.Log($"Social Auth: {result}"));
            }
            #endif

            yield return null;
            
            LevelsLoading().Run();

            var copyright = Behaviour.GetByID<LabelFormat>("Copyright");
            if (copyright) {
                copyright["appname"] = Application.productName;
                copyright["company"] = Application.companyName;
                copyright["version"] = Application.version;
            }
            
            DebugPanel.Log("Open Test Levels Map", "Levels", 
                () => LevelMapShowButton.Show("TestLevels"));
        }
        
        public static bool IsLevelsLoaded { get; private set; } = false;
        
        static IEnumerator  LevelsLoading() {
            yield return null;
            
            IsLevelsLoaded = false;

            using (var timer = new ExecutionTimer("Loading")) {
                
                string previewRaw = null;
                
                yield return TextData.LoadTextProcess(Path.Combine(nameof(LevelScriptOrdered), $"LevelsPreview{Serializator.FileExtension}"),
                    r => previewRaw = r);
                
                var levels = new List<LevelScriptOrdered>();

                if (!previewRaw.IsNullOrEmpty()) {
                    var previews = new LevelScriptOrderedPreviews();
                    Serializator.FromTextData(previews, previewRaw);
                    levels.AddRange(previews.scripts);
                } else {
                    #if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
                    timer.Flash("Resources");
                    
                    string assetsPath = Path.Combine(Application.streamingAssetsPath, nameof(LevelScriptOrdered));
                    string[] levelsRaw = new DirectoryInfo(assetsPath).GetFiles()
                        .Where(f => f.Extension == Serializator.FileExtension && f.Name.StartsWith("level_"))
                        .Select(f => File.ReadAllText(f.FullName)).ToArray();
                    
                    DelayedAccess access = new DelayedAccess(1f / 10);
                    foreach (string raw in levelsRaw) {
                        LevelScriptOrdered script;
                        try {
                            script = LevelScriptBase.Load<LevelScriptOrdered>(raw, true);
                            if (script == null) continue;
                        } catch (Exception e) {
                            Debug.LogException(e);
                            continue;
                        }

                        levels.Add(script);
                        if (access.GetAccess()) yield return null;
                    }
                    #endif
                }
                
                LevelWorld[] worlds = levels
                    .GroupBy(l => l.worldName)
                    .Select(g => new LevelWorld(g.Key) {levels = g.ToList()})
                    .ToArray();
                
                
                LevelWorld.all.Clear();
                LevelWorld.all.AddRange(worlds);
                
                DebugPanel.Log("Unlock All Levels", "Levels", () => {
                    var progress = PlayerData.levelProgress;
                    LevelWorld.all
                        .ForEach(w => { 
                            var worldPropgress = progress.GetWorldPropgress(w.name, true);
                            w.levels.ForEach(l => {
                                var score = worldPropgress.GetBestScore(l.ID);
                                if (score == 0) {
                                    worldPropgress.SetBestScore(l.ID, 1);
                                    progress.SetDirty();
                                }
                            });
                        });
                    
                    PlayerData.SetDirty();
                    UIRefresh.Invoke();
                });
            }
            
            IsLevelsLoaded = true;
        }

    }
    
    public class Launcher : OnLaunchModifier {
        public override void BeforeSceneLoaded() { }

        public override IEnumerator PreLoad() {
            yield return GameSettings.Load("ehO2MCO0t9ZH4J5Z5Fj3");
        }

        public override IEnumerator PostLoad() {
            yield return Page.WaitAnimation();
            
            bool fastStart = false;
            LevelScriptBase sctiptOnLauch = null;
            
            #if UNITY_EDITOR
            
            sctiptOnLauch = GetTestScript();
            
            #endif
            
            if (sctiptOnLauch != null) 
                PuzzleSpace.Start(sctiptOnLauch);
            else
                Page.GetDefault().Show();
            
            isLaunched = true;
            
            UIRefresh.Invoke();
        }
        
        
        const string testScriptIDKey = "TestScript_ID";
        const string testScriptTypeKey = "TestScript_Type";
        
        public static void TestScript(LevelScriptBase script) {
            #if UNITY_EDITOR
            if (script == null) return;
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
            
            PlayerPrefs.SetString(testScriptIDKey, script.ID);
            PlayerPrefs.SetString(testScriptTypeKey, script.GetType().Name);
            
            UnityEditor.EditorApplication.isPlaying = true;
            #endif
        }
        
        public static LevelScriptBase GetTestScript() {
            
            if (!PlayerPrefs.HasKey(testScriptIDKey) || !PlayerPrefs.HasKey(testScriptTypeKey))
                return null;
                            
            var id = PlayerPrefs.GetString(testScriptIDKey);
            var type = PlayerPrefs.GetString(testScriptTypeKey);
            
            PlayerPrefs.DeleteKey(testScriptIDKey);
            PlayerPrefs.DeleteKey(testScriptTypeKey);
            
            var raw = TextData.LoadText(Path.Combine(type, LevelScriptBase.fileNameFormat.FormatText(id)));
            
            if (raw.IsNullOrEmpty())
                return null;
            
            var level = LevelScriptBase.Load<LevelScriptBase>(raw, false);
            
            var locationBody = AssetManager.GetPrefab<LevelMapLocationBody>();
            
            if (locationBody)
                level.background = new LevelBackground.Info(locationBody.name,
                    (locationBody.bottom + locationBody.top) / 2);

            return level;
        }
        
        
        
        static bool isLaunched = false;
        
        [ReferenceValue("IsLaunched")]
        public static int IsLaunched() {
            return isLaunched ? 1 : 0;
        }
    }
}
