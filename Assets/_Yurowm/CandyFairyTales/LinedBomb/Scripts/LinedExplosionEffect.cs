using System;
using System.Collections.Generic;
using YMatchThree.Core;
using Yurowm.Extensions;

namespace Yurowm.Effects {
    public class LinedExplosionEffect : BaseBehaviour {
        public ContentAnimator rightWave;
        
        List<ContentAnimator> waves = new List<ContentAnimator>();

        public ContentAnimator GetWave(int index) {
            index = index.ClampMin(0);
            
            if (waves.IsEmpty())
                waves.Add(rightWave);
            
            ContentAnimator result;
            
            if (index >= waves.Count) {
                result = Instantiate(rightWave.gameObject).GetComponent<ContentAnimator>();
                result.transform.SetParent(rightWave.transform.parent);
                waves.Add(result);
            } else
                result = waves[index];
            
            result.transform.Reset();
            result.gameObject.SetActive(false);
            return result;
        }
        
        public void Clear() {
            waves.ForEach(w => w.gameObject.SetActive(false));
        }
    }
}