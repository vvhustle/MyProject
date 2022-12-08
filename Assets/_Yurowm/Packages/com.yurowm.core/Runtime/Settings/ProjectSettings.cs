using Yurowm.Serialization;

namespace Yurowm.Core {
    public class ProjectSettings : IPropertyStorage {
        
        public string FileName => "ProjectSettings" + Serializator.FileExtension;
        public TextCatalog Catalog => TextCatalog.StreamingAssets;
        public bool Encrypted => true;
        
        public string appStoreAppID;
        
        public string versionName;
        
        public string Version => $"{versionName}.{buildCode}";

        public int buildCode;
        
        public string supportEmail;

        public void Serialize(Writer writer) {
            writer.Write("buildCode", buildCode);
            writer.Write("versionName", versionName);
            writer.Write("appStoreAppID", appStoreAppID);
            writer.Write("supportEmail", supportEmail);
        }

        public void Deserialize(Reader reader) {
            reader.Read("buildCode", ref buildCode);
            reader.Read("versionName", ref versionName);
            reader.Read("appStoreAppID", ref appStoreAppID);
            reader.Read("supportEmail", ref supportEmail);
        }
    }
}