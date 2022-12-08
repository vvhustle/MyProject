using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Dashboard;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Utilities;
using Yurowm.Extensions;
using Yurowm.Help;
using Yurowm.HierarchyLists;
using Yurowm.Icons;
using Yurowm.Nodes;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    [DashboardGroup("Content")]
    [DashboardTab("Levels", "Script", "tab.levels", 10)]
    public class LevelEditorController : DashboardEditor {

        public static LevelEditorController instance;
        
        LevelEditorContext context;
        List<LevelEditorBase> editors = LevelEditorBase.allTypes.Select(Activator.CreateInstance)
            .Cast<LevelEditorBase>().ToList();

        static string lastSelectedEditor {
            get => EditorStorage.Instance.GetText("leveleditor.lasteditor");
            set => EditorStorage.Instance.SetText("leveleditor.lasteditor", value);
        }

        GUIHelper.LayoutSplitter splitter;
        
        public override bool Initialize() {
            splitter = new GUIHelper.LayoutSplitter(OrientationLine.Horizontal, OrientationLine.Vertical, 200);
            
            context = new LevelEditorContext();
            context.controller = this;
            if (!lastSelectedEditor.IsNullOrEmpty())
                context.editor = editors.FirstOrDefault(e => e.GetName() == lastSelectedEditor);
            if (context.editor == null) context.editor = editors.FirstOrDefault(e => e is IDefault);
            if (context.editor == null) context.editor = editors.FirstOrDefault();
            context.editor?.SetContext(context);
            
            instance = this;
            
            Loading();
            
            return true;
        }
        
        public static readonly Type[] colorSettingsTypes = Utils
            .FindInheritorTypes<LevelColorSettings>(true, false)
            .Where(t => !t.IsAbstract)
            .ToArray();

        #region Worlds

        public List<LevelDesignFileWorld> worlds = new List<LevelDesignFileWorld>();
        
        const string defaultWorldName = "Default";
        
        void Loading() {
            EditorUtility.DisplayProgressBar("Loading", "", 0f);
            
            var worldsDict = new Dictionary<string, LevelDesignFileWorld>();

            var files = LevelEditorContext.directory.GetFiles()
                .Where(f => f.Extension == Serializator.FileExtension && f.Name.StartsWith("level_")).ToArray();

            var progressBarUpdateDelay = new DelayedAccess(1f / 10);
            
            for (int i = 0; i < files.Length; i++) {
                if (progressBarUpdateDelay.GetAccess())
                    EditorUtility.DisplayProgressBar("Loading", $"Files {i}/{files.Length}", 1f * i / files.Length);
                
                var file = files[i];

                try {
                    var designFile = new LevelScriptOrderedFile(file);

                    if (!worldsDict.ContainsKey(designFile.World))
                        worldsDict[designFile.World] = new LevelDesignFileWorld(designFile.World);

                    worldsDict[designFile.World].files.Add(designFile);
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }

            EditorUtility.DisplayProgressBar("Loading", "Building Worlds...", .3f);
            
            if (worldsDict.Count == 0)
                worldsDict.Add(defaultWorldName, new LevelDesignFileWorld(defaultWorldName));

            worldsDict.Values.ForEach(w => w.files = w.files.OrderBy(f => f.Order).ToList());

            if (!EditorApplication.isCompiling) {

                var level = worlds.SelectMany(w => w.files)
                    .FirstOrDefault(l => l.ScriptPreview.ID == lastLevelID);
                
                if (level != null && !level.World.IsNullOrEmpty())
                    worldsDict.TryGetValue(level.World, out context.currentWorld);

                if (context.currentWorld == null)
                    context.currentWorld = worldsDict.ContainsKey(defaultWorldName) ? 
                        worldsDict[defaultWorldName] : worldsDict.Values.First();

                worlds = worldsDict.Values.ToList();
                
                levelList = new LevelList(context.currentWorld.files, 
                    context.folders.Load(context.currentWorld.name).Select(p => new TreeFolder {fullPath = p}).ToList(),
                    context);
                levelList.onSelectedItemChanged += x => {
                    if (x.Count == 1 && x[0] != context.designFile)
                        SelectLevel(x[0]);
                };
                
                SelectLevel(lastLevelID);
            }
            
            EditorUtility.ClearProgressBar();
        }
        
        #endregion
        
        #region GUI

        public override void OnGUI() {
            if (context.design == null) {
                SelectLevel(0);
                LevelList.visible = true;
            }
            
            using (GUIHelper.Horizontal.Start()) {
                using (splitter.Start(LevelList.visible, context.editor != null)) {
                    if (splitter.Area()) {
                        levelList.OnGUI();
                        EditorTips.PopLastRectByID("lle.scripts");
                    }

                    if (splitter.Area())
                        context.editor.OnGUI();
                }
            }
            
            if (context.designFile != null && context.designFile.dirty) {
                context.designFile.Save();
                BuildPreviewFile();
            }
        }

        #region Preview File

        LevelScriptOrderedPreviews previews = new LevelScriptOrderedPreviews();

        public void BuildPreviewFile() {
            previews.scripts = worlds
                .SelectMany(w => w.files)
                .Select(f => f.ScriptPreview)
                .ToArray();
            
            TextData.SaveText(Path.Combine(nameof(LevelScriptOrdered), $"LevelsPreview{Serializator.FileExtension}") , Serializator.ToTextData(previews)); 
        }
        
        #endregion
        
        protected static Texture2D navFirstIcon;
        protected static Texture2D navPreviousIcon;
        protected static Texture2D navNextIcon;
        protected static Texture2D navLastIcon;

        protected static Texture2D playIcon;
        
        public override void OnToolbarGUI() {
            #region Icons

            {
                if (navFirstIcon == null)
                    navFirstIcon = EditorIcons.GetUnityIcon("Animation.FirstKey@2x", "d_Animation.FirstKey@2x");
                
                if (navPreviousIcon == null)
                    navPreviousIcon = EditorIcons.GetUnityIcon("Animation.PrevKey@2x", "d_Animation.PrevKey@2x");
                
                if (navNextIcon == null)
                    navNextIcon = EditorIcons.GetUnityIcon("Animation.NextKey@2x", "d_Animation.NextKey@2x");
                
                if (navLastIcon == null)
                    navLastIcon = EditorIcons.GetUnityIcon("Animation.LastKey@2x", "d_Animation.LastKey@2x");
                
                if (playIcon == null)
                    playIcon = EditorIcons.GetUnityIcon("PlayButton@2x", "d_PlayButton@2x");
            }

            #endregion
            
            #region Navigation Panel
            
            if (GUILayout.Button(navFirstIcon, EditorStyles.toolbarButton, GUILayout.Width(30)))
                SelectLevel(0);
            EditorTips.PopLastRectByID("lle.nav.first");
            
            if (GUILayout.Button(navPreviousIcon, EditorStyles.toolbarButton, GUILayout.Width(30)))
                SelectLevel(levelList.itemCollection.IndexOf(context.designFile) - 1);
            EditorTips.PopLastRectByID("lle.nav.previous");

            LevelList.visible = GUILayout.Toggle(LevelList.visible, "List",
                EditorStyles.toolbarButton, GUILayout.Width(40));
            EditorTips.PopLastRectByID("lle.nav.list");

            if (GUILayout.Button(navNextIcon, EditorStyles.toolbarButton, GUILayout.Width(30)))
                SelectLevel(levelList.itemCollection.IndexOf(context.designFile) + 1);
            EditorTips.PopLastRectByID("lle.nav.next");
            
            if (GUILayout.Button(navLastIcon, EditorStyles.toolbarButton, GUILayout.Width(30)))
                SelectLevel(levelList.itemCollection.Count - 1);
            EditorTips.PopLastRectByID("lle.nav.last");
            
            #endregion
            
            #region World Selection

            {
                if (GUILayout.Button(new GUIContent(context.currentWorld?.name), EditorStyles.toolbarDropDown, GUILayout.Width(100))) {
                    var menu = new GenericMenu();
                    
                    menu.AddItem(new GUIContent("New..."), false, () => {                       
                        if (context.controller.window is YDashboard dashboard){
                            var popup = DashboardPopup.Create<NewWorldPopup>();
                            popup.context = context;
                            dashboard.ShowPopup(popup);
                        }});
                    
                    var current = context.currentWorld;
                    
                    if (current != null) {
                        menu.AddItem(new GUIContent("Edit/Rename..."), false, () => {
                            if (context.controller.window is YDashboard dashboard){
                                var popup = DashboardPopup.Create<RenameWorldPopup>();
                                popup.context = context;
                                popup.worldFile = current;
                                dashboard.ShowPopup(popup);
                            }
                        });
                        
                        menu.AddItem(new GUIContent("Edit/Delete..."), false, () => {
                            if (!current.files.IsEmpty())
                                if (EditorUtility.DisplayDialog("Warning",
                                    $"Are you sure want to remove \"{current.name}\" world with all included levels?",
                                    "Yes (remove levels)", "No")) {
                                    current.files.ForEach(f => f.file.Delete());
                                }
                            worlds.Remove(current);
                            SelectWorld(worlds.FirstOrDefault());
                        });
                    }
                    
                    menu.AddSeparator("");

                    foreach (var world in worlds) {
                        var _w = world;
                        menu.AddItem(new GUIContent(world.name), world == current, () => SelectWorld(_w));
                    }

                    if (menu.GetItemCount() > 0)
                        menu.ShowAsContext();
                    
                }    
                
                EditorTips.PopLastRectByID("lle.worlds");
            }
            
            #endregion
            
            #region Editor Selection
            
            using (GUIHelper.BackgroundColor.Start(Color.Lerp(Color.white, Color.yellow, 0.6f)))
                GUIHelper.Popup(null, context.editor, editors, EditorStyles.toolbarPopup, 
                    e => e.GetName(), e => {
                        context.editor = e;
                        e.SetContext(context);
                        e.OnSelectAnotherLevel(context.designFile);
                    }, e => e != null, GUILayout.Width(100));                
            
            EditorTips.PopLastRectByID("lle.editors");
            
            #endregion
            
            #region Action Buttons
            
            using (GUIHelper.Lock.Start(EditorApplication.isPlayingOrWillChangePlaymode))
            using (GUIHelper.BackgroundColor.Start(Color.Lerp(Color.white, Color.green, 0.6f)))
                if (GUILayout.Button(playIcon, EditorStyles.toolbarButton, GUILayout.Width(30))) 
                    Launcher.TestScript(context.design);
            EditorTips.PopLastRectByID("lle.run");
            
            context.editor.OnActionToolbarGUI();
            
            #endregion
        }
        
        #endregion

        #region Selection
        
        public LevelList levelList;
        
        public static string lastLevelID {
            get => EditorStorage.Instance.GetText("leveleditor.lastLevelID");
            set => EditorStorage.Instance.SetText("leveleditor.lastLevelID", value);
        }
        
        public void SelectWorld(LevelDesignFileWorld world) {
            if (world != null && !worlds.Contains(world)) return;
            
            if (context.currentWorld == world) return;
            
            context.currentWorld = world;
            
            levelList.itemCollection = context.currentWorld?.files ?? new List<LevelScriptOrderedFile>();
            levelList.folderCollection = context.folders.Load(world.name)
                .Select(p => new TreeFolder() {fullPath = p})
                .ToList();
            
            levelList.Reload();
            if (levelList.itemCollection.Count > 0) {
                if (world.lastSelectedLevelID.IsNullOrEmpty())
                    SelectLevel(world.files.FirstOrDefault());
                else
                    SelectLevel(world.lastSelectedLevelID);
            } else
                SelectLevel((LevelScriptOrderedFile) null);
        }
        
        public void SelectLevel(LevelScriptOrderedFile scriptFile) {
            if (scriptFile?.Script != null) {
                context.SetDesign(scriptFile);
                context.currentWorld.lastSelectedLevelID = scriptFile.Script.ID;
                
                levelList.Select(x => Equals(x, scriptFile));
            } else
                levelList.Select(null);

            context.editor?.SetContext(context);
            context.editor?.OnSelectAnotherLevel(scriptFile);

            scriptFile?.SetDirty();
            
            lastLevelID = scriptFile?.ScriptPreview.ID;
        }

        public void SelectLevel(string ID) {
            if (ID.IsNullOrEmpty()) return;
            
            var level = worlds.SelectMany(w => w.files)
                .FirstOrDefault(l => l.ScriptPreview.ID == ID);
            
            if (level == null) level = levelList.itemCollection.FirstOrDefault();
            
            if (level != null) {
                SelectWorld(worlds.FirstOrDefault(w => w.name == level.World));
                
                SelectLevel(level);
            }
        }
        
        public void SelectLevel(int number) {
            if (number < 0 || number >= levelList.itemCollection.Count) return;
            
            var level = levelList.itemCollection[number];
            
            if (level == null) level = levelList.itemCollection.FirstOrDefault();
            
            SelectLevel(level);
        }

        #endregion
    }
}