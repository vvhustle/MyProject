namespace YMatchThree.Core {
    public class OnFieldStartNode : FieldEventNode {
        public override void RegisterEvent(Field field) {
            field.events.onLevelStart += () => Push(outputPort, field);
        }
    }
}