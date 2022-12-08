using UnityEditor;
using Yurowm.Help;
using Yurowm.ObjectEditors;

namespace Yurowm.Serialization {
  
    public class StorageElementExtraDataEditor : ObjectEditor<IStorageElementExtraData> {
        public override void OnGUI(IStorageElementExtraData data, object context = null) {
            data.storageElementFlags = (StorageElementFlags) EditorGUILayout
                .EnumFlagsField("Element Flags", data.storageElementFlags);
            
            EditorTips.PopLastRectByID("storage.elementflags");
            
            if (data.storageElementFlags.HasFlag(StorageElementFlags.DebugOnly))
                EditorGUILayout.HelpBox("Debug Element. It will not be avaliable in the release build.", MessageType.Warning, false);
            
            if (data.storageElementFlags.HasFlag(StorageElementFlags.ReleaseOnly))
                EditorGUILayout.HelpBox("Release Only Element. It will not be avaliable in the developmet build.", MessageType.Warning, false);
            
            if (data.storageElementFlags.HasFlag(StorageElementFlags.WorkInProgress))
                EditorGUILayout.HelpBox("Work In Progress", MessageType.Warning, false);
        }
    }
}