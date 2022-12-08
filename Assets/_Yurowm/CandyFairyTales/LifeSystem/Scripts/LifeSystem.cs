using System;
using System.Collections;
using YMatchThree.Meta;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.Serialization;
using Yurowm.Services;
using Yurowm.Store;
using Yurowm.UI;

namespace Yurowm.Features {
    public class LifeSystem : Integration {
        
        public int lifeCount = 5;
        public TimeSpan lifeRecoveryTimeSpan = new TimeSpan(0, 20, 0);
        
        ITimeProvider timeProvider;
        public LifeSystemData data => App.data.GetModule<LifeSystemData>();
        
        IEnumerator recovering;

        public bool IsRecovering => data.count < lifeCount && data.IsRecovering;

        public override string GetName() {
            return "Life System";
        }

        public override void Initialize() {
            base.Initialize();
            
            timeProvider = (ITimeProvider) Get<TrueTime>() ?? new SystemTimeProvider();
            
            if (data.count < 0) 
                SetFull();
            
            if (data.lifeLock) 
                BurnLife();
            
            if (data.count < lifeCount) {
                recovering = LifeRecovering();
                recovering.Run();
            }
            
            #if DEBUG
            DebugLogic().Run();
            #endif
        }
        
        [ReferenceValueLoader]
        static void AddReferences() {
            var ls = Get<LifeSystem>();
            
            if (ls == null) return;
            
            ReferenceValues.Add("Lives", () => ls.data.count);
            ReferenceValues.Add("LiveSlots", () => ls.lifeCount);
        }

        public void SetFull() {
            data.count = lifeCount;
            data.lastBurn = default;
            data.SetDirty();
        }
        
        public void AddLife(int count = 1) {
            if (count < 1)
                return;
            
            data.count += count;
            
            if (data.count >= lifeCount) 
                data.lastBurn = default;
            
            data.SetDirty();
        }

        public void LockLife() {
            data.lifeLock = true;
            data.SetDirty();
        }

        public void UnlockLife() {
            data.lifeLock = false;
            data.SetDirty();
        }

        public bool HasLife() {
            return HasInfiniteLife() || data.count > 0;
        }

        public bool HasInfiniteLife() {
            return GetInfiniteLifeTime().Ticks > 0;
        }

        public TimeSpan GetInfiniteLifeTime() {
            if (!timeProvider.HasTime)
                return TimeSpan.Zero;
            return data.infiniteLifeEnd - timeProvider.UTCNow;
        }
        
        public void BurnLife() {
            if (!data.lifeLock) return;
            
            if (!HasLife()) return;
            
            if (!HasInfiniteLife()) 
                data.count --;
            data.lifeLock = false;
            data.SetDirty();
            
            if (recovering == null) {
                recovering = LifeRecovering();
                recovering.Run();
            }
            
            PlayerData.SetDirty();
        }

        public TimeSpan GetRecoveringTime() {
            if (!data.IsRecovering || !timeProvider.HasTime) 
                return TimeSpan.Zero;
            
            return data.lastBurn + lifeRecoveryTimeSpan - timeProvider.UTCNow;
        }

        public float GetRecoveringProgress() {
            if (!data.IsRecovering || !timeProvider.HasTime) 
                return 0;
            
            var progress = lifeRecoveryTimeSpan.TotalSeconds;
            
            progress = (progress - GetRecoveringTime().TotalSeconds) / progress;
            
            return (float) progress;
        }

        IEnumerator LifeRecovering() {
            yield return WaitForTime();
            
            while (data.count < lifeCount) {

                if (!data.IsRecovering) {
                    if (data.count < lifeCount) {
                        data.lastBurn = timeProvider.UTCNow;
                        data.SetDirty();
                    } else
                        break;
                }

                while (data.lastBurn + lifeRecoveryTimeSpan <= timeProvider.UTCNow) {
                    data.SetDirty();
                    if (data.count < lifeCount) 
                        data.count++;
                    if (data.count < lifeCount) 
                        data.lastBurn += lifeRecoveryTimeSpan;
                    else {
                        data.lastBurn = default;
                        break;
                    }
                }
                
                if (data.count < lifeCount) {
                    yield return new WaitTimeSpan(1f);
                    yield return WaitForTime();
                } else
                    break;
            }

            data.lastBurn = default;
            data.SetDirty();
            
            recovering = null;
        }
        
