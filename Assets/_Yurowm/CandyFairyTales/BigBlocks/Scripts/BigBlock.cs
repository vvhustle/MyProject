using System;

namespace YMatchThree.Core {
    public abstract class BigBlock : Block, IBigSlotContent {
        public abstract BigShape GetBigShape();
    }
}