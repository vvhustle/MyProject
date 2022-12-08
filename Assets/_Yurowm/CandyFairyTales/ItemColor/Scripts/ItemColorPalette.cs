using System;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Colors;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class ItemColorPalette : LevelContent {
        
        public List<Color> colors = new List<Color>();

        public override Type BodyType => null;

        public Color Get(int colorID) {
            if (colorID >= 0 && colorID < colors.Count)
                return colors[colorID];
            return Color.white;
        }

        public override Type GetContentBaseType() {
            return typeof(ItemColorPalette);
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            
            writer.Write("color", colors.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            
            colors.Clear();
            colors.AddRange(reader.ReadCollection<Color>("color"));
        }
    }

    public static class ItemColorPaletteExtensions {
        public static void Repaint(this GameObject gameObject, ItemColorPalette colorPalette, ItemColorInfo colorInfo) {
            gameObject.Repaint(colorPalette.Get(colorInfo.ID));
        }
    }
}
