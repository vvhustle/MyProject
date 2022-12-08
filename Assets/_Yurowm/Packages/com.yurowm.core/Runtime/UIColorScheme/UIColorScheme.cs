using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.Colors {
    public class UIColorScheme : ISerializableID {
        
        [PreloadStorage]
        public static Storage<UIColorScheme> storage = new Storage<UIColorScheme>("UIColor", TextCatalog.StreamingAssets, false);
        public static UIColorScheme current = new UIColorScheme();
        
        public string ID {get; set;}
        
        public Dictionary<string, ColorEntry> colors = new Dictionary<string, ColorEntry>();
        
        public bool GetColor(string key, out Color color) {
            if (colors.TryGetValue(key, out var entry)) {
                color = entry.color;
                return true;
            }
                
            
            color = default;
            return false;
        }
                
        public Color GetColor(string key) {
            return GetColor(key, out var result) ? result : Color.white;
        }
        
        public void SetColor(string key, Color color) {
            colors[key] = new ColorEntry {
                key = key,
                color = color
            };
        }

        public void Apply(UIColorScheme scheme) {
            foreach (var entry in scheme.colors.Values) 
                SetColor(entry.key, entry.color);
        }
        
        public struct ColorEntry {
            public Color color;
            public string key;
        }

        public void Serialize(Writer writer) {
            writer.Write("ID", ID);
            writer.Write("colors", colors.Values
                .Where(p => !p.key.IsNullOrEmpty())
                .GroupBy(p => p.key)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().color));
        }

        public void Deserialize(Reader reader) {
            ID = reader.Read<string>("ID");
            colors = reader.ReadDictionary<Color>("colors")
                .ToDictionary(p => p.Key, p => new ColorEntry {
                    key = p.Key,
                    color = p.Value
                });
        }
    }
    
    
    public static class UIColorSchemeExtensions {
        public static void Repaint(this GameObject gameObject, UIColorScheme scheme) {
            foreach (var component in gameObject.GetComponentsInChildren<ColorSchemeRepaintTag>(true)) {
                if (component.global)
                    component.Refresh();
                else
                    component.Refresh(scheme);
            }
        }
        
        public static string ColorizeUI(this string text, string uiColor) {
            if (UIColorScheme.current.GetColor(uiColor, out var color))
                return text.Colorize(color);
            return text;
        }
    }
}