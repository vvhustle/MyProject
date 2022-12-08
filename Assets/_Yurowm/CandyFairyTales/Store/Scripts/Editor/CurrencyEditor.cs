using UnityEditor;
using Yurowm.ObjectEditors;

namespace Yurowm.Store {
    public class CurrencyEditor : ObjectEditor<Currency> {
        public override void OnGUI(Currency currency, object context = null) {
            currency.symbol = EditorGUILayout.TextField("Symbol", currency.symbol);
        }
    }
}