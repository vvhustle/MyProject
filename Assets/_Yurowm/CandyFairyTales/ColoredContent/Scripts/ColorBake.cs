using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class ColorBake {
        
        protected YRandom random;
        
        public enum GenerationType {
            AllUnkonwn, 
            EvenSlots, 
            OddSlots, 
            EvenFinal
        }
        
        public void Bake(LiveContext context) {
            var script = context.GetArgument<LevelScriptBase>();
            var level = context.GetArgument<Level>();
            
            random = new YRandom(level.randomSeed);

            var colorSettings = context.GetArgument<LevelColorSettings>();
            
            int[] possibleColorIDs = colorSettings.dictionary.Keys.Take(script.colorSettings.Count).ToArray();

            var slots = level.slots.ToDictionaryKey(x => x.coordinate);
            var colorSets = new Dictionary<int2, int[]>();

            // Generating first color sets for all current items with unknown color
            foreach (var slot in slots) {
                if (slot.Value.GetCurrentColor().type != ItemColor.KnownRandom)
                    continue;

                var nears = Sides.all.ToDictionaryValue(side => slots.Get(slot.Key + side))
                    .Where(x => x.Value != null && x.Value.GetCurrentColor().IsMatchableColor())
                    .ToDictionary(x => x.Key, x => x.Value.GetCurrentColor());

                colorSets.Add(slot.Key, ColorGeneration(possibleColorIDs, nears, GenerationType.AllUnkonwn));
            }

            // f: 0 - even slots (blind generation - all content items has unknown colors)
            // f: 1 - odd slots (colors generated in neighbors slots context)
            // f: 2 - even slots (final generation - repeats f(0) step, because it was blind generation)
            for (int f = 0; f <= 2; f++)
                foreach (var slot in colorSets) {
                    if ((slot.Key.X + slot.Key.Y) % 2 == f % 2) continue;

                    GenerationType type = (slot.Key.X + slot.Key.Y) % 2 == 0 ? GenerationType.EvenSlots : GenerationType.OddSlots;

                    var nears = Sides.all.ToDictionaryValue(side => slots.Get(slot.Key + side))
                        .Where(x => x.Value != null && x.Value.GetCurrentColor().IsMatchableColor())
                        .ToDictionary(x => x.Key, x => x.Value.GetCurrentColor());

                    int[] colors;
                    if (f == 2) 
                        colors = ColorGeneration(possibleColorIDs, nears, GenerationType.EvenFinal);
                    else 
                        colors = slot.Value;
                    
                    slots[slot.Key].SetCurrentColorID(colors != null && colors.Length > 0 ? 
                        ColorGeneration(colors, nears, type)[0] : possibleColorIDs.GetRandom(random));
                }

            // now generates random colors for all non-current content items.
            foreach (var slot in level.slots)
                slot.Content()
                    .Select(c => c.GetVariable<ColoredVariable>())
                    .NotNull()
                    .Where(v => v.info.type == ItemColor.KnownRandom)
                    .ForEach(v => v.info = ItemColorInfo.ByID(possibleColorIDs.GetRandom(random)));

            // Applying color mask
            foreach (var slot in level.slots)
                slot.Content()
                    .Select(c => c.GetVariable<ColoredVariable>())
                    .NotNull()
                    .Where(v => v.info.type == ItemColor.Known)
                    .ForEach(v => 
                        v.info = ItemColorInfo.ByID(colorSettings.Convert(v.info.ID)));
        }

        public abstract int[] ColorGeneration(int[] possibleColors, Dictionary<Side,ItemColorInfo> nears, GenerationType generation);
    }
}