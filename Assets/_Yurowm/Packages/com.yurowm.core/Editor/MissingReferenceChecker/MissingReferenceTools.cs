using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yurowm.Dashboard;
using Yurowm.Extensions;
using Yurowm.HierarchyLists;
using Object = UnityEngine.Object;

namespace Yurowm.Utilities {
    [DashboardGroup("Development")]
    [DashboardTab("Missing References", "Hammer", "tab.missingreferences")]
    public class MissingReferenceTools : DashboardEditor {
        
        ErrorList list;
        
        List<Error> errors = new List<Error>();
        
        MissingReferenceFixer fixer = new MissingReferenceFixer();
        
        Mode mode = Mode.Scanner;
        enum Mode {
            Scanner,
            Fixer
        }
        
        public override bool Initialize() {
            list = new ErrorList(errors);
            
            return true;
        }
        
        public override void OnGUI() {
            switch (mode) {
                case Mode.Scanner: list.OnGUI(); break;
                case Mode.Fixer: fixer.OnGUI(); break;
            }
        }

        public override void OnToolbarGUI() {
            mode = (Mode) EditorGUILayout.EnumPopup(mode, EditorStyles.toolbarPopup, GUILayout.Width(100));
            if (mode == Mode.Scanner)
                if (GUILayout.Button("Scan", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    Scanning();
                    
            GUILayout.Label($"Errors: {errors.Count}", EditorStyles.toolbarButton);
        }

        const string MissingReferenceError = "{0}:{1}.{2} ({3}) is missed";
        
        void Scanning() {
            
            float progress = 0;
            
            void ProgressBar(string status) {
                progress += 0.1f;
                progress = progress.Repeat(1);
                EditorUtility.DisplayProgressBar("Scanning", status, progress);
            }
            
            errors.Clear();
            list.Reload();
            
            foreach (var path in AssetDatabase.GetAllAssetPaths()) {
                ProgressBar(path);
                var asset = AssetDatabase.LoadAssetAtPath<Transform>(path);
                if (asset) {
                    foreach (var child in asset.AndAllChild()) {
                        CheckGameObject(path, child.gameObject);
                    }
                    continue;
                }

                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so) 
                    CheckObject(so, path, so);
            }

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++) {
                var scene = SceneManager.GetSceneAt(sceneIndex);

                foreach (var root in scene.GetRootGameObjects()) {
                    foreach (var child in root.transform.AndAllChild()) {
                        ProgressBar($"@{scene.name}: {child.name}");
                        CheckGameObject("@" + scene.name, child.gameObject);
                    }
                }
            }
            
            EditorUtility.ClearProgressBar();
            
            list.Reload();
        }

        string GetPath(Transform transform) {
            if (transform == null) return "";
            return GetPath(transform.parent) + "/" + transform.name;
        }
        
        void CheckGameObject(string path, GameObject gameObject) {
            var _path = path + GetPath(gameObject.transform);
            CheckObject(gameObject, _path, gameObject, false);

            foreach (var component in gameObject.GetComponents<Component>().NotNull())
                CheckObject(gameObject, _path, component);
        }
        
        void CheckObject(Object reference, string path, Object obj, bool visibleOnly = true) {
            var so = new SerializedObject(obj);
            var sp = so.GetIterator();
                         
            while (Next(sp, visibleOnly)) {
                if (sp.propertyType == SerializedPropertyType.ObjectReference) {
                    if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0) {
                        
                        CheckMissedReference(reference, sp, path, obj);
                    }
                }
            }
        }
        
        bool Next(SerializedProperty property, bool visibleOnly) {
            return visibleOnly ? property.NextVisible(true) : property.Next(true);
        }
        
        void CheckMissedReference(Object reference, SerializedProperty property, string path, Object obj) {
            var error = new Error() {
                reference = reference,
                message = MissingReferenceError.FormatText(path, obj.GetType().Name, property.displayName, property.type),
            };

            errors.Add(error);
        }
        
        class Error {
            static int indexer = 0;
            int index;
            
            public int Index => index;
            public string message;
            public Object reference;
            public string report;

            public Error() {
                index = indexer++;
            }
            
            public Error(string message) : this() {
                this.message = message;
            }
            
            public void Draw(Rect rect) {
                Handles.DrawSolidRectangleWithOutline(rect, Color.white.Transparent(Index % 2 == 0 ? .1f : .2f), Color.gray);
                GUI.Label(rect, message);
            }
            
            public void ContextMenu(GenericMenu menu) {
                menu.AddItem(new GUIContent("Print Report"), false, () => {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine(report);
                    Debug.Log(builder.ToString());
                });
            }
        }
        
        class ErrorList : NonHierarchyList<Error> {
            public ErrorList(List<Error> collection) : base(collection) {}

            public override void DrawItem(Rect rect, ItemInfo info) {
                info.content.Draw(rect);
            }

            public override int GetUniqueID(Error element) {
                return element.Index;
            }

            public override Error CreateItem() {
                return null;
            }

            protected override float GetCustomRowHeight(int row, TreeViewItem item) {
                return 40;
            }

            public override void ContextMenu(GenericMenu menu, List<IInfo> selected) {
                if (selected.Count > 0)
                    menu.AddItem(new GUIContent("Select"), false, () => {
                        Selection.objects = selected.Select(i => i.asItemKind.content.reference).ToArray();
                    });
                
                if (selected.Count == 1)
                    selected[0].asItemKind.content.ContextMenu(menu);
            }

            public override bool CanRename(ItemInfo info) {
                return false;
            }

            protected override bool CanStartDrag(CanStartDragArgs args) {
                return false;
            }
        }
    }
    
}
