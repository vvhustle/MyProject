using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class BigSimpleBlock : BigBlock, INearHitDestroyable {
        
        BigShape shape = new RectBigShape(new int2(2, 2));
        
        public override BigShape GetBigShape() {
            return shape;
        }

        public IEnumerator Destroying() {
            yield break;
        }

        public int scoreReward { get; set; }
    }
}