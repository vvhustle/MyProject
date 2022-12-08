using System;

namespace Yurowm {
    public class Module {
        
        public Module() {}
        
        public GameEntity gameEntity {
            get;
            private set;
        }

        public static M Emit<M>(GameEntity gameEntity) where M : Module {
            var result = Activator.CreateInstance<M>();
            result.gameEntity = gameEntity;
            return result;
        }
    }
}