using System.Collections.Generic;
using System.Linq;
using Yurowm;
using Yurowm.Extensions;
using Behaviour = Yurowm.Behaviour;

namespace YMatchThree.Core {
    public class LevelGoalContainer : Behaviour {
        public LevelGoalCounter counter;

        List<LevelGoalCounter> counters = new List<LevelGoalCounter>();

        public LevelGoalCounter GetNewCounter() {
            var result = counters.FirstOrDefault(c => !c.gameObject.activeSelf);
            
            if (result == null) {
                result = Instantiate(counter.gameObject).GetComponent<LevelGoalCounter>();
                result.transform.SetParent(transform);
                result.transform.Reset();
                counters.Add(result);
            } 
            
            return result;
        }
    }
}