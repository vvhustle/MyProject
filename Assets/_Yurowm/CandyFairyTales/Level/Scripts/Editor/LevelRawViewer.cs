using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YMatchThree.Core;
using YMatchThree.Editor;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.InUnityReporting;

namespace BubbleShooter {
    public class LevelRawViewer : LevelEditorBase {
        SerializableReport report;
        SerializableReportEditor editor = new SerializableReportEditor();
        
        public override void OnGUI() {
            editor.OnGUI(GUILayout.ExpandHeight(true));
        }

        public override void OnSelectAnotherLevel(LevelScriptOrderedFile file) {
            report = new SerializableReport(file.Script);
            report.Refresh();
            editor.SetProvider(report);
        }

        public override string GetName() {
            return "Raw";
        }
    }
}