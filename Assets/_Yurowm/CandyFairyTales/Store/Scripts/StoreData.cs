using System;
using System.Collections.Generic;
using System.Linq;
#if FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif
using Yurowm.Analytics;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.Store {
    public class StoreData : GameData.Module {

        #region Inventory
        
        Dictionary<string, int> inventory = new Dictionary<string,int>();
        
        public int GetCount(string itemID) {
            if (itemID.IsNullOrEmpty())
                return 0;
            if (inventory.TryGetValue(itemID, out var value))
                return value.ClampMin(0);
            return 0;
        }
        
        public void AddItem(string itemID, int count) {
            if (count == 0 || itemID.IsNullOrEmpty())
                return;
            
            inventory.TryGetValue(itemID, out var value);
           
            value = (value + count).ClampMin(0);
            
            if (value > 0)
                inventory[itemID] = value;
            else
                inventory.Remove(itemID);
            
            SetDirty();
        }

        #endregion

        #region Access Keys

        List<Keychain> keychains = new List<Keychain>();

        public static Action<string> onAddAccess = delegate {};
        
        public bool HasAccess(string accessKey) {
            return keychains.Any(c => c.keys.Contains(accessKey));
        }
        
        public void AddKeychain(string ID, IEnumerable<string> keys) {
            if (ID.IsNullOrEmpty())
                return;
            
            RemoveKeychain(ID);
            var keychain = new Keychain {
                ID = ID,
                keys = keys.ToArray()
            };
            
            if (keychain.keys.IsEmpty())
                return;
            
            keychains.Add(keychain);
            keychain.keys.ForEach(k => onAddAccess(k));
            SetDirty();
        }
        
        public void RemoveKeychain(string ID) {
            if (keychains.RemoveAll(k => k.ID == ID) > 0)
                SetDirty();
        }
        
        public bool HasKeychain(string ID) {
            return keychains.Any(k => k.ID == ID);
        }
        
        public IEnumerable<Keychain> GetAllKeychains() {
            foreach (var keychain in keychains)
                yield return keychain;
        }
        
        public IEnumerable<string> GetAllAccessKeys() {
            return keychains
                .SelectMany(c => c.keys)
                .Distinct();
        }
        
        public class Keychain : ISerializableID {
            public string ID { get; set; }
            public string[] keys;
            
            public void Serialize(Writer writer) {
                writer.Write("ID", ID);
                writer.Write("keys", keys);
            }

            public void Deserialize(Reader reader) {
                ID = reader.Read<string>("ID");
                keys = reader.ReadCollection<string>("keys").ToArray();
            }
        }
        
        #endregion

        public void Clear() {
            keychains.Clear();
        }

        public override void Serialize(Writer writer) {
            writer.Write("keychains", keychains.ToArray());
            
            writer.Write("inventory", inventory);
        }

        public override void Deserialize(Reader reader) {
            inventory.Clear();
            inventory.AddPairs(reader.ReadDictionary<int>("inventory"));
            
            keychains.Clear();
            keychains.AddRange(reader.ReadCollection<Keychain>("keychains"));
        }
    }
}