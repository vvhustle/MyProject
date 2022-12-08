using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;

namespace Yurowm.Editors {
    public class VirusEditor : ObjectEditor<Virus> {
        public override void OnGUI(Virus virus, object context = null) {
            Edit("Eating", virus.eating);
        }
    }
}