using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.HierarchyLists;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    public class LevelEditorFolders : ISerializable {
        
        public List<World> collection = new List<World>();
        readonly string path;
        
        public LevelEditorFolders() {}
        
        public LevelEditorFolders(string path) : base() {
            this.path = path;
            if (File.Exists(path))
                Serializator.FromTextData(this, File.ReadAllText(path));
        }

        public void Save(string name, IEnumerable<string> folders) {
            var world = collection.FirstOrDefault(w => w.name == name);
            
            if (world == null) {
                world = new World();
                world.name = name;
                collection.Add(world);
            }
            
            world.folders = folders.ToArray();
            
            Save();
        }
        
        void Save() {
            if (!path.IsNullOrEmpty())
                File.WriteAllText(path, Serializator.ToTextData(this));
        }

        public IEnumerable<string> Load(string name) {
            var world = collection.FirstOrDefault(w => w.name == name);
            
            if (world != null)
                foreach (var folder in world.folders)
                    yield return folder;
        }

        public void RenameWorld(string oldName, string newName) {
            var world = collection.FirstOrDefault(w => w.name == oldName);

            if (world == null) return;
            
            world.name = newName;
            Save();
        }
        
        public void Serialize(Writer writer) {
            writer.Write("collection", collection.ToArray());
        }

        public void Deserialize(Reader reader) {
            collection.Reuse(reader.ReadCollection<World>("collection"));
        }
        
        public class World : ISerializable {
            public string name;
            public string[] folders;
            
            public void Serialize(Writer writer) {
                writer.Write("name", name);
                writer.Write("folders", folders);
            }

            public void Deserialize(Reader reader) {
                reader.Read("name", ref name);
                folders = reader.ReadCollection<string>("folders").ToArray();
            }
        }
    }
}