using System;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;

namespace Yurowm.Dialogues {
    public class Character : ContextedBehaviour, IReserved {
        
        [NonSerialized]
        public Transform parent;
        
        public string characterID;

        public Transform speachSource;

        public void Link(Transform parent) {
            this.parent = parent;
            transform.SetParent(parent);
            transform.Reset();
        }

        public void Rollout() {
            parent = null;
        }
    }
}