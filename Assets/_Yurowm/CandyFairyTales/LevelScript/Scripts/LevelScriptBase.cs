using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Nodes;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class LevelScriptBase : NodeSystem {
        
        public static readonly string fileNameFormat = "level_{0}" + Serializator.FileExtension;
        
        public bool preloadMode = false;
        
        public LevelBackground.Info background;
        
        public LevelStars stars = new LevelStars();
        
        public List<ContentInfo> extensions = new List<ContentInfo>();
        
        public LevelColorSettings colorSettings;
        
        public string ID;
        
        public string name = "";
        
        public string chipBody;

        #region Localization

        public string localizationKey = "";
        
        public virtual string LocalizationPath => localizationKey;
        
        public virtual bool IsLocalized() {
            return !localizationKey.IsNullOrEmpty();
        }

        #endregion
        
        #region Preview & Loading

        bool preview;
        string raw;

        public bool IsPreview => preview;

        public void MarkAsPreview() {
            preview = true;
        }
        
        public static S Load<S>(string raw, bool preview) where S : LevelScriptBase {
            return Serializator.FromTextData<S>(raw, p => {
                p.preview = preview;
                p.raw = raw;
            });
        }
        
        public IEnumerator LoadCompletely() {
            if (!preview) 
                yield break;
            
            if (raw.IsNullOrEmpty())
                yield return TextData.LoadTextProcess(Path.Combine(GetType().Name, $"level_{ID}{Serializator.FileExtension}"),
                    r => raw = r);

            if (raw.IsNullOrEmpty())
                throw new NullReferenceException("The design doesn't have raw data for loading");
            
            preview = false;
            
            Serializator.FromTextData(this, raw);
            
            raw = null;
        }

        #endregion

        public override IEnumerable<Type> GetSupportedNodeTypes() {
            yield return typeof (LevelScriptNode);
            yield return typeof (BasicNode);
        }

        public void Launch(LiveContext context) {
            context.SetArgument(this);
            
            var levelScriptNodes = nodes
                .CastIfPossible<LevelScriptNode>()
                .ToArray();
                
            levelScriptNodes.ForEach(n => n.context = context);
            levelScriptNodes.ForEach(n => n.OnLauch());
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            writer.Write("id", ID);
            writer.Write("name", name);
            writer.Write("stars", stars);
            writer.Write("colorSettings", colorSettings);
            writer.Write("localizationKey", localizationKey);
            
            if (preloadMode) return;
            
            writer.Write("chipBody", chipBody);
            writer.Write("extensions", extensions.ToArray());
            
            base.Serialize(writer);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("id", ref ID);
            reader.Read("name", ref name);
            reader.Read("stars", ref stars);
            reader.Read("colorSettings", ref colorSettings);
            reader.Read("localizationKey", ref localizationKey);
            
            if (preview) return;
            
            reader.Read("chipBody", ref chipBody);
            extensions = reader.ReadCollection<ContentInfo>("extensions").ToList();
            
            base.Deserialize(reader);
        }
        
        #endregion

        #region Localization
        
        [LocalizationKeysProvider]
        static IEnumerator<string> GetLocalizationKeys() {
            var previews = new LevelScriptOrderedPreviews();
            Serializator.FromTextData(previews, TextData.LoadText(Path.Combine(nameof(LevelScriptOrdered), $"LevelsPreview{Serializator.FileExtension}")));
            foreach (var level in previews.scripts) {
                if (!level.IsLocalized()) continue;
                level.LoadCompletely().Complete().Run();

                foreach (var node in level.nodes)
                    if (node is ILocalizedLevelScriptNode llsn)
                        foreach (var key in llsn.GetLocalizationKeys(level))
                            yield return key;
            }
        }

        #endregion

        public override string ToString() {
            return name.IsNullOrEmpty() ? 
                GetType().Name.NameFormat(null, null, true) : name;
        }
        
        const string localizationPath = "Levels/{0}/{1}";

        public string GetFullLocalizationKey(string key) {
            return localizationPath.FormatText(LocalizationPath, key);
        }
    }
}