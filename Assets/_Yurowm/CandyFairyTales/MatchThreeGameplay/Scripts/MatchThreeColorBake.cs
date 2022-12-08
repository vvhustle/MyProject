using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class MatchThreeColorBake : ColorBake {

        public override int[] ColorGeneration(int[] possibleColors, Dictionary<Side, ItemColorInfo> nears, GenerationType generation) {
            switch (generation) {
                case GenerationType.AllUnkonwn:
                case GenerationType.EvenFinal:
                    return possibleColors.Where(c => !nears.Any(x => x.Key.IsStraight() && x.Value.IsMatchWith(c))).ToArray();
                case GenerationType.OddSlots:
                case GenerationType.EvenSlots: {
                    List<int> result = new List<int>();
                    foreach (var colorID in possibleColors) {
                        if (nears.Values.All(x => !x.IsMatchWith(colorID)))
                            result.Add(colorID);
                        else {
                            bool add = true;
                            foreach (var near in nears.Where(x => x.Value.IsMatchWith(colorID))) {
                                if (near.Key.IsSlanted())
                                    continue;
                                Side mirrorSide = near.Key.Mirror();
                                if (nears.ContainsKey(mirrorSide) && nears[mirrorSide].IsMatchWith(colorID)) {
                                    add = false;
                                    break;
                                }
                            }
                            if (add)
                                result.Add(colorID);
                        }
                    }
                    return new[] { result.Count > 0 ? result.GetRandom(random) : possibleColors.GetRandom(random)};
                }
            }
            return null;
        }
    }
}