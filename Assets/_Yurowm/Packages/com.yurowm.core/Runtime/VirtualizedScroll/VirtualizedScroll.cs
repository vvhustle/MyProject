using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Utilities;
using Direction = UnityEngine.UI.Slider.Direction;

namespace Yurowm.UI {
    public class VirtualizedScroll : Behaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IUIRefresh {
        
        public float friction = 1f;
        
        IPrefabProvider _prefabProvider;
        
        public IPrefabProvider prefabProvider {
            get {
                if (_prefabProvider == null)
                    _prefabProvider = new AssetManagerPrefabProvider();
                return _prefabProvider;
            }
            set => _prefabProvider = value;
        }
        
        [Flags]
        public enum Options {
            AllowUserToScroll = 1 << 0,
            BreakPositionOnEnable = 1 << 1
        }
        
        public Direction orientation = Direction.TopToBottom;
        
        public Options options = Options.AllowUserToScroll;
        
        public float spacing = 10;
        public RectOffset padding;

        float position = 0;
        
        float totalSize = 0;
        
        List<ItemInfo> infos = new List<ItemInfo>();
        
        bool clamping => friction <= 0;

        void OnValidate() {
            RefreshDimensions();
        }
        
        void OnRectTransformDimensionsChange() {
            Build();
        }
        
        void OnEnable() {
            if (options.HasFlag(Options.BreakPositionOnEnable)) {
                position = 0;
                velocity = 0;
            }
            RefreshDimensions();
        }
        
        void RefreshDimensions() {
            if (!infos.IsEmpty()) {
                var position = 0f;
                infos.ForEach(i => {
                        i.position = position;
                        i.size = VectorToValue(i.GetBodyPrefab().size);
                        position += i.size + spacing;
                    });
                totalSize = position - spacing;
            }
            
            position = ClampPosition(position);
            Build();
        }

        public void SetList<I>(IEnumerable<I> list) where I : IVirtualizedScrollItem {
            var position = 0f;
            
            infos.ForEach(i => i.Hide());
            
            if (list == null) {
                infos.Clear();
            } else
                infos.Reuse(list
                    .Select(i => {
                        var info = new ItemInfo();
                        info.source = i;
                        info.list = this;
                        return info;
                    }));

            RefreshDimensions(); 
        }

        Vector2 canvasScale = new Vector2(1, 1);
        
        RectTransform canvasRect;
        
        public override void Initialize() {
            base.Initialize();
            canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }
        
        void Move(float delta) {
            position = position + delta;
            
            var clampPosition = ClampPosition(position);
            
            if (clamping)
                position = clampPosition;
            else { 
                if (position != clampPosition) 
                    velocity += (clampPosition - position) * 10 * Time.unscaledDeltaTime;
            }

            if (delta != 0) 
                Build();
        }
        
        float ClampPosition(float position) {
            return position.Clamp(0, totalSize - GetViewSize() + GetPaddingSize());
        }
        
        void Build() {
            if (!isActiveAndEnabled) 
                return;
            
            var startPadding = GetStartPadding();
            var itemRange = new FloatRange();
            var viewRange = new FloatRange(
                position,
                position + GetViewSize());
            
            foreach (var info in infos) {
                itemRange.Set(
                    info.position + startPadding,
                    info.position + startPadding + info.size);
                if (viewRange.Overlaps(itemRange)) {
                    Align(info.Show().rectTransform, itemRange);
                } else 
                    info.Hide();
            }
        }

        void Align(RectTransform rectTransform, FloatRange itemRange) {
            if (rectTransform.parent != transform) {
                rectTransform.SetParent(transform);
                rectTransform.Reset();
            }

            var position = this.position * GetDirectionSign();

            switch (orientation) {
                case Direction.TopToBottom: {
                    rectTransform.anchorMin = Vector2.up;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMax = new Vector2( 
                        -padding.right, 
                        position - itemRange.min);
                    rectTransform.offsetMin = new Vector2( 
                        padding.left, 
                        position - itemRange.max);
                    break;
                }
                case Direction.BottomToTop: {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.right;
                    rectTransform.offsetMax = new Vector2( 
                        -padding.right, 
                        position + itemRange.max);
                    rectTransform.offsetMin = new Vector2( 
                        padding.left, 
                        position + itemRange.min);
                    break;
                }
                case Direction.RightToLeft: {
                    rectTransform.anchorMin = Vector2.right;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMax = new Vector2( 
                        position - itemRange.min,
                        -padding.top);
                    rectTransform.offsetMin = new Vector2( 
                        position - itemRange.max,
                        padding.bottom);
                    break;
                }
                case Direction.LeftToRight: {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.up;
                    rectTransform.offsetMax = new Vector2( 
                        position + itemRange.max,
                        -padding.top);
                    rectTransform.offsetMin = new Vector2( 
                        position + itemRange.min,
                        padding.bottom);
                    break;
                }
                
            }
        }

        #region Orientation
        
        float GetDelta(PointerEventData eventData) {
            float delta = GetDirectionSign();
            switch (orientation) {
                case Direction.BottomToTop:
                case Direction.TopToBottom: delta *= eventData.delta.y * canvasScale.y; break;
                case Direction.LeftToRight:
                case Direction.RightToLeft: delta *= eventData.delta.x * canvasScale.x; break;
                default: return 0;
            }
            return delta;
        }
        
