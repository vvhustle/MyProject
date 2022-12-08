namespace YMatchThree.Core {
    public class FastForwardSimulation : PuzzleSimulation {
        public override bool AllowSounds() => false;

        public override bool AllowAnimations() => false;

        public override bool AllowEffects() => false;
        
        public override bool AllowToWait() => false;
        
        public override bool AllowBodies() => false;
    }
}