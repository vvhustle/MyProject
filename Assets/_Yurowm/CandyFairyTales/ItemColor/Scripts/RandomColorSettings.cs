using System.Linq;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class RandomColorSettings : FixedSizeColorSettings, IDefault {
        public override void Initialize() {
            count = Mathf.Min(count, colorPalette.colors.Count, ItemColorInfo.IDs.Length);
            YRandom random = YRandom.main.NewRandom();
            int[] shuffled = ItemColorInfo.IDs
                .Take(colorPalette.colors.Count)
                .Shuffle(random)
                .Take(count)
                .ToArray();
            
            dictionary.Clear();
            for (int index = 0; index < count; index++)
                dictionary.Add(ItemColorInfo.IDs[index], shuffled[index]);
        }

        public bool isDefault { get; set; } = true;
    }
}