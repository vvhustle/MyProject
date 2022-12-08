using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Dashboard;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Help;
using Yurowm.HierarchyLists;
using Yurowm.Nodes.Editor;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    public class LevelDesignFileWorld {
        public string name;
        static int Indexer;
        public readonly int Index;
        public string lastSelectedLevelID = null;

        public List<LevelScriptOrderedFile> files = new List<LevelScriptOrderedFile>();
            
        public LevelDesignFileWorld(string name) {
            this.name = name;
            Index = Indexer ++;
        }
    }
    
    public abstract class WorldPopup : DashboardPopup {
        public LevelEditorContext context;
        
        protected string worldName = "Untitled";
        protected string error;
        
        protected virtual void ValidateName() {
            if (context.controller.worlds.Any(w => w.name == worldName)) {
                error = "One of the existed worlds has the such name";
                return;
                
            }
            
            error = null;
        }
    }
    
    public class RenameWorldPopup : WorldPopup {
        public LevelDesignFileWorld worldFile;
        
        
        public override void Initialize() {
            base.Initialize();
            titleContent = new GUIContent("Rename world");
            worldName = worldFile.name;
            ValidateName();
        }

        protected override void ValidateName() {
            if (worldName == worldFile.name) {
                error = "It is the same name";
                return;
            }
            
            base.ValidateName();
        }

        public override void OnGUI() {
            EditorGUILayout.LabelField("Current Name", worldFile.name);

            using (GUIHelper.Change.Start(ValidateName)) 
                worldName = EditorGUILayout.TextField("New Name", worldName);

            GUILayout.FlexibleSpace();

            if (!error.IsNullOrEmpty())
                EditorGUILayout.HelpBox(error, MessageType.Error);
            
            using (GUIHelper.Lock.Start(!error.IsNullOrEmpty()))
                if (GUILayout.Button("Rename")) {
                    context.folders.RenameWorld(worldFile.name, worldName);
                    worldFile.name = worldName;
                    worldFile.files.ForEach(w => w.World = worldName);
                    Close();
                }
        }
    }
    
    public class NewWorldPopup : WorldPopup {
        
        public override void Initialize() {
            base.Initialize();
            titleContent = new GUIContent("New world");
        }

        public override void OnGUI() {
            using (GUIHelper.Change.Start(ValidateName)) 
                worldName = EditorGUILayout.TextField("Name", worldName);

            GUILayout.FlexibleSpace();

            if (!error.IsNullOrEmpty())
                EditorGUILayout.HelpBox(error, MessageType.Error);
            
            using (GUIHelper.Lock.Start(!error.IsNullOrEmpty()))
                if (GUILayout.Button("Create")) {
                    var result = new LevelDesignFileWorld(worldName);
                    context.controller.worlds.Add(result);
                    context.controller.SelectWorld(result);
                    Close();
                }
        }
    }
}