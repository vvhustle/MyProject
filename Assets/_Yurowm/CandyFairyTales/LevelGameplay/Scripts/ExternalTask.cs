using System;
using System.Collections;

namespace YMatchThree.Core {
    public class ExternalTask : LevelGameplay.Task, IDisposable {
        enum State {
            None,
            Started,
            Complete
        }
        
        State state = State.None;

        public override IEnumerator Logic() {
            state = State.Started;
            
            while (state != State.Complete)
                yield return null;
            
            state = State.None;
        }

        public void Dispose() {
            state = State.Complete;
        }

        public IEnumerator WaitAccess() {
            while (state != State.Started)
                yield return null;
        }
    }
}