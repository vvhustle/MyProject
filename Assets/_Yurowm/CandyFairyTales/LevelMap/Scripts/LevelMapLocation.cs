using System.Collections.Generic;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Spaces;

namespace YMatchThree.Seasons {
    public class LevelMapLocation : SpacePhysicalItem {
        LevelMapLocationBody _body;

        public float yPosition => position.y;
        
        public float TopPosition => yPosition + TopEdge;
        public float BottomPosition => yPosition + BottomEdge;
        public float TopVisible => yPosition + _body.VisibleEdges.Max;
        public float BottomVisible => yPosition + _body.VisibleEdges.Min;
        public float TopEdge => _body.Edges.Max;
        public float BottomEdge => _body.Edges.Min;
        
        public LevelMapPointsProvider pointsProvider => _body.pointsProvider;
        
        public List<LevelButton> buttons = new List<LevelButton>();

        public override SpaceObject EmitBody() {
            _body = base.EmitBody() as LevelMapLocationBody;
            return _body;
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            body?
                .GetComponentsInChildren<ILevelMapLocationComponent>(true)
                .ForEach(s => s.Initialize(context));
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            buttons.ForEach(b => b.Kill());
        }
    }
    
    public interface ILevelMapLocationComponent {
        void Initialize(LiveContext context);
    }
}