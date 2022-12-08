using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class BigShape {
        public abstract IEnumerable<int2> GetCoords();
    }

    public class RectBigShape : BigShape {
        int2 size;
        
        public RectBigShape(int2 size) {
            this.size = size;
        }
        
        public override IEnumerable<int2> GetCoords() {
            for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
                yield return new int2(x, y);
        }
    }
    
}