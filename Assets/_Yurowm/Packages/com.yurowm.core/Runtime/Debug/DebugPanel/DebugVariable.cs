using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yurowm.DebugTools {
    
    public class DebugVariableMessage : DebugPanel.IMessage {
        public DebugVariable variable;

        public int CastPriority => 1;
        
        public DebugVariableMessage() {}
        
        DebugVariableMessage(DebugVariable variable) {
            this.variable = variable;
        }
        
        public DebugPanel.IMessage TryToEmitFor(object value) {
            if (value is DebugVariable variable)
                return new DebugVariableMessage(variable);
            return null;
        }

        public bool Update(object obj) {
            return variable == obj;
        }

        public bool IsExtendable() {
            return false;
        }
    }
    public abstract class DebugVariable {
        public abstract Type GetVariableType();
    }
    
    public class DebugVariable<T> : DebugVariable {
        readonly Action<T> setter;
        readonly Func<T> getter;
        
        public DebugVariable(Func<T> getter, Action<T> setter) {
            this.setter = setter;
            this.getter = getter;
        }
        
        public T Get() {
            if (getter != null)
                return getter.Invoke();
            return default(T);
        }

        public void Set(T value) {
            setter?.Invoke(value);
        }

        public override Type GetVariableType() {
            return typeof(T);
        }
    }
    
    public class DebugVariableRange<T> : DebugVariable<T> where T : IComparable {
        public T min;
        public T max;
        
        public DebugVariableRange(Func<T> getter, Action<T> setter, T min, T max) :
            base(getter, setter) {
            
            if (max.CompareTo(min) > 0) {
                this.min = min;
                this.max = max;
            } else {
                this.min = max;
                this.max = min;
            }
        }
        
    }
}