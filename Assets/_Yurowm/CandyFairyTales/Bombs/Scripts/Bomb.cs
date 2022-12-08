using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class Bomb : BombChipBase, IColored {

        int scorePoints;

        public BombHit hit = new BombHit();
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            scorePoints = scoreReward;
            scoreReward = 0;
        }

        public override IEnumerator Exploding() {
            
            hit.Initialize(slotModule, context);
            hit.Prepare();

            yield return hit.Logic();  
            
            scorePoints += hit.scorePoints;
            
            if (scorePoints > 0) {
                field.context.Get<Score>().AddScore(scorePoints);
                
                slotModule
                    .Slot()
                    .ShowScoreEffect(scorePoints, colorInfo);
            }
            
            BreakParent();
            
            yield return lcAnimator.PlayClipAndWait("Destroying");
            
            if (simulation.AllowToWait()) 
                yield return time.Wait(.1f);  
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("hit", hit);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader); 
            reader.Read("hit", ref hit);
        }

        #endregion
    }
}