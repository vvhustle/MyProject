using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.IO;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.HierarchyLists;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class LevelList : HierarchyList<LevelScriptOrderedFile> {
        public static bool visible {
            get => EditorStorage.Instance.GetBool("leveleditor.list.visible", true);  
            set => EditorStorage.Instance.SetBool("leveleditor.list.visible", value);
        }
        
        LevelEditorContext context;
        
        List<IInfo> highlighted = new List<IInfo>();

        public LevelList(List<LevelScriptOrderedFile> collection, List<TreeFolder> folders, LevelEditorContext context) : base(collection, folders, new TreeViewState()) {
            this.context = context;
            
            onChanged += UpdateLevelsOrder;
            onChanged += SaveFolders;
            onRebuild += () => {
                if (context.design != null) {
                    IInfo info = root.Find(GetUniqueID(context.designFile));
                    highlighted.Clear();
                    if (info != null) {
                        while (info != root) {
                            highlighted.Add(info);
                            info = info.parent;
                        }
                    }
                }
            };
            onSelectionChanged += x => {
                if (x.Count == 1 && x[0].isItemKind) {
                    IInfo info = x[0];
                    highlighted.Clear();
                    while (info != root) {
                        highlighted.Add(info);
                        info = info.parent;
                    }
                }
            };
            onRemove += x => {
                foreach (IInfo i in x)
                    if (i.isItemKind) i.asItemKind.content.file.Delete();
                SaveFolders();
            };
        }

        void SaveFolders() {
            context.folders.Save(context.currentWorld.name, folderCollection.Select(x => x.fullPath));
        }

        public void Sync(List<LevelScriptOrderedFile> designs) {
            if (itemCollection == designs)
                return;
            itemCollection = designs;
            Reload();
            onChanged();
        }
        
        void UpdateLevelsOrder() {
            int cursor = 0;
            
            root.GetAllChild()
                .CastIfPossible<ItemInfo>()
                .Select(i => i.content)
                .ForEach(file => {
                    cursor++;
                    file.Order = cursor;
                });
        }

        LevelScriptOrderedFile newScriptFile;
        
        const string newFolderNameFormat = "Group{0}";
        public override void ContextMenu(GenericMenu menu, List<IInfo> selected) {

            if (selected.Count == 0) {
                foreach (var scriptPreset in context.scriptPresets) {
                    menu.AddItem(new GUIContent($"New Script/{scriptPreset.GetName()}"), false, () => {
                        var script = scriptPreset.Emit(context);
                        script.worldName = context.currentWorld.name;
                        
                        newScriptFile = new LevelScriptOrderedFile(script, LevelScriptEditor.NewID());
                        
                        AddNewItem(root, null); 
                    });
                }
                menu.AddItem(new GUIContent("New Folder"), false, () => AddNewFolder(root, newFolderNameFormat));
            } else {
                if (selected.Count == 1 && selected[0].isFolderKind) {
                    FolderInfo parent = selected[0].asFolderKind;

                    menu.AddItem(new GUIContent("Add New Entry"), false, () => AddNewItem(parent, null));
                    menu.AddItem(new GUIContent("Add New Folder"), false, () => AddNewFolder(parent, newFolderNameFormat));
                } else {
                    FolderInfo parent = selected[0].parent;
                    if (selected.All(x => x.parent == parent)) {
                        menu.AddItem(new GUIContent("Group"), false, () => Group(selected, parent, newFolderNameFormat));
                        if (selected.All(x => x.isItemKind))
                            menu.AddItem(new GUIContent("Duplicate"), false, () => Duplicate(selected.Select(x => x.asItemKind).ToList(), parent));
                    }
                    else
                        menu.AddItem(new GUIContent("Group"), false, () => Group(selected, root, newFolderNameFormat));

                }
                menu.AddItem(new GUIContent("Remove"), false, () => Remove(selected.ToArray()));
                
                if (context.controller.worlds.Count > 1) {
                    foreach (var world in context.controller.worlds) {
                        if (world.name == context.design.worldName) continue;
                        var worldName = world.name;
                        menu.AddItem(new GUIContent("Move To/" + worldName), false, () => MoveToWorld(selected, worldName));
                    }
                }

            }
        }
        
        void MoveToWorld(List<IInfo> items, string world)  {
            List<LevelScriptOrderedFile> levels = new List<LevelScriptOrderedFile>();
            foreach (var item in items) {
                if (item.isFolderKind)
                    levels.AddRange(item.asFolderKind.GetAllChild().Where(i => i.isItemKind).Select(i => i.asItemKind.content));
                else
                    levels.Add(item.asItemKind.content);
            }
            
            levels.Distinct().ForEach(l => l.World = world);
            context.controller.Initialize();
        }

        void Duplicate(List<ItemInfo> levels, FolderInfo folder) {
            var clones = levels.Select(x => x.content.Script.Clone()).ToList();

            string path = folder.fullPath;
            foreach (var script in clones) {
                newScriptFile = new LevelScriptOrderedFile(script, LevelScriptEditor.NewID());
                SetPath(newScriptFile, path);
                AddNewItem(root, null);
            }

            Reload();
            onChanged();
        }

        public override void DrawItem(Rect rect, ItemInfo info) {
            if (highlighted.Contains(info))
                Highlight(rect, true);
            rect = ItemIconDrawer.DrawSolid(rect, LevelEditorStyles.listLevelIcon);
            GUI.Label(rect, info.content.ToString(),
                LevelEditorStyles.richLabelStyle);
        }

        public override void DrawFolder(Rect rect, FolderInfo info) {
            if (highlighted.Contains(info))
                Highlight(rect, false);
            base.DrawFolder(rect, info);
        }

        public override int GetUniqueID(LevelScriptOrderedFile element) {
            return element.file.GetHashCode();
        }

        public override int GetUniqueID(TreeFolder element) {
            return element.GetHashCode();
        }

        public override bool ObjectToItem(UnityEngine.Object o, out IInfo result) {
            result = null;
            return false;
        }

        public override void SetPath(LevelScriptOrderedFile element, string path) {
            if (element.Script.path != path) {
                element.Script.path = path;
                element.Save();
            }
        }

        public override string GetPath(LevelScriptOrderedFile element) {
            return element.Script.path;
        }

        public override LevelScriptOrderedFile CreateItem() {
            if (newScriptFile == null)
                return null;
            if (!newScriptFile.file.Exists) newScriptFile.Save();

            return newScriptFile;
        }

        public override bool CanRename(ItemInfo info) {
            return false;
        }

        public override bool CanBeChild(IInfo parent, IInfo child) {
            return parent.isFolderKind;
        }

        public void Select(Func<LevelScriptOrderedFile, bool> func) {
            List<IInfo> infos = new List<IInfo>();
            if (func != null)
                infos.AddRange(root.GetAllChild().Where(x => x.isItemKind && func(x.asItemKind.content)));
            
            onSelectionChanged(infos);
            SetSelection(infos.Select(x => GetUniqueID(x.asItemKind.content)).ToList());
            onSelectedItemChanged(infos.Select(x => x.asItemKind.content).ToList());
            onSelectionChanged(infos);
        }

        Color hightlightFace = new Color(0, 1, 1, 0.05f);
        void Highlight(Rect rect, bool outline = false) {
            rect.x += 1;
            rect.width -= 2;
            Handles.DrawSolidRectangleWithOutline(rect, hightlightFace, outline ? Color.cyan : Color.clear);
        }
    }
}