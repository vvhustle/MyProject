using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using AOEvent = Yurowm.Analytics.DebugAnalyticsIntegration.AOEvent;
using AOEvents = Yurowm.Analytics.DebugAnalyticsIntegration.AOEvents;

namespace Yurowm.Analytics {
    public static class DebugAnalyticsReportData {

        #region Menu
        
        [MenuItem("Yurowm/Analytics/Reload")]
        public static void ReloadData() {
            Load();
        }
        
        [MenuItem("Yurowm/Analytics/Merge Files")]
        public static void MergeFiles() {
            Load();
            if (!isInitialized) {
                EditorUtility.DisplayDialog("Merge Files", "Error: Not Initialized", "Ok");
                return;
            }
            
            if (events.Count == 0) {
                EditorUtility.DisplayDialog("Merge Files", "Error: There is not any events", "Ok");
                return;
            }
            
            AOEvents merged = new AOEvents();

            events.ForEach(e => merged.Add(e));
            
            string raw;    
            try {
                raw = Serializator.ToTextData(merged);
            } catch (Exception e) {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Merge Files", "Error: Serialization problem", "Ok");
                return;
            }
            
            directory.Refresh();
            directory.GetFiles()
                .Where(f => f.Extension == Serializator.FileExtension)
                .ForEach(f => f.Delete());
            
            FileInfo file = new FileInfo(Path.Combine(directory.FullName, "Merged" + Serializator.FileExtension));
            
            File.WriteAllText(file.FullName, raw);
            
            EditorUtility.DisplayDialog("Merge Files", 
                $"Succcess: All events merged into {file.Name} file",
                "Ok");
        }
        
        #endregion
        
        static DirectoryInfo directory;
        
        static bool isInitialized = false;
        
        static void Initialize() {
            if (isInitialized || !Utils.IsMainThread()) return;
            
            directory = new DirectoryInfo(Path.Combine(Application.dataPath, "Editor Default Resources", "Analytics"));
            if (!directory.Exists) directory.Create();
            
            isInitialized = true;
            
            Load();
        }
        
        static List<AOEvent> events = new List<AOEvent>();
        public static void Load() {
            Initialize();
            
            if (!isInitialized) return;
            
            events.Clear();

            foreach (FileInfo file in directory.GetFiles()) {
                if (file.Extension != Serializator.FileExtension) 
                    continue;
                
                try {
                    var aoEvents = new AOEvents();
                    Serializator.FromTextData(aoEvents, File.ReadAllText(file.FullName));
                    events.AddRange(aoEvents);     
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }
        
        public static IEnumerable<AOEvent> GetEvents() {
            if (!isInitialized) Initialize();
            if (!isInitialized) yield break;

            foreach (var e in events)
                yield return e;
        }
        
    }

    #region Reports
    
    public abstract class DebugAnalyticsReport {
        
        string build = null;
        
        public string Text {
            get {
                try {
                    build = Build(DebugAnalyticsReportData.GetEvents());
                } catch (Exception e) {
                    Debug.LogException(e);
                    build = "Error";
                }
                if (build.IsNullOrEmpty()) 
                    build = "Null or Empty";
                
                return build;
            }
        }

        public abstract string Build(IEnumerable<AOEvent> events);
        
        public void Draw() {
            if (build == null)
                build = Text;
            
            EditorGUILayout.HelpBox(build, MessageType.None);
        }
    }
    
    public abstract class UniversalDebugAnalyticsReport : DebugAnalyticsReport {
        
        public abstract bool FilterEvent(AOEvent e);
        public abstract IEnumerable<ValueSampler> GetSamplers();

        public override string Build(IEnumerable<AOEvent> events) {
            Dictionary<string, ValueSampler> samplers = GetSamplers()
                .ToDictionary(s => s.Name, s => s);

            int sampleSize = 0;
            
            foreach (var e in events) {
                if (!FilterEvent(e)) continue;
                
                sampleSize ++;

                foreach (var pair in e) {
                    if (!samplers.TryGetValue(pair.Key, out var sampler)) {
                        sampler = new NumberSampler(pair.Key);
                        samplers.Add(pair.Key, sampler);
                    }
                    sampler.Sample(pair.Value);
                }
            }

            StringBuilder builder = new StringBuilder();
            
            builder.AppendLine($"Sample Size: {sampleSize}");

            foreach (var sampler in samplers.Values)
                sampler.Report(builder);
            
            return builder.ToString();
        }
    }
    
    #endregion
        
    #region Samplers
    
    public abstract class ValueSampler {
        protected string name;
        public string Name => name;
        
        protected int count = 0;
        
        public ValueSampler(string name) {
            this.name = name;
        }
        
        public abstract void Sample(string value);
        
        public abstract void Report(StringBuilder builder);
    }
    
    public class NumberSampler : ValueSampler {
        
        float avg = 0;
        float min = float.MaxValue;
        float max = float.MinValue;   
        
        public NumberSampler(string name) : base(name) {}
        
        public override void Sample(string value) {
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float number)) {
                count++;
                avg += number;
                min = Mathf.Min(min, number);
                max = Mathf.Max(max, number);
            }
        }
        
        public override void Report(StringBuilder builder) {
            if (count <= 0) return;
            
            if (count == 1 || min == max)
                builder.AppendLine($"{name}: {avg/count}");
            else 
                builder.AppendLine($"{name}: {avg/count} ({min} - {max})");
        }
    }
    
    public class TextSampler : ValueSampler {
        
        Dictionary<string, int> values = new Dictionary<string, int>();
        int countOfVisibleValues = 0;
        
        public TextSampler(string name) : base(name) {}
        public TextSampler(string name, int countOfVisibleValues = 5) : base(name) {
            this.countOfVisibleValues = countOfVisibleValues;
        }
        
        public override void Sample(string value) {
            count++;
            
            if (values.TryGetValue(value, out int c))
                values[value] = c + 1;
            else
                values[value] = 1;
        }
        
        public override void Report(StringBuilder builder) {
            if (count <= 0) return;
            
            if (count == 1)
                builder.AppendLine($"{name}: {values.Keys.First()} (100%)");
            else {
                int number = 0;
                builder.AppendLine($"{name}:");
                values
                    .OrderByDescending(v => v.Value)
                    .Take(countOfVisibleValues > 0 ? countOfVisibleValues : int.MaxValue)
                    .ForEach(v => {
                        builder.AppendLine($"\t{v.Key} ({100f * v.Value / count}%)");
                    });
            }
        }
    }
    
    #endregion
}
