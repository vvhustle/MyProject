using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Colors;
using Yurowm.ContentManager;
using Yurowm.Extensions;

namespace Yurowm.Effects {
    public class RepaintEffectLogicProvider : IEffectLogicProvider {

        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.transform
                .AndAllChild()
                .Any(t => t.GetComponent<Repaint>() != null);
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (callbacks.All(c => !(c is Callback)))
                yield break;
            
            var callback = callbacks.CastOne<Callback>();                
            
            var palette = context.Get<ItemColorPalette>();
            
            effect.body.gameObject.Repaint(palette, callback.colorInfo);
            
            SetSpriteRepaint.Repaint(effect.body.gameObject, callback.colorInfo.ID);
        }
        
        public struct Callback : IEffectCallback {
            public ItemColorInfo colorInfo;
        }
    }
}