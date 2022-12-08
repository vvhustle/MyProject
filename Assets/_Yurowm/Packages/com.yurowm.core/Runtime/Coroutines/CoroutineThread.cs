using System.Collections;
using System.Collections.Generic;
using Routine = Yurowm.Coroutines.CoroutineCore.Routine;

namespace Yurowm.Coroutines {
    public class CoroutineThread : IEnumerator {
        
        public readonly string name;
        
        Queue<Routine> queue = new Queue<Routine>();
        Routine current = null;
        
        public CoroutineThread(string name = "Untitled") {
            this.name = name;
        }

        public void AddInQueue(IEnumerator logic, CoroutineOptions options = CoroutineOptions.Run) {
            queue.Enqueue(new Routine(logic, options, CoroutineCore.Loop.Update));
        }

        public void Clear() {
            current = null;
            queue.Clear();
        }
        
        public int Count() {
            return queue.Count;
        }
        
        public bool MoveNext() {
            if (current != null) {
                current.Update();
                if (current.IsComplete())    
                    current = null;
                return true;
            }
            
            if (queue.Count > 0) {
                current = queue.Dequeue();
                return MoveNext();
            }
            
            return true;   
        }

        public object Current => null;

        public void Reset() {}
    }
}