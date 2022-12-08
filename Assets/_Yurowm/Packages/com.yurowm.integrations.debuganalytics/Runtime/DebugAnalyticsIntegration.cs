using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Analytics {
    public class DebugAnalyticsIntegration : AnalyticIntegration {
        public override void Initialize() {
            base.Initialize();
            
            if (!Application.isEditor) {
                active = false;
                return;
            }
            
            if (file.Exists) {
                var raw = File.ReadAllText(file.FullName);
                Serializator.FromTextData(events, raw);
            }

            #if NATVIESHARE
            DebugPanel.Log("Share", "Debug Analytics", () => {
                file.Refresh();
                if (file.Exists)
                    new NativeShare()
                        .AddFile(file.FullName) 
                        .SetSubject("Debug Analytics Data")
                        .Share();
                
            });
            #endif
            
            DebugPanel.Log("Clear", "Debug Analytics", () => events.Clear());
            
            DirtyChecker().Run();
        }
        
        #if UNITY_EDITOR
        static readonly FileInfo file = new FileInfo(Path.Combine(Application.dataPath, "Editor Default Resources", "Analytics", "Editor" + Serializator.FileExtension));
        #else
        static readonly FileInfo file = new FileInfo(Path.Combine(Application.persistentDataPath, "DebugAnalytics" + Serializator.FileExtension));
        #endif

        public override string GetName() {
            return "Debug Analytics";
        }

        AOEvents events = new AOEvents();
        
        IEnumerator DirtyChecker() {
            DelayedAccess access = new DelayedAccess(1f);
            while (true) {
                if (events.isDirty && access.GetAccess()) {
                    var raw = Serializator.ToTextData(events);
                    File.WriteAllText(file.FullName, raw);
                }
                    
                yield return null;
            }
        }
        
        public override void Event(string eventName) {
            events.Add(new AOEvent(eventName));
        }
        
        public override void Event(string eventName, params Segment[] segments) {
            var e = new AOEvent(eventName);
            segments.ForEach(s => e[s.ID] = s.value.ToString());
            events.Add(e);
        }

        public class AOEvents : ISerializable, IEnumerable<AOEvent> {
            List<AOEvent> events = new List<AOEvent>(1000);
            
            public void Add(AOEvent e) {
                events.Add(e);
                isDirty = true;
            }

            public void Clear() {
                events.Clear();
                isDirty = true;
            }

            public bool isDirty = false;
            
            public void Serialize(Writer writer) {
                writer.Write("events", events);
            }

            public void Deserialize(Reader reader) {
                events.Clear();
                events.AddRange(reader.ReadCollection<AOEvent>("events"));
            }

            public IEnumerator<AOEvent> GetEnumerator() {
                foreach (var e in events)
                    yield return e;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }
        
        [SerializeShort]
        public class AOEvent : ISerializable, IEnumerable<KeyValuePair<string, string>> {
            public string name; 
            Dictionary<string, string> dict;
            
            public AOEvent() {
                dict = new Dictionary<string, string>();
            }
            
            public AOEvent(string name) : this() {
                this.name = name;
            }
            
            public AOEvent(string name, Dictionary<string, string> segmentation) {
                this.name = name;
                dict = segmentation;
            }
            
            public string this[string key] {
                get => dict.Get(key);
                set => dict[key] = value;
            }
            
            public void Serialize(Writer writer) {
                writer.Write("name", name);
                writer.Write("time", Time.unscaledTime.ToString(CultureInfo.InvariantCulture));
                writer.Write("dict", dict);
            }

            public void Deserialize(Reader reader) {
                reader.Read("name", ref name);
                dict = reader.ReadDictionary<string>("dict").ToDictionary();
                dict["time"] = reader.Read<string>("time");
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
                foreach (var pair in dict)
                    yield return pair;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public bool TryGet(string key, out string value) {
                return dict.TryGetValue(key, out value);
            }
        }
    }
}