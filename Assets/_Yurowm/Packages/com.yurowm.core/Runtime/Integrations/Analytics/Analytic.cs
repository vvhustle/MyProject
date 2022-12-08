using System.Collections.Generic;
using Yurowm.Extensions;
using Yurowm.UI;
using Yurowm.Utilities;

namespace Yurowm.Analytics {
    public static class Analytic {
        public static bool log = true;

        public static List<AnalyticIntegration> integrations = new List<AnalyticIntegration>();
        
        [OnLaunch()]
        public static void Initialize() {
            // if (!Application.isEditor)
            //     Application.logMessageReceived += Log;
            
            if (OnceAccess.GetAccess("Analytic")) 
                Page.onShow += OnShowPage;
        }

        static void OnShowPage(Page page) {
            Event($"PageView_{page.ID}");
        }
        
        // static void Log(string condition, string stackTrace, LogType type) {
        //     if (type == LogType.Exception)
        //         Event(type.ToString(), 
        //             Segment.New("condition", condition),
        //             Segment.New("stackTrace", stackTrace),
        //             Segment.New("appVersion", Application.version));
        // }

        static IEnumerable<AnalyticIntegration> AllActive() {
            foreach (var integration in integrations)
                if (integration != null && integration.active)
                    yield return integration;
        }
        
        static IEnumerable<AnalyticIntegration> AllFullTracked() {
            foreach (var integration in integrations)
                if (integration != null && integration.active && integration.trackAll)
                    yield return integration;
        }

        #region Events
        
        public static void Event(string eventName) {
            if (!log) return;
            AllFullTracked().ForEach(x => x.Event(eventName));
        }

        public static void Event(string eventName, params Segment[] segments) {
            if (!log) return;
            AllFullTracked().ForEach(x => x.Event(eventName, segments));
        }
        
        public static AI Event<AI>() where AI : AnalyticIntegration {
            return AllActive().CastOne<AI>();
        }

        #endregion
    }
    
    public struct Segment {
        public readonly string ID;
        public readonly object value;
        
        Segment(string ID, object value) {
            this.ID = ID;
            this.value = value;
        }
        
        public static Segment New(string ID, object value) {
            return new Segment(ID, value);
        }
    }
}


