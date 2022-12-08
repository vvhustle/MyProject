using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.Serialization;

namespace Yurowm.Advertising {
    public abstract class AdIntegration : Integration {

        public int weight = 0;
        public AdType typeMask = (AdType) (~0);
        
        public List<AppKey> appKeys = new List<AppKey>();
        public List<AdKey> adKeys = new List<AdKey>();

        public override void Initialize() {
            base.Initialize();
            
            if (typeMask == 0) return;
            
            Adverts.integrations.Add(this);
        }

        public virtual void OnUpdate() {}

        public abstract bool IsReady(AdType type);
        
        public virtual string LogState(AdType type) {
            return IsReady(type).ToString();
        }

        public abstract void Show(AdType type, Action onComplete, Action<Adverts.Callback> callback);

        public AdKey GetAdKey(AdType type, string placeID = null) {
            
            var platform = GetPlatform();

            return adKeys
                .Where(k => k.platform == platform && k.type == type)
                .FirstOrDefaultFiltered(
                    k => k.placeID == placeID,
                    k => k.placeID.IsNullOrEmpty());
        }
        
        public IEnumerable<AdKey> GetSuitableAdKeys() {
            var platform = GetPlatform();
            
            return adKeys
                .Where(k => k.platform == platform && typeMask.OverlapFlag(k.type));
        }
        
        public AppKey GetAppKey() {
            var platform = GetPlatform();
            
            return appKeys
                .FirstOrDefault(k => k.platform == platform);
        }

        public bool IsUseless() {
            return this is IAdIntegrtaionUseless;
        }

        public static AdPlatform GetPlatform() {
            #if UNITY_ANDROID
            return AdPlatform.Android;
            #elif UNITY_IOS
            return AdPlatform.iOS;
            #endif
            return default;
        }
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("mask", typeMask);
            writer.Write("weight", weight);
            writer.Write("appKeys", appKeys.ToArray());
            writer.Write("adKeys", adKeys.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("mask", ref typeMask);
            reader.Read("weight", ref weight);
            appKeys.Reuse(reader.ReadCollection<AppKey>("appKeys"));
            adKeys.Reuse(reader.ReadCollection<AdKey>("adKeys"));
        }

        #endregion
    }
    
    public interface IAdIntegrtaionUseless {}
    
    public interface IAdIntegrationPreloadable {
        bool IsReadyToPreload(AdType type);
        void Preload(AdType type);
        bool IsLoading(AdType type);
        bool IsReady(AdType type);
    }
}