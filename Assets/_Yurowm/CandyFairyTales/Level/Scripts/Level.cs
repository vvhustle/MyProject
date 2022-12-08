using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Nodes;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class Level : ISerializable {

        #region Measurements
        
        public int2 size = new int2(defaultSize, defaultSize);
        
        public int width {
            get => size.X;
            set => size.X = value;
        }
        public int height {
            get => size.Y;
            set => size.Y = value;
        }
        
        public area Area => new area(int2.zero, size);
        
        public const int minSize = 1;
        public const int defaultSize = 6;
        public const int maxSize = 12;
        
        #endregion
        
        public string gamePlay = "";
        public string physic = "";
        
        public long randomSeed;

        public List<SlotLayerBase> layers = new List<SlotLayerBase>();
        public List<SlotInfo> slots = new List<SlotInfo>();
        public List<ContentInfo> extensions = new List<ContentInfo>();
        
        #region ISerializable
        
        public virtual void Serialize(Writer writer) {
            writer.Write("size", size);
            writer.Write("gamePlay", gamePlay);
            writer.Write("physic", physic);
            writer.Write("seed", randomSeed);

            writer.Write("layers", layers);
            
            var fieldArea = Area;
            
            writer.Write("slots", slots
                .Where(s => fieldArea.Contains(s.coordinate))
                .DistinctBy(s => s.coordinate)
                .ToArray());
                
            writer.Write("extensions", extensions.ToArray());
        }

        public virtual void Deserialize(Reader reader) {
            reader.Read("size", ref size);
            reader.Read("gamePlay", ref gamePlay);
            reader.Read("physic", ref physic);
            reader.Read("seed", ref randomSeed);
            
            slots = reader.ReadCollection<SlotInfo>("slots").ToList();
            extensions = reader.ReadCollection<ContentInfo>("extensions").ToList();
            layers = reader.ReadCollection<SlotLayerBase>("layers").ToList();
            layers.ForEach(l => l.Initialize(this));
        }

        #endregion
    }
    
    public class LevelWorld {
        public static readonly List<LevelWorld> all = new List<LevelWorld>();
        public List<LevelScriptOrdered> levels = new List<LevelScriptOrdered>();

        public readonly string name;
        
        public LevelWorld(string name) {
            this.name = name;
        }

        public LevelScriptBase GetLevel(int number) {
            if (number <= 0 || number >= levels.Count) return null;
            return levels[number - 1];
        }
    }
}