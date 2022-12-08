using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.HierarchyLists;
using Yurowm.Icons;

namespace YMatchThree.Editor {
    public class SlotLayersList : NonHierarchyList<SlotLayerBase> {
        LevelLayoutEditor editor;
        public readonly List<SlotLayerBase> selected = new List<SlotLayerBase>();
        
        static readonly Color visibleColor = new Color(0.1f, 0.75f, 1f);
        static readonly Color invisibleColor = new Color(0.05f, 0.13f, 0.26f);
        static Texture2D layerIcon;

        public SlotLayersList(List<SlotLayerBase> collection, LevelLayoutEditor editor) : base(collection) {
            this.editor = editor;
            onChanged += editor.SetDirty;
            onSelectedItemChanged += list => {
                selected.Clear();
                selected.AddRange(list);
            };
        }

        public override int GetUniqueID(SlotLayerBase element) {
            return element.ID.GetHashCode();
        }

        public override void OnGUI(Rect rect) {
            if (layerIcon == null) layerIcon = EditorIcons.GetIcon("LayerListIcon");
            base.OnGUI(rect);
        }

        public override void DrawItem(Rect rect, ItemInfo info) {
            Color color;
            
            if (selected.Contains(info.content))
                color = visibleColor;
            else
                color = invisibleColor;
            
            rect = ItemIconDrawer.Draw(rect, layerIcon, color);

            switch (info.content) {
                case AllSlotsLayer _: 
                    rect = ItemIconDrawer.DrawTag(rect, "All");
                    break;
                case SlotLayer _: 
                    rect = ItemIconDrawer.DrawTag(rect, "Few");
                    break;
            }
            
            if (info.content.isDefault)
                rect = ItemIconDrawer.DrawTag(rect, "Default");
            
            GUI.Label(rect, GetLabel(info), EditorStyles.label);
        }
        
        public void AddNewLayer(SlotLayerBase slotLayer) {
            newLayer = slotLayer;
            
            if (editor.design.layers.All(l => !l.isDefault))
                newLayer.isDefault = true;
            
            selected.Reuse(new [] { newLayer });
            
            AddNewItem(headFolder, null);
        }

        public override SlotLayerBase CreateItem() {
            var result = newLayer;
            newLayer = null;
            return result;
        }

        public override string GetName(SlotLayerBase element) {
            return element.ID;
        }

        public override void SetName(SlotLayerBase element, string name) {
            element.ID = name;
        }

        SlotLayerBase newLayer;

        public override void ContextMenu(GenericMenu menu, List<IInfo> selected) {
            if (selected.Count == 1) {
                menu.AddItem(new GUIContent("Select"), false, () => {
                    var l = selected[0].asItemKind.content;
                    editor.selected = editor.design.slots
                        .Where(s => l.Contains(editor.design, s))
                        .Select(s => s.coordinate)
                        .ToList();
                });
                menu.AddItem(new GUIContent("Set Default"), false, func: () => {
                    var sl = selected[0].asItemKind.content;
                    itemCollection.ForEach(l => l.isDefault = l == sl);     
                    onChanged();
                });
            }
            
            if (selected.Count == 1 && selected[0].asItemKind.content is SlotLayer layer) {
                var selectedSlots = editor.design.slots
                    .Where(s => editor.selected.Contains(s.coordinate))
                    .ToArray();
                if (!selectedSlots.IsEmpty())
                    menu.AddItem(new GUIContent("Set Selection"), false, () => {
                        layer.SetSelection(selectedSlots);
                        onChanged();
                    });
            }
            
            if (selected.Count > 0)
                menu.AddItem(new GUIContent("Remove"), false, () => Remove(selected.ToArray()));
        }
    }
}