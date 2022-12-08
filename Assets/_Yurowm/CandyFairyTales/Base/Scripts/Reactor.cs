using System;
using System.Collections;

namespace YMatchThree.Core {
    public class Reactor {
        public Reaction reaction;

        Func<IEnumerator> provider;

        IEnumerator _logic;
        public IEnumerator logic => _logic;
        
        public enum Result { CompleteToGravity, GravityToRepeate, CompleteToContinue }

        public Reactor(Func<IEnumerator> provider, Reaction reaction = null) {
            this.reaction = reaction;
            this.provider = provider;
            _logic = provider.Invoke();
        }

        public void Reset() {
            _logic = provider.Invoke();
        }
    }
}