using LuaInterface;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GM
{
    [CustomEditor(typeof(LuaMain))]
    public class LuaMainEditor : Editor
    {
        private string debugCode;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(5);

            GUILayout.Label("Debug Code");
            debugCode = GUILayout.TextArea(debugCode);
            if (GUILayout.Button("Run Code"))
            {
                var luaMain = target as LuaMain;
                Debugger.useLog = luaMain.LogDebugger;
                luaMain.DoString(debugCode, "debug");
            }
        }
    }
}
