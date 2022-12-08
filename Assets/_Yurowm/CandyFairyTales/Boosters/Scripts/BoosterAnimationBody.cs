using System;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Spaces;

namespace YMatchThree.Core {
    [RequireComponent(typeof(ContentAnimator))]
    public class BoosterAnimationBody : SpaceObject, IReserved {
        public Action<string> callback;

        public void Callback(string callbackName) {
            callback?.Invoke(callbackName);
        }

        public void Rollout() {
            callback = null;
        }
    }
}