using Yurowm.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Serialization;
using Yurowm.Services;
using Yurowm.Utilities;

namespace Yurowm.Analytics {
    public class FlurryIntegration : AnalyticIntegration {
        public string ApiKey_iOS = "";
        public string ApiKey_Android = "";
        
        ThirdPartyAPI api;
        
        const string API_INIT = "Initialize";
        const string API_EVENT_EMPTY = "EventEmpty";
        const string API_EVENT_SEGMENTS = "EventSegments";
        
        public override void Initialize() {
            base.Initialize();

            api = ThirdPartyAPI.Get("Flurry");
            
            if (api == null) {
                active = false;
                return;
            }
            
            var apiKey = GetApiKey();
            
            if (!apiKey.IsNullOrEmpty()) 
                api.Call(API_INIT, apiKey);
            else
                active = false;
        }
        
        string GetApiKey() {
            if (Application.isEditor)
                return "EDITOR";
            switch (Application.platform) {
                case RuntimePlatform.Android: return ApiKey_Android;
                case RuntimePlatform.IPhonePlayer: return ApiKey_iOS;
                default: return null;
            }
        }

        public override void Event(string eventName) {
            api.Call(API_EVENT_EMPTY, eventName);
        }
        
        
        public override void Event(string eventName, params Segment[] segments) {
            api.Call(API_EVENT_SEGMENTS, eventName, segments
                .ToDictionary(s => s.ID, s => s.value.ToString()));
        }

        public override string GetName() {
            return "Flurry";
        }

        public override bool HasAllNecessarySDK() {
            #if !FLURRY
            return false;
            #endif
            return base.HasAllNecessarySDK();
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("androidKey", ApiKey_Android);
            writer.Write("iOSKey", ApiKey_iOS);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("androidKey", ref ApiKey_Android);
            reader.Read("iOSKey", ref ApiKey_iOS);
        }

        #endregion
    }

    
    public class FlurrySymbol : ScriptingDefineSymbolAuto {
        public override string GetSybmol() {
            return "FLURRY";
        }

        public override IEnumerable<string> GetRequiredPackageIDs() {
            yield return "com.yahoo.flurry";
        }
    }
}