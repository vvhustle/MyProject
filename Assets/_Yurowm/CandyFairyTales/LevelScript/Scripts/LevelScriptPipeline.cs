using System.Collections;
using System.Linq;
using UnityEngine;
using YMatchThree.UI;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.UI;
using Behaviour = Yurowm.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class LevelScriptPipeline : GameEntity {
        
        public LevelStars stars = new LevelStars();
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            var events = new LevelScriptEvents();
            
            space.context.SetArgument(events);
            
            var score = new Score {
                stars = stars
            };
            
            ScoreAnimation(score).Run(space.coroutine);

            space.AddItem(score);
        }
        
        IEnumerator ScoreAnimation(Score score) {
            var scorebars = Behaviour.GetAllByID<ScoreBar>("Field.ScoreBar").ToArray();
            var scoreCounters = Behaviour.GetAllByID<LabelFormat>("Field.Score").ToArray();
            var starNotches = Behaviour.GetAllByID<StarNotch>("Field.StarNotch").ToArray();
            
            starNotches.ForEach(n => n.Place(stars));
            
            void ShowScore(int value) {
                scoreCounters.ForEach(s => s["value"] = value.ToString());
                var fill = 1f * value / stars.Third;
                scorebars.ForEach(b => b.SetFillValue(fill));
                starNotches.ForEach(n => n.OnChangeScore(value));
            }
            
            int currentScore = score.value;
            
            ShowScore(currentScore);
            
            while (true) {
                
                if (currentScore != score.value) {
                    while (currentScore != score.value) {
                        currentScore = Mathf.RoundToInt(Mathf.MoveTowards(
                            currentScore, 
                            score.value,
                            (score.value - currentScore) / 20 + 1));
                        ShowScore(currentScore);
                        yield return null;
                    }
                }
                
                yield return null;
            }
        }
    }
}