        public void Normalize(LifeSystemData data, DateTime currentTimeUTC) {
            if (data.count < lifeCount && !data.IsRecovering) {
                data.lastBurn = currentTimeUTC;
                return;
            }

            if (data.count < lifeCount) {
                var deltaLife = 
                    ((float) ((currentTimeUTC - data.lastBurn).TotalSeconds / lifeRecoveryTimeSpan.TotalSeconds))
                    .FloorToInt()
                    .ClampMax(lifeCount - data.count);
                
                data.count += deltaLife;
                data.lastBurn += deltaLife * lifeRecoveryTimeSpan;
            }

            if (data.count >= lifeCount)
                data.lastBurn = default;
        }
        
        IEnumerator WaitForTime() {
            while (!timeProvider.HasTime)
                yield return null;
        }
        
        IEnumerator DebugLogic() {
            const string group = "Lives";
            
            DebugPanel.Log("Recover", group, SetFull);
            
            DebugPanel.Log("Infinite for 1 min", group, () => AddInfiniteLife(TimeSpan.FromMinutes(1)));
            DebugPanel.Log("Infinite for 1 hour", group, () => AddInfiniteLife(TimeSpan.FromHours(1)));
            
            while (true) {

                if (!data.IsRecovering)
                    DebugPanel.Log("Life Recovering", group, "No");
                else
                    DebugPanel.Log("Life Recovering", group, timeProvider.HasTime ?
                        (data.lastBurn + lifeRecoveryTimeSpan - timeProvider.UTCNow).ToString(@"hh\:mm\:ss") : "Wait");

                DebugPanel.Log("Life Count", group, $"{data.count}/{lifeCount}");
                DebugPanel.Log("Life Lock", group, data.lifeLock);
                
                yield return new WaitTimeSpan(1f);
            }
        }

        public void AddInfiniteLife(TimeSpan timeSpan) {
            if (!timeProvider.HasTime || timeSpan.Ticks <= 0)
                return;
            if (data.infiniteLifeEnd < timeProvider.UTCNow)
                data.infiniteLifeEnd = timeProvider.UTCNow;
            data.infiniteLifeEnd += timeSpan;
            data.SetDirty();
            
            PlayerData.SetDirty();
        }
        
        public static string TimeSpanToString(TimeSpan timeSpan) {
            var value = (int) timeSpan.TotalHours;
            
            string result;
            
            if (value > 0)
                result = value + ":";
            else 
                result = string.Empty;
            
            result += $"{timeSpan.Minutes:D2}:{timeSpan.Seconds.ClampMin(1):D2}";
            
            return result;
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("lifeCount", lifeCount);
            writer.Write("lifeRecoveryTimeSpan", lifeRecoveryTimeSpan);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("lifeCount", ref lifeCount);
            reader.Read("lifeRecoveryTimeSpan", ref lifeRecoveryTimeSpan);
        }

        #endregion
    }
        
    public class LifeSystemData : GameData.Module {
        public int count = -1;
        public bool lifeLock = false;
        public DateTime lastBurn;
        public DateTime infiniteLifeEnd;
        
        public bool IsRecovering => lastBurn.Ticks != 0;

        public override void Serialize(Writer writer) {
            writer.Write("count", count);
            writer.Write("lifeLock", lifeLock);
            writer.Write("lastBurn", lastBurn);
            writer.Write("infiniteLifeEnd", infiniteLifeEnd);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("count", ref count);
            reader.Read("lifeLock", ref lifeLock);
            reader.Read("lastBurn", ref lastBurn);
            reader.Read("infiniteLifeEnd", ref infiniteLifeEnd);
        }
    }
}