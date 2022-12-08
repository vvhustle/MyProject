using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm.Console;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.UI {
    public class Page : ISerializableID, IStorageElementExtraData {
        
        [PreloadStorage]
        public static Storage<Page> storage = new Storage<Page>("Pages", TextCatalog.StreamingAssets, true);
        
        public string ID {get; set;}
        
        public PanelInfo.Mode defaultMode = PanelInfo.Mode.Disable;
        public List<PanelInfo> panels = new List<PanelInfo>();
        public List<PageExtension> extensions = new List<PageExtension>();
        static Dictionary<int, UIPanel> panelLinks;
        
        [OnLaunch(Behaviour.INITIALIZATION_ORDER + 1)]
        static void OnLaunch() {
            Initialize();
            GetDefault()?.Show();
            
            OnLaunchAttribute.unload += () => {
                IsInitialized = false;
                current = null;
                history.Clear();
            };
        }
        
        [QuickCommand("ui show", "MainMenu", "Show 'MainMenu' page")]
        static string ShowPageCommand(string pageID) {
            var page = Get(pageID); 
            if (page != null) {
                page.Show();
                return YConsole.Success("Success!");
            }

            return YConsole.Error("Page isn't found!");
        }
        
        [QuickCommand("ui pages", null, "Show all page names")]
        static IEnumerator ShowPageCommand() {
            foreach (var page in storage.items)
                yield return YConsole.Alias(page.ID);
        }
        
        static bool IsInitialized = false;
        
        static void Initialize() {
            if (IsInitialized) return;
            panelLinks = new Dictionary<int, UIPanel>();
            
            Behaviour
                .GetAll<UIPanel>()
                .ForEach(p => panelLinks.Add(p.linkID, p));

            IsInitialized = true;
            
            ShowQueue().Run();
        }

        #region Get
        
        public static UIPanel GetPanel(string name) {
            return panelLinks.Values.FirstOrDefault(p => p.name == name);
        }
        
        public static Page Get(string ID) {
            return storage.items.FirstOrDefault(p => p.ID == ID);
        }
        
        public static Page GetDefault() {
            return storage.GetDefault<Page>();
        }

        public static Page GetCurrent() {
            return history.Current ?? GetDefault();
        }
        
        public static Page GetWithTag(string tag) {
            return storage.items.FirstOrDefault(p => p.HasTag(tag));
        }
        
        #endregion
        
        #region Show
        
        static Queue<Action> showQueue = new Queue<Action>();
        
        public static Action<Page> onShow = delegate {};
        
        static History<Page> history = new History<Page>(100);
        
        static Page current;
        
        public void Show(bool immediate = false) {
            
            if (IsAnimating) {
                showQueue.Enqueue(() => Show(immediate));
                return;
            }

            if (current == this)
                return;
            
            current = this;
            
            history.Next(this);
            
            extensions.ForEach(e => e.OnShow());

            void SetMode(UIPanel panel, PanelInfo.Mode mode) {
                switch (mode) {
                    case PanelInfo.Mode.Ignore: break;
                    case PanelInfo.Mode.Enable: 
                        panel.SetVisible(true, immediate); break;
                    case PanelInfo.Mode.Disable:
                        panel.SetVisible(false, immediate);
                        break;
                }
            }
            
            List<UIPanel> uiPanelBuffer = new List<UIPanel>(panelLinks.Values);
            
            foreach (var info in panels) {
                var panel = info.panel;
                if (!panel) continue;
                uiPanelBuffer.Remove(panel);
                SetMode(panel, info.mode);
            }
            
            foreach (var panel in uiPanelBuffer) 
                SetMode(panel, defaultMode);

            UIRefresh.Invoke();
            
            onShow.Invoke(this);
        }
        
        public static void Back() {
            if (!IsAnimating)
                history.Back()?.Show();
        }
        
        public IEnumerator ShowAndWait() { 
            yield return WaitAnimation();
            Show();
            yield return WaitAnimation();
        }
        
        static IEnumerator ShowQueue() {
            while (true) {
                DebugPanel.Log("Is Animating", "UI", IsAnimating);
                DebugPanel.Log("Current Page ID", "UI", GetCurrent()?.ID);
                
                if (!IsAnimating && showQueue.Count > 0)
                    showQueue.Dequeue().Invoke();
                
                yield return null;
            }
        }

        #endregion

        #region Animation
        
        public static bool IsAnimating => HasActiveAnimations || !IsInitialized;

        public static IEnumerator WaitAnimation() {
            while (IsAnimating)
                yield return null;
        }
        
        static bool HasActiveAnimations => !animations.IsEmpty();        

        static List<ActiveAnimation> animations = new List<ActiveAnimation>();
        
        public static IDisposable NewActiveAnimation() {
            return new ActiveAnimation(animations);
        }
        
        class ActiveAnimation : IDisposable {
            List<ActiveAnimation> all;
            
            public ActiveAnimation(List<ActiveAnimation> all) {
                all.Add(this);
                this.all = all;
            }

            public void Dispose() {
                all.Remove(this);
            }
        }
        
        #endregion

        public class PanelInfo : ISerializable {
            public int linkID;
            public Mode mode = Mode.Enable;
            
            public UIPanel panel => panelLinks.Get(linkID);

            public enum Mode {
                Disable = 0,
                Enable = 1,
                Ignore = 2
            }
            
            public void Serialize(Writer writer) {
                writer.Write("linkID", linkID);
                writer.Write("mode", mode);
            }

            public void Deserialize(Reader reader) {
                reader.Read("linkID", ref linkID);
                reader.Read("mode", ref mode);
            }
        }

        public StorageElementFlags storageElementFlags { get; set; }
        
        public void Serialize(Writer writer) {
            writer.Write("ID", ID);
            writer.Write("storageElementFlags", storageElementFlags);
            writer.Write("defaultMode", defaultMode);
            writer.Write("panels", panels.Where(p => p.mode != defaultMode).ToArray());
            writer.Write("extensions", extensions.ToArray());
        }

        public void Deserialize(Reader reader) {
            ID = reader.Read<string>("ID");
            storageElementFlags = reader.Read<StorageElementFlags>("storageElementFlags");
            defaultMode = reader.Read<PanelInfo.Mode>("defaultMode");
            panels.Reuse(reader.ReadCollection<PanelInfo>("panels"));
            extensions.Reuse(reader.ReadCollection<PageExtension>("extensions"));
        }
    }
}