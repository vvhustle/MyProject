using UnityEngine;
using System.Collections.Generic;

namespace Yurowm.UI {
    public class UIManager : MonoBehaviour {
        
        public void ShowPage(string pageID) {
            Page.Get(pageID).Show();
        }
        
        public void ShowPreviousPage() {
            Page.Back();
        }
    }
    
    public static class InputLock {
        static List<string> keys = new List<string>();
        static bool full = false;

        public static void AddPassKey(string key) {
            if (!keys.Contains(key))
                keys.Add(key);
        }
        
        public static void LockEverything() {
            keys.Clear();
            full = true;
        }
        
        public static void Unlock() {
            keys.Clear();
            full = false;
        }
        
        public static bool GetAccess(string key) {
            if (full) return false;
            if (keys.Count == 0) return true;
            return keys.Contains(key);
        }
    }
}