        float GetDirectionSign() {
            switch (orientation) {
                case Direction.TopToBottom:
                case Direction.RightToLeft: return 1;
                case Direction.BottomToTop:
                case Direction.LeftToRight: return -1;
            }
            return 0f;
        }
        
        float GetStartPadding() {
            switch (orientation) {
                case Direction.TopToBottom: return padding.top;
                case Direction.BottomToTop: return padding.bottom;
                case Direction.LeftToRight: return padding.left;
                case Direction.RightToLeft: return padding.right;
            }
            return 0f;
        }
        
        float GetViewSize() {
            switch (orientation) {
                case Direction.BottomToTop:
                case Direction.TopToBottom: return rectTransform.rect.height;
                case Direction.LeftToRight:
                case Direction.RightToLeft: return rectTransform.rect.width;
            }
            return 0f;
        }
        
        float GetPaddingSize() {
            switch (orientation) {
                case Direction.BottomToTop:
                case Direction.TopToBottom: return padding.vertical;
                case Direction.LeftToRight:
                case Direction.RightToLeft: return padding.horizontal;
            }
            return 0f;
        }
        
        float VectorToValue(Vector2 vector) {
            switch (orientation) {
                case Direction.BottomToTop:
                case Direction.TopToBottom: return vector.y;
                case Direction.LeftToRight:
                case Direction.RightToLeft: return vector.x;
            }
            return 0f;
        }
        
        #endregion
        
        #region Center

        public void CenterTo<SL>(Func<SL, bool> targetFilter, float duration = 0) where SL : IVirtualizedScrollItem {
            if (infos.IsEmpty()) 
                return;
            
            var info = infos
                .FirstOrDefault(i => i.source is SL sl && targetFilter.Invoke(sl));
            
            if (info == null)
                return;
            
            velocity = 0;
            
            var targetPosition = ClampPosition(info.position 
                                                + info.size / 2 
                                                - GetViewSize() / 2); 
            
            if (duration <= 0) 
                Build();
            else {
                centering = Centering(targetPosition, duration);                    
                centering.Run();
            }
        }
        
        public void CenterTo(IVirtualizedScrollItem target, float duration = 0) {
            CenterTo<IVirtualizedScrollItem>(i => i == target, duration);    
        }
        
        IEnumerator centering = null;
        
        IEnumerator Centering(float targetPosition, float duration) {
            if (duration <= 0)
                yield break;

            var start = position;
            
            velocity = 0;
            
            for (var t = 0f; t < 1 && isActiveAndEnabled && !drag; t += Time.unscaledDeltaTime / duration) {
                position = YMath.Lerp(start, targetPosition, t.Ease(EasingFunctions.Easing.InOutCubic));
                position = ClampPosition(position);
                Build();
                yield return null;
            }

            centering = null;
        }

        #endregion

        #region Dragging
        
        bool drag = false;
        float velocity = 0;
        
        public void OnBeginDrag(PointerEventData eventData) {
            if (options.HasFlag(Options.AllowUserToScroll) && GetViewSize() < totalSize) {
                canvasScale = canvasRect.rect.size.Scale(1f / Screen.width, 1f / Screen.height);
                drag = true;
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if (drag) 
                Move(GetDelta(eventData));
        }
        
        public void OnEndDrag(PointerEventData eventData) {
            if (!drag) return;
            
            drag = false;
                
            if (clamping) return;
            
            velocity = GetDelta(eventData) / Time.unscaledDeltaTime;
            Inertia().Run();
        }
        
        IEnumerator Inertia() {
            
            while (isActiveAndEnabled && !drag) {
                
                Move(velocity * Time.unscaledDeltaTime);
                
                if (ClampPosition(position) == position) {
                    velocity *= (1f - friction * Time.unscaledDeltaTime).Clamp01();
                    if (velocity.Abs() < 1) {
                        velocity = 0;
                        break;
                    }
                } 
                
                yield return null;
            }
        }
        
        #endregion

        class ItemInfo {
            public float position;
            public float size;
            public IVirtualizedScrollItem source;
            public VirtualizedScroll list;
            VirtualizedScrollItemBody prefab;
            VirtualizedScrollItemBody body;
            
            bool visible = false;

            public void Hide() {
                if (!visible) return; 
                
                body.Kill();
                body = null;
                visible = false;
            }
        
            public VirtualizedScrollItemBody Show() {
                if (!visible) {
                    body = list.prefabProvider.Emit(GetBodyPrefab());
                    source.SetupBody(body);
                    visible = true;
                }
            
                return body;
            }
            
            public VirtualizedScrollItemBody GetBodyPrefab() {
                if (!prefab)
                    prefab = list.prefabProvider.GetPrefab(source.GetBodyPrefabName());
                return prefab;
            }

            public void Refresh() {
                if (visible)
                    source.SetupBody(body);
            }
        }

        public void Refresh() {
            infos?.ForEach(i => i.Refresh());
        }
    }

    public interface IPrefabProvider {
        VirtualizedScrollItemBody GetPrefab(string name);
        
        VirtualizedScrollItemBody Emit(VirtualizedScrollItemBody item);
        
        void Remove(VirtualizedScrollItemBody item);
    }
    
    public interface IVirtualizedScrollItem {
        void SetupBody(VirtualizedScrollItemBody body);
        string GetBodyPrefabName();
    }
}
