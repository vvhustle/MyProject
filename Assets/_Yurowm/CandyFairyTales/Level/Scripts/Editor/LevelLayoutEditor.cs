using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Help;
using Yurowm.HierarchyLists;
using Yurowm.Icons;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class LevelLayoutEditor {
        
        GUIHelper.LayoutSplitter splitter;
        
        public LevelLayoutEditor() {
            splitter = new GUIHelper.LayoutSplitter(OrientationLine.Horizontal, OrientationLine.Vertical, 250);

            LoadPinned();
            
            SetupField();
        }

        public Action repaint = delegate {};
        
        void Repaint() {
            repaint.Invoke();
        }
        
        #region Storage Selectors

        StorageSelector<LevelContent> gameplaySelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is LevelGameplay);
        
        StorageSelector<LevelContent> physicSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is ChipPhysic);
        
        #endregion
        
        #region Level

        public Level design {
            get;
            private set;
        }

        public LevelScriptBase script;
        
        SlotLayersList slotLayersList; 
        
        public void SetLevel(Level level) {
            design = level;
            if (design.gamePlay.IsNullOrEmpty())
                LevelScriptOrderedPresetDefault.SetupLevel(design);
                
            extensionsList = new ExtensionsList(design.extensions);
            extensionsList.onChanged += SetDirty;
            extensionsList.onChanged += OnSelectionChanged;
            extensionsList.drawOverlay = GUIHelper.DrawSeparatorRect;

            slotLayersList = new SlotLayersList(level.layers, this);
            slotLayersList.selected.AddRange(level.layers);
            slotLayersList.drawOverlay = GUIHelper.DrawSeparatorRect;
            
            design.gamePlay = gameplaySelector.Select(c => c.ID == design.gamePlay)?.ID;
            design.physic = physicSelector.Select(c => c.ID == design.physic)?.ID;
            
            ReadSlots();
            
            selected.Clear();
            
            OnSelectionChanged();
        }

        bool dirty;
        public void SetDirty() {
            dirty = true;
        }
        
        public Dictionary<int2, SlotInfo> slots;
        
        public SlotInfo NewSlot(int2 coord, bool replace = false) {
            SlotInfo currentSlot = design.slots.Find(x => x.coordinate == coord);
            if (currentSlot == null || replace) {
                SlotInfo newSlot = new SlotInfo(coord);
                
                if (currentSlot != null)
                    design.slots.Remove(currentSlot);
                
                design.slots.Add(newSlot);
                slots[coord] = newSlot;
                
                SetDirty();
                
                return newSlot;
            }
            return null;
        }
        
        void ReadSlots() {            
            slots = design.slots.ToDictionaryKey(x => x.coordinate);
        }
            
        void Vacuum() {
            area area = new area(int2.zero, design.size);
            design.slots.RemoveAll(x => !area.Contains(x.coordinate));
            SetDirty();
        }
        
        public Action onGetDirty = delegate {};

        #endregion

        public void OnGUI() {
            using (splitter.Start(true, true)) {
                if (splitter.Area())
                    OnParamtersGUI();
                
                if (splitter.Area())
                    OnLayoutGUI();
            }
            
            if (dirty) {
                onGetDirty.Invoke();
                dirty = false;
            }
            
            Repaint();
        }

        #region Paramters GUI

        enum EditMode {
            Parameters,
            Layers,
            Extensions,
            Selection
        }
        
        EditMode editMode;

        void OnParamtersGUI() {
            EditorGUILayout.Space();
            
            #region EditMode

            {
                void DrawModeButton(EditMode mode, GUIStyle style) {
                    var selected = editMode == mode;
                    if (selected != GUILayout.Toggle(selected, mode.ToString(), style))
                        editMode = mode;
                } 
                
                using (GUIHelper.Horizontal.Start()) {
                    DrawModeButton(EditMode.Parameters, EditorStyles.miniButtonLeft);
                    DrawModeButton(EditMode.Layers, EditorStyles.miniButtonMid);
                    DrawModeButton(EditMode.Extensions, EditorStyles.miniButtonMid);
                    DrawModeButton(EditMode.Selection, EditorStyles.miniButtonRight);
                }
            }

            #endregion

            EditorGUILayout.Space();

            using (GUIHelper.EditorLabelWidth.Start(120))
            using (GUIHelper.Change.Start(SetDirty)) {
                switch (editMode) {
                    case EditMode.Parameters: OnLevelParametersGUI(); break;
                    case EditMode.Layers: OnLayersGUI(); break;
                    case EditMode.Extensions: OnExtensionsGUI(); break;
                    case EditMode.Selection: OnSelectionGUI(); break;
                }
            }
        }
        
        void OnLevelParametersGUI() {
            gameplaySelector.Draw("Gameplay", c => {
                design.gamePlay = c.ID;
                SetDirty();
            });
            EditorTips.PopLastRectByID("lle.gamplay");

            physicSelector.Draw("Physic", c => {
                design.physic = c.ID;
                SetDirty();
            });
            EditorTips.PopLastRectByID("lle.physic");

            design.width =
                Mathf.RoundToInt(EditorGUILayout.Slider("Width", design.width, Level.minSize, Level.maxSize));
            EditorTips.PopLastRectByID("lle.width");

            design.height =
                Mathf.RoundToInt(EditorGUILayout.Slider("Height", design.height, Level.minSize, Level.maxSize));
            EditorTips.PopLastRectByID("lle.height");

            using (GUIHelper.Horizontal.Start()) {
                EditorGUILayout.PrefixLabel("Seed");
                var hasSeed = design.randomSeed != 0;
                if (EditorGUILayout.Toggle(hasSeed, GUILayout.Width(25)) != hasSeed) {
                    hasSeed = !hasSeed;
                    design.randomSeed = hasSeed ? YRandom.main.Seed() : 0;
                }

                using (GUIHelper.Lock.Start(!hasSeed))
                    design.randomSeed = EditorGUILayout.LongField(design.randomSeed);
            }

            EditorTips.PopLastRectByID("lle.seed");
        }

        void OnLayersGUI() {

            using (GUIHelper.Horizontal.Start()) {
                if (GUILayout.Button("Show All Slots", EditorStyles.miniButtonLeft))
                    slotLayersList.selected.Reuse(design.layers);

                if (GUILayout.Button("Show Default", EditorStyles.miniButtonRight))
                    slotLayersList.selected.Reuse(new[] {design.layers.FirstOrDefault(l => l.isDefault)});
            }

            using (GUIHelper.Horizontal.Start()) {
                using (GUIHelper.Lock.Start(selected.IsEmpty()))
                    if (GUILayout.Button("New \"Few\" Layer", EditorStyles.miniButtonLeft)) {
                        var result = new SlotLayer();
                        result.SetSelection(design.slots.Where(s => selected.Contains(s.coordinate)));
                        slotLayersList.AddNewLayer(result);
                    }

                using (GUIHelper.Lock.Start(design.layers.CastOne<AllSlotsLayer>() != null))
                    if (GUILayout.Button("New \"All\" Layer", EditorStyles.miniButtonRight))
                        slotLayersList.AddNewLayer(new AllSlotsLayer());
            }
            
            EditorGUILayout.Space();
            
            slotLayersList.OnGUI(GUILayout.MaxHeight(slotLayersList.totalHeight + 30));
            EditorTips.PopLastRectByID("lle.layers");
        }

        #endregion

        #region Extensions
        
        ExtensionsList extensionsList = null;

        void OnExtensionsGUI() {
            extensionsList.OnGUI(GUILayout.MaxHeight(extensionsList.totalHeight + 30));    
             
            using (GUIHelper.Vertical.Start()) {
                if (extensionsList.selected.Count == 0) {
                    GUILayout.Label("Nothing Selected", Styles.centeredMiniLabel);
                } else {
                    if (extensionsList.selected.Count == 1) 
                        GUILayout.Label(extensionsList.selected[0].ID.NameFormat(), Styles.centeredMiniLabel);
                     
                    ObjectEditor.Edit(extensionsList.selected.ToArray(), field);                 
                }
                 
                EditorGUILayout.Space();
            }
        }
        
        class ExtensionsList : NonHierarchyList<ContentInfo> {
            
            public List<ContentInfo> selected = new List<ContentInfo>();
            
            public ExtensionsList(List<ContentInfo> collection) : base(collection) {
                
                onSelectedItemChanged = x => {
                    selected.Clear();
                    selected.AddRange(x);
                };
            }
            
            Texture2D listExtensionIcon;

            public override void OnGUI(Rect rect) {
                if (!listExtensionIcon)
                    listExtensionIcon = EditorIcons.GetIcon("ExtensionListIcon");
                base.OnGUI(rect);
            }

            public override void DrawItem(Rect rect, ItemInfo info) {
                if (selected.Contains(info.content))
                    Highlight(rect, true);
                
                rect = ItemIconDrawer.DrawSolid(rect, listExtensionIcon);

                GUI.Label(rect, info.content.ID);
            }
            
            Color hightlightFace = new Color(0, 1, 1, 0.05f);
            void Highlight(Rect rect, bool outline = false) {
                Handles.DrawSolidRectangleWithOutline(rect, hightlightFace, outline ? Color.cyan : Color.clear);
            }
            
            public override void ContextMenu(GenericMenu menu, List<IInfo> selected) {
                selected = selected.Where(x => x.isItemKind).ToList();
                foreach (var extension in LevelContent.storage.Items<LevelExtension>()) {
                    if (extension.IsUnique && itemCollection.Any(e => e.Reference == extension))
                        continue;
                    
                    var _extension = extension;
                    menu.AddItem(new GUIContent($"Add/{extension.GetContentBaseType().Name}/{extension.ID}"), false, () => {
                        newItemReference = _extension;
                        AddNewItem(headFolder, null);
                    });
                }
                if (selected.Count > 0)
                    menu.AddItem(new GUIContent("Remove"), false, () => Remove(selected.ToArray()));
            }

            public override int GetUniqueID(ContentInfo element) {
                return element.GetHashCode();
            }

            LevelContent newItemReference = null;
            
            public override ContentInfo CreateItem() {
                return new ContentInfo(newItemReference);
            }

            protected override bool CanRename(TreeViewItem item) {
                return false;
            }
        }
        
        #endregion

        #region Selection
        
        SelectionList selectionList;
        public List<int2> selected = new List<int2>();
        
        List<LevelContent> pinned = new List<LevelContent>();
        
        const string pinnedStorageKey = "levelLayout.pinned";
        
        void SavePinned() {
            EditorStorage.Instance
                .SetText(pinnedStorageKey, pinned
                    .Select(f => f.ID)
                    .Join(";") ?? "");
        }
        
        void LoadPinned() {
            pinned.Clear();
            var raw = EditorStorage.Instance.GetText(pinnedStorageKey);
            if (!raw.IsNullOrEmpty())
                pinned.AddRange(raw
                    .Split(";")
                    .Select(id => LevelContent.GetItem<LevelContent>(id))
                    .NotNull());  
        }

        void OnSelectionGUI() {
        
            ObjectEditor.Edit(slots.Values.Where(c => selected.Contains(c.coordinate)));
            
            if (selectionList != null) {
                
                selectionList.OnGUI(Mathf.Min(300, selectionList.totalHeight + 30));

                EditorGUILayout.Space();
                using (GUIHelper.Vertical.Start(GUIHelper.DrawSeparatorRect))
                    selectionList.DrawSelected();
            }

            EditorGUILayout.Space();
        }

        void OnSelectionChanged() {
            var selectedContent = selectionList?.selectedContent;
            
            selectionList = new SelectionList(selected, this);
            selectionList.onChanged += SetDirty;
            selectionList.drawOverlay = GUIHelper.DrawSeparatorRect;

            if (selectedContent) selectionList.Select(selectedContent);
        }
        
        public Action<SlotInfo> onSlotClick = delegate {};

        void SelectionControl(int2 coord) {
            SlotInfo slot = slots.Get(coord);
            if (slot != null) onSlotClick.Invoke(slot);
            
            if (Event.current.shift && selected.Count > 0) {
                int2 start = selected.Last();
                int2 delta = new int2();
                delta.X = start.X < coord.X ? 1 : -1;
                delta.Y = start.Y < coord.Y ? 1 : -1;
                int2 cursor = new int2();
                for (cursor.X = start.X; cursor.X != coord.X + delta.X; cursor.X += delta.X)
                for (cursor.Y = start.Y; cursor.Y != coord.Y + delta.Y; cursor.Y += delta.Y)
                    if (!selected.Contains(cursor))
                        selected.Add(cursor);
            } else {
                if (!Event.current.control)
                    selected.Clear();
                if (selected.Contains(coord))
                    selected.Remove(coord);
                else
                    selected.Add(coord);
            }

            editMode = EditMode.Selection;
            
            OnSelectionChanged();
            Repaint();
        }
        
        class SelectionList : NonHierarchyList<LevelContent> {
            LevelLayoutEditor editor;
            public LevelContent selectedContent;
            static Dictionary<LevelContent, Dictionary<int2, ContentInfo>> references = null;
            Dictionary<int2, SlotInfo> slots;
            List<int2> selection;
            List<SlotContent> avaliable;

            public SelectionList(List<int2> coord, LevelLayoutEditor editor) :
                base(ExtractContent(ExtractSlots(coord, editor), editor)) {
                
                slots = ExtractSlots(coord, editor);
                selection = coord;
                
                avaliable = itemCollection
                    .CastIfPossible<SlotContent>()
                    .Where(c => slots.Values.Any(s => s.HasContent(c)))
                    .ToList();
                
                this.editor = editor;
                onSelectedItemChanged = x => {
                    if (x.Count == 1) selectedContent = x[0];
                };
                
                onDoubleClick = OnDoubleClick;
            }

            static Dictionary<int2, SlotInfo> ExtractSlots(List<int2> coord, LevelLayoutEditor editor) {
                return coord
                    .ToDictionary(x => x, x => editor.slots.Get(x))
                    .RemoveAll(x => x.Value == null);
            }

            static List<LevelContent> ExtractContent(Dictionary<int2, SlotInfo> slots, LevelLayoutEditor editor) {
                references = new Dictionary<LevelContent, Dictionary<int2, ContentInfo>>();
                
                foreach (SlotContent contentRef in LevelContent.storage.Items<SlotContent>()) {
                    Dictionary<int2, ContentInfo> content = new Dictionary<int2, ContentInfo>();
                    foreach (SlotInfo slot in slots.Values) {
                        ContentInfo c = slot.GetContent(x => x.ID == contentRef.ID);
                        if (c != null) content.Set(slot.coordinate, c);
                    }

                    if (content.Count > 0)
                        references.Set(contentRef, content);
                }

                var result = editor.design.slots.SelectMany(s => s.Content())
                    .Select(c => c.Reference)
                    .Concat(editor.pinned)
                    .Distinct()
                    .ToList();
                
                result.AddRange(editor.design.extensions
                    .Select(e => e.Reference)
                    .CastIfPossible<LevelSlotExtension>());
                
                result.Sort(c => c.ID);
                
                return result;
            }

            public override void ContextMenu(GenericMenu menu, List<IInfo> selected) {
                var newContent = LevelContent.storage
                    .Items<SlotContent>()
                    .ToList();
                
                newContent.RemoveAll(references.ContainsKey);

                selected = selected.Where(x => x.isItemKind).ToList();

                foreach (var content in newContent) {
                    
                    #region BigObjects Filter
                    
                    if (content is IBigSlotContent bigC) {
                        if (slots.Count != 1) continue;
                        var coord = slots.Keys.First();
                        if (bigC.GetBigShape().GetCoords()
                            .Select(c => editor.slots.Get(c + coord))
                            .Any(s => s == null || content.IsUniqueContent() && s.HasBaseContent(content)))
                            continue;
                    }
                    
                    #endregion
                    
                    var _content = content;
                
                    menu.AddItem(new GUIContent($"Add/{content.GetContentBaseType().Name}/{content.ID.NameFormat()}"), false, () => {
                        newContentRef = _content;
                        AddNewItem(headFolder, null);
                    });
                }
                    
                if (selected.Count > 0) {
                    var content = selected
                        .Select(s => s.asItemKind.content)
                        .CastIfPossible<SlotContent>()
                        .ToArray();
                    
                    if (content.Length > 0)
                        menu.AddItem(new GUIContent("Select"), false, () => {
                            SelectSlots(content);
                        });
                    
                    menu.AddItem(new GUIContent("Remove"), false, () => Remove(selected.ToArray()));
                
                    var isPinned = selected.All(s => editor.pinned.Contains(s.asItemKind.content));
                    
                    menu.AddItem(new GUIContent(isPinned ? "Unpin" : "Pin"), false, () => {
                        if (isPinned) {
                            selected.ForEach(s => editor.pinned.Remove(s.asItemKind.content));
                        } else {
                            selected.ForEach(s => editor.pinned.Add(s.asItemKind.content));
                            editor.pinned = editor.pinned.Distinct().ToList();
                        }
                        editor.SavePinned();
                        editor.OnSelectionChanged();
                    });
                }
            }

            void OnDoubleClick(LevelContent content) {
                if (content is SlotContent slotContent) {
                    if (Event.current.shift) {
                        SelectSlots(new []{ slotContent });
                        return;
                    } 
                    
                    if (Event.current.control) {
                        var filled = slots.Values
                            .All(s => s.HasContent(slotContent));

                        if (filled) {
                            slots.Values
                                .ForEach(s => s.RemoveContent(c => c.ID == slotContent.ID));
                        } else {
                            slots.Values
                                .ForEach(s => s.AddContent(slotContent));
                        }
                        
                        editor.OnSelectionChanged();
                        onChanged();
                    }
                }
                    
            }

            void SelectSlots(SlotContent[] content) {
                editor.selected = editor.design.slots
                    .Where(s => content.Any(s.HasContent))
                    .Select(s => s.coordinate)
                    .ToList();
                editor.OnSelectionChanged();
            }
            
            new void Remove(IInfo[] selected) {
                foreach (IInfo info in selected) 
                foreach (SlotInfo slot in slots.Values) 
                    slot.RemoveContent(x => x.ID == info.asItemKind.content.ID);
                editor.OnSelectionChanged();
                onChanged();
            }

            protected override bool CanRename(TreeViewItem item) {
                return false;
            }

            protected override bool CanStartDrag(CanStartDragArgs args) {
                return false;
            }
            
            SlotContent newContentRef = null;
            public override LevelContent CreateItem() {
                if (newContentRef == null) return null;

                List<SlotInfo> slots = 
                    selection.Select(c => editor.slots.Get(c) ?? editor.NewSlot(c))
                        .ToList();

                Type baseType = newContentRef.GetContentBaseType();            
            
                if (newContentRef.IsUniqueContent() && this.slots.Values.Any(x => x.HasContent(newContentRef)) &&
                    EditorUtility.DisplayDialog("Replace", "Do you want to replace existing content to this one?",
                        "Replace", "Cancel")) {
                    slots.ForEach(s => s.RemoveContent(c => c.BaseType == baseType));
                }

                foreach (SlotInfo slot in slots) {
                    if (!slot.HasContent(newContentRef))
                        slot.AddContent(newContentRef);
                }

                selectedContent = newContentRef;
                onChanged();
                editor.OnSelectionChanged();
                return newContentRef;
            }

            static readonly Color unavaliableColor = Color.white.Transparent(.2f);

            #region Icons

            static Texture2D listChipIcon;
            static Texture2D listBlockIcon;
            static Texture2D listModifierIcon;
            static Texture2D listExtensionIcon;
            static Texture2D listOtherIcon;
            static Texture2D pinIcon;

            public override void OnGUI(Rect rect) {
                if (listChipIcon == null) {
                    listChipIcon = EditorIcons.GetIcon("ChipListIcon");
                    listBlockIcon = EditorIcons.GetIcon("BlockListIcon");
                    listModifierIcon = EditorIcons.GetIcon("ModifierListIcon");
                    listOtherIcon = EditorIcons.GetIcon("OtherListIcon");
                    listExtensionIcon = EditorIcons.GetIcon("ExtensionListIcon");
                    pinIcon = EditorIcons.GetIcon("Pin");
                }
                
                base.OnGUI(rect);
            }

            #endregion
            
            public override void DrawItem(Rect rect, ItemInfo info) {
                Texture2D icon;
                
                switch (info.content) {
                    case Chip _: icon = listChipIcon; break;
                    case Block _: icon = listBlockIcon; break;
                    case SlotModifier _: icon = listModifierIcon; break;
                    case LevelSlotExtension _: icon = listExtensionIcon; break;
                    default: icon = listOtherIcon; break;
                }

                rect = ItemIconDrawer.DrawSolid(rect, icon);
                
                if (editor.pinned.Contains(info.content))
                    rect = ItemIconDrawer.DrawSolid(rect, pinIcon, ItemIconDrawer.Side.Right);

                if (selectedContent == info.content) {
                    if (Event.current.shift) {
                        rect = ItemIconDrawer.DrawTag(rect, "Select", Color.cyan, ItemIconDrawer.Side.Right);
                    } else if (!slots.IsEmpty() && Event.current.control && selectedContent is SlotContent sc) {
                        if (slots.Values.All(s => s.HasContent(sc)))
                            rect = ItemIconDrawer.DrawTag(rect, "Remove", Color.red, ItemIconDrawer.Side.Right);
                        else
                            rect = ItemIconDrawer.DrawTag(rect, "Add", Color.green, ItemIconDrawer.Side.Right);
                    }
                }
                
                using (info.content is SlotContent && !avaliable.Contains(info.content) ?
                    GUIHelper.Color.Start(unavaliableColor) : null) 
                    GUI.Label(rect, info.content.ID.NameFormat());
            }

            public void DrawSelected() {
                if (!selectedContent) {
                    GUILayout.Label("Nothing Selected", Styles.centeredMiniLabel);
                } else {
                    GUILayout.Label(selectedContent.ID.NameFormat(), Styles.centeredMiniLabel);

                    switch (selectedContent) {
                        case SlotContent slotContent: {
                            var selection = editor.selected
                                .Select(c => editor.slots.Get(c)?.GetContent(slotContent))
                                .NotNull()
                                .ToArray();
                            ObjectEditor.Edit(selection, editor.field);
                            break;
                        }
                        case LevelSlotExtension extension: {
                            var slots = editor.selected
                                .Select(c => editor.slots.Get(c))
                                .NotNull()
                                .ToArray();
                            ObjectEditor.Edit(new LevelSlotExtensionSelection {
                                extension = editor.design.extensions.FirstOrDefault(e => e.Reference == extension),
                                slots = slots
                            }, editor.field);
                            
                            break;
                        }
                    }
                }

                EditorGUILayout.Space();
            }

            public override int GetUniqueID(LevelContent element) {
                return element.ID.GetHashCode();
            }

            public override bool ObjectToItem(UnityEngine.Object o, out IInfo result) {
                result = null;
                return false;
            }

            public void Select(LevelContent content) {
                selectedContent = content;
                SetSelection(new List<int>() { GetUniqueID(content) });
            }
        }

        #endregion

        #region Layout
        
        public readonly LevelFieldEditor field = new LevelFieldEditor();
        
        void SetupField() {
            
            Color referenceColor = Color.yellow.Transparent(.2f);
            Color selectedColor = Color.cyan;
            
            field.context = this;
            
            field.repaint = Repaint;
            
            field.onMainClick += SelectionControl;
            
            field.onSecondaryClick += coord => {
                if (selected.Contains(coord)) {
                    GenericMenu menu = new GenericMenu();
                    
                    selectionList.ContextMenu(menu, new List<SelectionList.IInfo>());
                    
                    menu.AddItem(new GUIContent("Clear"), false, () => {
                        selected.Where(x => slots.ContainsKey(x))
                            .ForEach(x => slots[x].RemoveContent(c => true));
                        SetDirty();
                        OnSelectionChanged();
                    });
                    
                    if (selected.Any(c => !slots.ContainsKey(c))) {
                        menu.AddItem(new GUIContent("Slots/Create"), false, () => {
                            selected.ForEach(c => NewSlot(c));
                            ReadSlots();
                            SetDirty();
                        });
                    }
                    
                    menu.AddItem(new GUIContent("Slots/Default"), false, () => {
                        var slots = selected
                            .Select(c => design.slots.FirstOrDefault(s => s.coordinate == c) ?? NewSlot(c))
                            .NotNull()
                            .ToList();
                        
                        slots.ForEach(s => s.RemoveContent(c => true));

                        
                        foreach (var content in LevelContent.storage.GetAllDefault<SlotContent>())
                            if (content is IDefaultSlotContent dsc)
                                slots    
                                    .Where(slot => dsc.IsSuitableForNewSlot(design, slot))
                                    .ForEach(s => s.AddContent(new ContentInfo(content)));

                        ReadSlots();
                        SetDirty();
                    });
                    
                    if (selected.Any(c => slots.ContainsKey(c))) {
                        menu.AddItem(new GUIContent("Slots/Remove"), false, () => {
                            foreach (var c in selected)
                                if (slots.ContainsKey(c))
                                    design.slots.RemoveAll(s => s.coordinate == c);
                            ReadSlots();
                            SetDirty();
                        });
                    }
                    
                    menu.ShowAsContext();
                }
            };
            
            field.slotMask = slot => slotLayersList.selected.Any(l => l.Contains(design, slot));

            // Drawing highlights for slots which contains selected content
            field.onPostDrawing += () => {
                var reference = selectionList?.selectedContent;
                
                if (reference is SlotContent slotContent) 
                    field.DrawOutlinedShape(slots
                        .Where(p => p.Value.HasContent(slotContent))
                        .Select(p => p.Key)
                        .ToArray(), referenceColor.Transparent(.1f), referenceColor);
            };
            
            // Drawing highlights for selected slots
            field.onPostDrawing += () => 
                field.DrawOutlinedShape(selected, selectedColor.Transparent(.2f), selectedColor);
        }
        
        void OnLayoutGUI() {
            if (design == null) return;
            
            field.fieldSize = design.size;
            field.slots = slots;
            field.extensions = design.extensions;
            
            field.PrepareToDraw();
            field.DrawFieldView();

            if (GUI.enabled) {
                field.DrawFieldViewCoordinates();
                field.DrawFieldViewResizerButtons(FieldResizeCallback);
            }
            
            if (dirty)
                Vacuum();
        }
        

        void FieldResizeCallback(OrientationLine orientation, int position, LevelFieldEditor.ResizeMode mode) {
            
            var slotVariables = new List<IFieldSensitiveVariable>();
            
            slotVariables.AddRange(design.extensions
                .SelectMany(e => e.Variables())
                .CastIfPossible<IFieldSensitiveVariable>());
            
            slotVariables.AddRange(design.slots
                .SelectMany(s => s.Content())
                .SelectMany(e => e.Variables())
                .CastIfPossible<IFieldSensitiveVariable>());

            switch (orientation) {
                case OrientationLine.Horizontal: {
                    switch (mode) {
                        case LevelFieldEditor.ResizeMode.Add: {
                            design.width++;
                                
                            design.slots
                                .Where(s => s.coordinate.X >= position)
                                .OrderByDescending(s => s.coordinate.X)
                                .ToArray()
                                .ForEach(s => {
                                    slotVariables.ForEach(v => v.MoveSlot(s.coordinate, s.coordinate.Right())); 
                                    s.coordinate.X++;
                                });
                            
                            ReadSlots();
                        } break;
                        case LevelFieldEditor.ResizeMode.Remove: {
                            design.width--; 
                            
                            design.slots.RemoveAll(s => {
                                if (s.coordinate.X == position) {
                                    slotVariables.ForEach(v => v.RemoveSlot(s.coordinate)); 
                                    return true;
                                }
                                return false;
                            });
                            
                            design.slots
                                .Where(s => s.coordinate.X > position)
                                .ToArray()
                                .OrderBy(s => s.coordinate.X)
                                .ForEach(s => {
                                    slotVariables.ForEach(v => v.MoveSlot(s.coordinate, s.coordinate.Left())); 
                                    s.coordinate.X--;
                                });
                            
                            ReadSlots();
                        } break;
                    }
                } break;
                case OrientationLine.Vertical: {
                    switch (mode) {
                        case LevelFieldEditor.ResizeMode.Add: {
                            design.height++; 
                            
                            design.slots
                                .Where(s => s.coordinate.Y >= position)
                                .OrderByDescending(s => s.coordinate.Y)
                                .ToArray()
                                .ForEach(s => {
                                    slotVariables.ForEach(v => v.MoveSlot(s.coordinate, s.coordinate.Up())); 
                                    s.coordinate.Y++;
                                });
                            
                            ReadSlots();
                        } break;
                        case LevelFieldEditor.ResizeMode.Remove: {
                            design.height--; 
                            
                            design.slots.RemoveAll(s => {
                                if (s.coordinate.Y == position) {
                                    slotVariables.ForEach(v => v.RemoveSlot(s.coordinate)); 
                                    return true; 
                                }
                                return false;
                            });
                            
                            design.slots
                                .Where(s => s.coordinate.Y > position)
                                .OrderBy(s => s.coordinate.Y)
                                .ToArray()
                                .ForEach(s => {
                                    slotVariables.ForEach(v => v.MoveSlot(s.coordinate, s.coordinate.Down())); 
                                    s.coordinate.Y--;
                                });
                            
                            ReadSlots();
                        } break;
                    }
                } break; 
            }
           
            SetDirty();
            Repaint();
        }

        #endregion
    }

    public abstract class ContentSelectionEditor<C> : ObjectEditor where C : class {
        public override void OnGUI(object obj, object context = null) {
            var list = (ContentInfo[]) obj;
            if (list.Length == 0) return;
            if (list[0].Reference is C)
                OnGUI(list, context as LevelFieldEditor);
        }
        
        public abstract void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor);
        
        public override bool IsSuitableType(Type type) {
            return typeof(ContentInfo[]).IsAssignableFrom(type);
        }
    }
}