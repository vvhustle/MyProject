using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    [SerializeShort]
    public class SlotInfo : ISerializable {
        public int2 coordinate;
        
        public SlotInfo() {
            content = ContentBaseTypes.ToDictionary(t => t, t => (ContentInfoContainer) null);
        }
        
        public SlotInfo(int2 coordinate) : this() {
            this.coordinate = coordinate;
        }

        #region Content
        
        public static readonly Type[] ContentBaseTypes = Utils.FindInheritorTypes<SlotContent>(true, false)
            .Where(t => t.GetAttribute<BaseContentOrderAttribute>() != null)
            .Select(t => (t, t.GetAttribute<BaseContentOrderAttribute>().order))
            .OrderBy(c => c.order)
            .Select(c => c.t)
            .ToArray();
        
        Dictionary<Type, ContentInfoContainer> content;

        public IEnumerable<ContentInfo> Content() {
            foreach (ContentInfoContainer container in content.Values)
                if (container != null)
                    foreach (ContentInfo info in container)
                        yield return info;
        }
        
        public void AddContent(ContentInfo ContentInfo) {
            if (!(ContentInfo.Reference is SlotContent slotContent)) return;
            
            Type baseType = ContentInfo.BaseType;
            
            if (content == null)
                content = ContentBaseTypes.ToDictionary(t => t, t => (ContentInfoContainer) null);
            
            var container = content.Get(baseType);
            
            if (container == null) {
                if (slotContent.IsUniqueContent())
                    container = new SingleContentContainer(baseType);
                else 
                    container = new MultipleContentContainer(baseType);
                content[baseType] = container;
            }
            
            container.Add(ContentInfo);
        }
        
        public void AddContent(SlotContent slotContent) {
            AddContent(new ContentInfo(slotContent));
        }

        public void RemoveContent(ContentInfo info) {
            Type baseType = info.BaseType;

            var container = content.Get(baseType);

            container?.Remove(info);
        }

        public void RemoveContent(Func<ContentInfo, bool> filter) {
            foreach (var container in content.Values)
                container?.Remove(filter);
        }

        public ContentInfo GetContent<SC>() where SC : SlotContent {
            return Content().FirstOrDefault(c => c.Reference is SC);
        }
        
        public ContentInfo GetContent(SlotContent slotContent) {
            Type baseType = slotContent.GetContentBaseType();

            var container = content.Get(baseType);

            return container?.FirstOrDefault(c => c.Reference.ID == slotContent.ID);
        }

        public ContentInfo GetContent(Func<ContentInfo, bool> filter) {
            return Content().FirstOrDefault(filter.Invoke);
        }

        public bool HasContent() {
            return Content().Any();
        }
        
        public bool HasContent(SlotContent slotContent) {
            return Content().Any(c => c.ID == slotContent.ID);
        }
        
        public bool HasBaseContent(SlotContent slotContent) {
            return HasBaseContent(slotContent.GetContentBaseType());
        }
        
        public bool HasBaseContent<T>() {
            return HasBaseContent(typeof(T));
        }
        
        public bool HasBaseContent(Type baseType) {
            if (content.TryGetValue(baseType, out var container))
                if (container != null && container.Any())
                    return true;
            return false;
        }
        #endregion

        #region ISeializable
        public void Serialize(Writer writer) {
            writer.Write("coordinate", coordinate);
            writer.Write("content", Content().ToArray());
        }
    
        public void Deserialize(Reader reader) {
            reader.Read("coordinate", ref coordinate);
            content = ContentBaseTypes.ToDictionary(t => t, t => (ContentInfoContainer) null);
            foreach (var ContentInfo in reader.ReadCollection<ContentInfo>("content"))
                if (ContentInfo.Reference)
                    AddContent(ContentInfo);  
        }
        #endregion
        
        #region Container
        abstract class ContentInfoContainer : IEnumerable<ContentInfo> {
            public Type BaseType { get; private set; }
        
            public ContentInfoContainer(Type baseType) {
                BaseType = baseType;
            }

            public abstract void Add(ContentInfo info); 
            public abstract void Remove(ContentInfo info); 
            public abstract void Remove(Func<ContentInfo, bool> filter); 
            
            public abstract IEnumerator<ContentInfo> GetEnumerator();
        
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        class SingleContentContainer : ContentInfoContainer {
            public SingleContentContainer(Type baseType) : base(baseType) {}

            ContentInfo content;
            public override void Add(ContentInfo info) {
                if (info.Reference.GetContentBaseType() == BaseType)
                    content = info;
            }

            public override void Remove(ContentInfo info) {
                if (info == content)
                    content = null;
            }

            public override void Remove(Func<ContentInfo, bool> filter) {
                if (content != null && filter.Invoke(content))
                    content = null;
            }

            public override IEnumerator<ContentInfo> GetEnumerator() {
                if (content != null)
                    yield return content;
            }
        }
        
        class MultipleContentContainer : ContentInfoContainer {
            public MultipleContentContainer(Type baseType) : base(baseType) {}

            List<ContentInfo> content = new List<ContentInfo>();
            public override void Add(ContentInfo info) {
                if (info.Reference.GetContentBaseType() == BaseType
                    && !content.Contains(info))
                    content.Add(info);
            }

            public override void Remove(ContentInfo info) {
                content.Remove(info);
            }

            public override void Remove(Func<ContentInfo, bool> filter) {
                content.RemoveAll(filter.Invoke);
            }

            public override IEnumerator<ContentInfo> GetEnumerator() {
                foreach (ContentInfo info in content)
                    yield return info;
            }
        }
        #endregion
        
        public ContentInfo GetCurrentContent() {
            return Content().FirstOrDefault();
        }

        public ItemColorInfo GetCurrentColor() {
            var content = GetCurrentContent();
            
            if (content?.Reference is IColored) {
                var variable = content.GetVariable<ColoredVariable>();
                if (variable == null) return ItemColorInfo.None;
                return variable.info;
            }
            
            return ItemColorInfo.None;
        }

        public void SetCurrentColor(ItemColorInfo colorInfo) {
            var variable = GetCurrentContent()?.GetVariable<ColoredVariable>();
                
            if (variable != null)
                variable.info = colorInfo;
        }

        public void SetCurrentColorID(int colorID) {
            var variable = GetCurrentContent()?.GetVariable<ColoredVariable>();
                
            if (variable != null)
                variable.info = ItemColorInfo.ByID(colorID);
        }
    }
    
    [Flags]
    public enum SlotFlags {
        Default = BuildOnStart,
        BuildOnStart = 1 << 0,
        BuildByTag = 1 << 1
    }
}