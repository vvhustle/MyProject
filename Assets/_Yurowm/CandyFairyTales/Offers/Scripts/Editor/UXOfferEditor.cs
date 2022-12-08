using Yurowm.ObjectEditors;
using UnityEditor;

namespace Yurowm.Offers {
    public class UXOfferEditor : ObjectEditor<UXOffer> {
        public override void OnGUI(UXOffer offer, object context = null) {
            offer.parameters = (UXOffer.Parameters) EditorGUILayout.EnumFlagsField("Parameters", offer.parameters);
        }
    }
}