using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Simulator
{
    [CustomEditor(typeof(Simulator))]
    public class Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var simulator = target as Simulator;

            if (GUILayout.Button("맵 만들기"))
            {
                // simulator.SetMap();
            }

            if (GUILayout.Button("시뮬레이션 시작"))
            {
                // simulator.StartSimulation();
            }
        }
    }
}
