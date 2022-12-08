using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace YMatchThree.Meta {
    [SerializeShort]
    public class LevelProgressModule : GameData.Module {
        
        Dictionary<string, World> worlds = new Dictionary<string, World>();
        
        public World GetWorldPropgress(string worldName, bool createNew = false) {
            if (worlds.TryGetValue(worldName, out var world))    
                return world;
            
            if (createNew) {
                world = new World();
                worlds.Add(worldName, world);
                return world;
            }
            
            return null;
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("worlds", worlds);
        }

        public override void Deserialize(Reader reader) {
            worlds = reader.ReadDictionary<World>("worlds").ToDictionary();
        }
        
        [SerializeShort]
        public class World : ISerializable {
            
            List<Level> levels = new List<Level>();

            public Level GetLevel(string levelID) {
                return levels.FirstOrDefault(l => l.ID == levelID);
            }
            
            public bool Complete(string levelID) {
                return GetBestScore(levelID) > 0; 
            }
            
            public int GetBestScore(string levelID) {
                var level = GetLevel(levelID);
                if (level != null)
                    return level.score;
                return 0;
            }

            public void SetBestScore(string levelID, int score) {
                var level = GetLevel(levelID);
                if (level == null) {
                    level = new Level {
                        ID = levelID
                    };
                    levels.Add(level);
                }
                
                level.score = score;
            }
            
            public void Serialize(Writer writer) {
                writer.Write("levels", levels.ToArray());    
            }

            public void Deserialize(Reader reader) {
                levels.Clear();
                levels.AddRange(reader.ReadCollection<Level>("levels"));
            }
        }
        
        [SerializeAs("LevelProgress")]
        public class Level : ISerializable {
            
            public string ID;
            public int score;
            
            public void Serialize(Writer writer) {
                writer.Write("ID", ID);
                writer.Write("score", score);
            }

            public void Deserialize(Reader reader) {
                reader.Read("ID", ref ID);
                reader.Read("score", ref score);
            }
        }
    }
}