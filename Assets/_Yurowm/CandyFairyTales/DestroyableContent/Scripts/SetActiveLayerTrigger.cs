namespace YMatchThree.Core {
    public class SetActiveLayerTrigger : LayerTrigger {

        public bool less = true;
        public bool equal = true;
        public bool greater = false;
        
        public override void OnLayerBelow() {
            gameObject.SetActive(less);
        }

        public override void OnLayer() {
            gameObject.SetActive(equal);
        }

        public override void OnLayerAbove() {
            gameObject.SetActive(greater);
        }
    }
}