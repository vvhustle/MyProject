using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Colors;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace Yurowm.Effects {
    public class SlotHighlighterLogicProvider : IEffectLogicProvider {
        
        static readonly int _MainTex = Shader.PropertyToID("_MainTex");
        static readonly int _TintColor = Shader.PropertyToID("_TintColor");
        
        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<SlotHighlighterEffectTag>();
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (!effect.body.SetupChildComponent(out SlotRenderer slotRenderer)) 
                yield break;
            
            var callback = callbacks.CastOne<Callback>();
            
            if (callback.center == null)
                yield break;
            
            var field = callback.center.field;
            
            var highlighted = field.fieldContext.GetArgument<Highlighted>();
            
            if (highlighted == null) {
                highlighted = new Highlighted();
                field.fieldContext.SetArgument(highlighted);
            }

            var slotsToHighlight = callback.slots
                .Where(s => !highlighted.slots.Contains(s))
                .ToArray();
            
            highlighted.slots.AddRange(slotsToHighlight);
            
            var palette = field.fieldContext.Get<ItemColorPalette>();
            
            var coordiantes = callback.slots.Select(s => s.coordinate - callback.center.coordinate).ToArray();
            
            var colorInfo = callback.colorInfo;
            if (!colorInfo.IsKnown())
                colorInfo = callback.center.GetCurrentColor();
            
            effect.body.gameObject.Repaint(palette.Get(colorInfo.ID));

            slotRenderer.RebuildImmediate(coordiantes);

            var material = slotRenderer.InstanceMaterial;
            var tintColor = Color.white;

            var time = context.GetArgument<SpaceTime>();

            using (MatchingPool.Get(context).Use()) {
                for (float t = 0; t < 1; t += time.Delta / 0.4f) {

                    var scale = 1f / YMath.Lerp(.01f, 2f, t);

                    material.SetTextureScale(_MainTex, new Vector2(scale, scale));

                    tintColor.a = EasingFunctions.OutCubic(1f - t);
                    material.SetColor(_TintColor, tintColor);

                    yield return null;
                }
            }

            slotRenderer.Clear();
            slotsToHighlight.ForEach(s => highlighted.slots.Remove(s));
        }

        class Highlighted {
            public List<Slot> slots = new List<Slot>();
        }

        public struct Callback : IEffectCallback {
            public Slot[] slots;
            public Slot center;
            public ItemColorInfo colorInfo;
        }
    }
}
