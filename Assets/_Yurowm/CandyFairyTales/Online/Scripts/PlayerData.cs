using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Features;
using Yurowm.Integrations;
using Yurowm.Serialization;
using Yurowm.Services;
using Yurowm.Store;
using Yurowm.UI;
using Yurowm.Utilities;

namespace YMatchThree.Meta {
    public static class PlayerData {
        
        [OnLaunch(Integration.INITIALIZE_ORDER + 1)]
        static void OnLaunch() {
            if (OnceAccess.GetAccess("PlayerData")) {
                #if PLAYFAB
                var playfab = Integration.Get<PlayfabIntegration>();
                    
                playfab?.AddOnSignedInListener(TryToLoad);
                playfab?.AddOnSignedOutListener(Clear);
                #endif
                
                Update().Run();
            }
        }
        
        public static Inventory inventory => App.data.GetModule<Inventory>();
        
        public static LevelProgressModule levelProgress => App.data.GetModule<LevelProgressModule>();
        
        public static LifeSystemData life => App.data.GetModule<LifeSystemData>();
        
        public static StoreData store => App.data.GetModule<StoreData>();
        
        static DataInfo info => App.data.GetModule<DataInfo>();
        
        public static IEnumerable<GameData.Module> GetModules() {
            yield return info;
            yield return inventory;
            yield return levelProgress;
            yield return life;
            yield return store;
        }
        
        public static IEnumerable<string> GetModuleKeys() {
            return GetModules().Select(m => m.GetKey());
        }
        
        static bool isDirty = false;
        
        public static void SetDirty() {
            GetModules().ForEach(m => m.SetDirty());
            isDirty = true;
        }
        
        static IEnumerator Update() {
            while (true) {
                if (isDirty) {
                    TryToSave();
                }
                
                yield return new Wait(5);
            }
        }
        
        public static Dictionary<string, string> GetRawData() {
            return ModulesToRawData(GetModules());
        }
        
        public static void SetRawData(Dictionary<string, string> data, bool force = false) {
            
            string raw;
            
            if (!force && data.TryGetValue(info.GetKey(), out raw)) {
                Conflict(
                    cloud: RawDataToModules(data).ToArray(), 
                    local: GetModules().ToArray());
                
                return;
            }
            
            foreach (var module in GetModules()) {
                if (data.TryGetValue(module.GetKey(), out raw)) {
                    Serializator.FromTextData(module, raw);
                    module.SetDirty();
                }
            }
            
            UIRefresh.Invoke();
        }
        
        static void Conflict(GameData.Module[] cloud, GameData.Module[] local) {
            void Apply(GameData.Module[] data) {
                SetRawData(ModulesToRawData(data), true);
                TryToSave();
            }
            
            var localDataInfo = info;

            if (localDataInfo.IsEmpty()) {
                Apply(cloud);
                return;
            }
            
            var lifeCloud = cloud.CastOne<LifeSystemData>();
            var lifeSystem = Integration.Get<LifeSystem>();
            
            if (lifeCloud != null && lifeSystem != null)
                lifeSystem.Normalize(lifeCloud, DateTime.UtcNow);

            if (IsTheSame(cloud, local)) {
                Apply(cloud);
                return;
            }

            var cloudDataInfo = cloud.CastOne<DataInfo>();
            
            if (localDataInfo.lastSaveTime > cloudDataInfo.lastSaveTime) 
                Apply(local);
            else
                Apply(cloud);
            
            
            // new PlayerDataConflictPopup(cloud, local).Show();
        }
        
        static bool IsTheSame(GameData.Module[] data1, GameData.Module[] data2) {
            string[] ToRawArray(GameData.Module[] data) {
                return data
                    .Where(m => !(m is DataInfo))
                    .Select(m => m.ToRaw())
                    .OrderBy(r => r)
                    .ToArray();
            }
            
            var raw1 = ToRawArray(data1);
            var raw2 = ToRawArray(data2);
            
            if (raw1.Length != raw2.Length)
                return false;
            
            
            for (var i = 0; i < raw1.Length; i++)
                if (raw1[i] != raw2[i])
                    return false;

            return true;
        }
        
        public static IEnumerable<GameData.Module> RawDataToModules(Dictionary<string, string> data) {
            return GetModules()
                .Where(m => data.ContainsKey(m.GetKey()))
                .Select(m => m.Clone())
                .Initialize(m => Serializator.FromTextData(m, data[m.GetKey()]));
        }
        
        public static Dictionary<string, string> ModulesToRawData(IEnumerable<GameData.Module> modules) {
            return modules
                    .ToDictionary(
                        m => m.GetKey(),
                        m => m.ToRaw());
        }
        
        static bool silentMode = false;
        
        public static bool IsSilentMode() => silentMode;

        static bool saving = false;
        
        public static void TryToSave() {
            if (saving) return;
            
            #if PLAYFAB
            
            var playfabIntegration = Integration.Get<PlayfabIntegration>();
            
            if (playfabIntegration != null && playfabIntegration.IsLoggedIn()) {
                
                var saveTime = DateTime.UtcNow;
                var previousSaveTime = info.lastSaveTime;
                
                void OnSuccess() {
                    info.lastSaveTime = saveTime;
                    info.SetDirty();
                    
                    saving = false;
                    isDirty = false;
                }

                void OnFailed() {
                    saving = false;
                }
                
                info.lastSaveTime = saveTime;
                playfabIntegration.SaveProgress(GetRawData(), OnSuccess, OnFailed);
                info.lastSaveTime = previousSaveTime;
                
            } else 
                saving = false;
            
            #endif
        }


        public static void TryToLoad() {
            #if PLAYFAB
            
            var playfabIntegration = Integration.Get<PlayfabIntegration>();
            
            if (playfabIntegration == null || !playfabIntegration.IsLoggedIn()) 
                return;
            
            silentMode = true;
            
            void OnSuccess(Dictionary<string, string> data) {
                silentMode = false;
                SetRawData(data);    
            }            
            
            void OnFailed() {
                silentMode = false;
            }            
            
            playfabIntegration.LoadProgress(GetModuleKeys(), OnSuccess, OnFailed);
            
            #endif
        }
        
        public static void Clear() {
            App.data.Clear();
            
            UIRefresh.Invoke();
        }
        
        public class DataInfo: GameData.Module {
            public DateTime lastSaveTime;
            
            public bool IsEmpty() => lastSaveTime.Ticks == 0;

            public override void Serialize(Writer writer) {
                writer.Write("lastSaveTime", lastSaveTime);
            }

            public override void Deserialize(Reader reader) {
                reader.Read("lastSaveTime", ref lastSaveTime);
            }
        }
    }
}