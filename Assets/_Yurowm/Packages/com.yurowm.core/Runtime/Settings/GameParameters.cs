using System;
using System.Collections.Generic;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm {
    public class GameParameters : IPropertyStorage {
        
        static GameParameters _Instance;
        static GameParameters Instance {
            get {
                if (_Instance == null)
                    _Instance = PropertyStorage.Load<GameParameters>();
                return _Instance;
            }
        }
        
        public string FileName => "GameParameters" + Serializator.FileExtension;
        public TextCatalog Catalog => TextCatalog.StreamingAssets;
        public bool Encrypted => true;
        
        List<Module> modules = null;
        Type[] moduleTypes = Utils
            .FindInheritorTypes<Module>(true)
            .Where(t => !t.IsAbstract) 
            .ToArray();
        
        public IEnumerable<Module> GetModules() {
            if (modules == null)
                modules = moduleTypes 
                    .Select(Activator.CreateInstance)
                    .Cast<Module>()
                    .ToList();
            
            foreach (var module in modules)
                yield return module;
        }
        
        public static M GetModule<M>() where M : Module {
            return Instance.GetModules().CastOne<M>();
        }
        
        public abstract class Module : ISerializable {
            public abstract string GetName();
            public abstract void Serialize(Writer writer);
            public abstract void Deserialize(Reader reader);
        }
        
        public void Serialize(Writer writer) {
            writer.Write("modules", modules.ToArray());
        }

        public void Deserialize(Reader reader) {
            if (modules == null)
                modules = new List<Module>();
            else 
                modules.Clear();
            modules.AddRange(reader.ReadCollection<Module>("modules"));
            foreach (var moduleType in moduleTypes)
                if (modules.All(m => !moduleType.IsInstanceOfType(m)))
                    modules.Add((Module) Activator.CreateInstance(moduleType));
        }
    }
    
    public class GameParametersGeneral : GameParameters.Module {
        public string privacyPolicyURL;
        public float maxDeltaTime = 1f / 30;

        public override string GetName() {
            return "General";
        }

        public override void Serialize(Writer writer) {
            writer.Write("privacyPolicy", privacyPolicyURL);
            writer.Write("maxDeltaTime", maxDeltaTime);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("privacyPolicy", ref privacyPolicyURL);
            reader.Read("maxDeltaTime", ref maxDeltaTime);
        }
    }
}