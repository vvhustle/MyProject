﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Sounds;

#if FMOD
using FMODUnity;
#endif

namespace Yurowm {
    [CustomEditor(typeof(ContentSound))]
    public class ContentSoundEditor : UnityEditor.Editor {
        
        ContentSound provider;

        void OnEnable () {
            provider = (ContentSound) target;
            
            RemoveEmpty();
            
            Undo.RecordObject(provider, null);
        }

        IEnumerable<string> GetEvents() {
            #if FMOD
            foreach (var path in EventManager.Events.Select(e => e.Path))
                yield return path;
            #else
            yield break;
            #endif
        }
        
        public override void OnInspectorGUI() {
            if (!serializedObject.isEditingMultipleObjects) {
                Undo.RecordObject(target, "ContentSound is changed");
                
                using (GUIHelper.Change.Start(SetDirty)) {
                    foreach (var clip in provider.clips)
                        ClipSelector(clip);

                    NewClip();
                }
                
                RemoveEmpty();
            }
        }
        
        void RemoveEmpty() {
            if (provider.clips.Any(c => c == null || c.name.IsNullOrEmpty()))
                provider.clips = provider.clips
                    .Where(c => c != null && !c.name.IsNullOrEmpty())
                    .ToList();
        }

        void SetDirty() {
            EditorUtility.SetDirty(target);
        }
        
        void ClipSelector(ContentSound.Sound selected) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            
            if (Event.current.type == EventType.Layout) return;

            Rect rectLabel = rect;
            rectLabel.width = EditorGUIUtility.labelWidth;

            selected.name = EditorGUI.TextField(rectLabel, selected.name);
            
            rect.xMin = rectLabel.xMax;
            
            if (GUI.Button(rect, selected.clip ?? "-", EditorStyles.miniButton))
                SelectEvent(selected);
            
        }
        
        void SelectEvent(ContentSound.Sound selected) {
            GenericMenu menu = new GenericMenu();

            #if FMOD
            foreach (var e in GetEvents()) {
                var _e = e;
                menu.AddItem(new GUIContent(e), false, () => {
                    selected.clip = _e;
                    
                    if (selected.name.IsNullOrEmpty()) {
                        int index = _e.LastIndexOf("/", StringComparison.Ordinal);
                        if (index >= 0) 
                            selected.name = _e.Substring(index + 1);
                        else
                            selected.name = _e;
                    }
                });
            }
            #endif
                
            foreach (var sound in SoundBase.storage.Items<SoundEffect>()) {
                var ID = sound.ID;
                menu.AddItem(new GUIContent("YSounds/" + sound.ID), false, () => {
                    selected.clip = ID;
                    
                    if (selected.name.IsNullOrEmpty()) {
                        int index = ID.LastIndexOf("/", StringComparison.Ordinal);
                        if (index >= 0) 
                            selected.name = ID.Substring(index + 1);
                        else
                            selected.name = ID;
                    }
                });
            }
            
            if (menu.GetItemCount() > 0)
                menu.ShowAsContext();
        }
        
        ContentSound.Sound newSound;
        
        void NewClip() {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            
            if (Event.current.type == EventType.Layout) return;

            rect.xMin = rect.xMax - 30;
            
            if (newSound != null && !newSound.clip.IsNullOrEmpty()) {
                provider.clips.Add(newSound);
                newSound = null;
            }

            if (newSound == null)
                newSound = new ContentSound.Sound("");
            
            if (GUI.Button(rect, "+", EditorStyles.miniButton))
                SelectEvent(newSound);
        }
    }
}