using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public class DestroyFieldNode : ActionNode {
        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            field?.Kill();
            
            Push(outputPort);
            
            yield break;
        }
    }
}