using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm.ContentManager;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace Yurowm.Effects {
    public class EffectBody : SpaceObject, IReserved {
        
        static readonly IEffectLogicProvider[] allLogicProviders = Utils
            .FindInheritorTypes<IEffectLogicProvider>(true)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(Activator.CreateInstance)
            .Cast<IEffectLogicProvider>()
            .ToArray();
        
        IEffectLogicProvider[] logicProviders;
        
        void Awake() {
            logicProviders = allLogicProviders
                .Where(p => p.IsSuitable(this))
                .ToArray();
        }

        public IEnumerable<IEffectLogicProvider> GetLogicProviders() {
            foreach (var provider in logicProviders) 
                yield return provider;
        }

        public void Rollout() { }
    }
}