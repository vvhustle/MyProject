using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class LazyColorBake : ColorBake {
        public override int[] ColorGeneration(int[] possibleColors, Dictionary<Side, ItemColorInfo> nears, GenerationType generation) {
            switch (generation) {
                case GenerationType.AllUnkonwn:
                    return possibleColors.Where(c => !nears.Any(x => x.Key.IsStraight() && x.Value.IsMatchWith(c))).ToArray();
                case GenerationType.EvenFinal:
                    return null;
                case GenerationType.OddSlots:
                case GenerationType.EvenSlots: {
                    if (possibleColors.Length > 0)
                        return new[] { possibleColors.GetRandom(random) };
                    
                    var rnd = nears.Values.Distinct().GetRandom(random);
                    
                    if (rnd.type == ItemColor.Known)
                        return new [] {rnd.ID};
                    
                    return null;
                }
            }
            return null;
        }
    }
}