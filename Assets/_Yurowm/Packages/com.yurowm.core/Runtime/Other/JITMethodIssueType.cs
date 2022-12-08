using System;
using System.Collections.Generic;
using System.Reflection;

namespace Yurowm {
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct)]
    public class JITMethodIssueTypeAttribute : Attribute {}
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class JITMethodIssueTypeProvider : Attribute {        
        MethodInfo method = null;
        public void SetMethod(MethodInfo method) {
            this.method = method;
        }

        public IEnumerable<Type> GetTypes() {
            var result = method.Invoke(null, new object[0]) as IEnumerator<Type>;
            while (result.MoveNext())
                yield return result.Current;
        }
    }
}