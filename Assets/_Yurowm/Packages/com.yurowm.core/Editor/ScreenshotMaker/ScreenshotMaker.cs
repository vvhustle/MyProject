using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using Yurowm.Dashboard;
using Yurowm.GUIHelpers;

namespace Yurowm {
    [DashboardGroup("Development")]
    [DashboardTab("Screenshot Maker", null, "tab.screenshots")]
    public class ScreenshotMaker : DashboardEditor {
        
        static DirectoryInfo folder = null;
        public override bool Initialize() {
            return true;
        }

        static void CreateFolder() {
            if (folder == null) {
                var projectFolder= new DirectoryInfo(Application.dataPath).Parent?.FullName ?? "";
                folder = new DirectoryInfo(Path.Combine(projectFolder, "Screenshots"));
                if (!folder.Exists) folder.Create();
            }
        }

        public override void OnGUI() {
                
            using (GUIHelper.Lock.Start(!Application.isPlaying)) {
                if (GUIHelper.Button("Time Scale Pause", Time.timeScale == 0 ? "[PAUSED]" : "[PLAYING]"))
                    Time.timeScale = Time.timeScale == 0 ? 1 : 0;
                //
                //
                // if (GUIHelper.Button("Set Resolution", "...")) {
                //     GenericMenu menu = new GenericMenu();
                //     
                //     menu.AddItem(new GUIContent("iPhone 5.5"), false, () => Screen.SetResolution(1242, 2208, false));
                //     menu.AddItem(new GUIContent("iPhone 6.5"), false, () => Screen.SetResolution(1242, 2688, false));
                //     menu.AddItem(new GUIContent("iPad"), false, () => Screen.SetResolution(2048, 2732, false));
                //     menu.AddItem(new GUIContent("FullHD"), false, () => Screen.SetResolution(1080, 1920, false));
                //     
                //     menu.ShowAsContext();
                // }
                
                using (GUIHelper.Horizontal.Start()) {
                    if (GUILayout.Button("Shot and Save", EditorStyles.miniButtonLeft, GUILayout.Width(150))) Shot(1);
                    if (GUILayout.Button("X2", EditorStyles.miniButtonMid, GUILayout.Width(40))) Shot(2);
                    if (GUILayout.Button("X5", EditorStyles.miniButtonMid, GUILayout.Width(40))) Shot(5);
                    if (GUILayout.Button("X10", EditorStyles.miniButtonRight, GUILayout.Width(40))) Shot(10);
                }
            }
            
        }

        static void Shot(int size) {
            CreateFolder();

            string fileName = $"Screen_{Application.productName}_{DateTime.Now}";
            fileName = fileName.Replace('/', '-').Replace(' ', '_').Replace(':', '-');
            fileName += ".png";

            FileInfo file = new FileInfo(Path.Combine(folder.FullName, fileName));
            ScreenCapture.CaptureScreenshot(file.FullName, size);
        }
        
        [MenuItem("Yurowm/Screenshot/Create x1 %1")]
        static void Shot1() {
            Shot(1);
        }
            
        [MenuItem("Yurowm/Screenshot/Create x2 %2")]
        static void Shot2() {
            Shot(2);
        }

        [MenuItem("Yurowm/Screenshot/Create x5 %5")]
        static void Shot5() {
            Shot(5);
        }

        [MenuItem("Yurowm/Screenshot/Create x10 %0")]
        static void Shot10() {
            Shot(10);
        }
    }
}