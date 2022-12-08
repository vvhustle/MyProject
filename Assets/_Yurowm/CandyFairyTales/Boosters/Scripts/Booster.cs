using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Meta;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Store;
using Yurowm.UI;
using Behaviour = Yurowm.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class Booster : LevelContent, ILocalized, IVirtualizedScrollItem {
        
        public string buttonName;
        public string iconName;
        public string animationName;

        public override Type GetContentBaseType() {
            return typeof(Booster);
        }

        public virtual void SetupBody(VirtualizedScrollItemBody body) {
            if (!(body is BoosterButton bb))
                return;
            
            bb.booster = this;
            
            if (bb.icon) {
                var boosterIcon = AssetManager.GetAsset<Sprite>(iconName);
                
                bb.icon.sprite = boosterIcon;
                bb.icon.color = boosterIcon ? Color.white : Color.clear;
            }

            var boosterCount = PlayerData.inventory.GetItemCount(ID);

            bb.empty?.SetActive(boosterCount <= 0);
            if (bb.count) {
                bb.count.gameObject.SetActive(boosterCount > 0);
                bb.count.text = boosterCount.ToString();
            }
        }

        public string GetBodyPrefabName() {
            return buttonName;
        }

        public bool CanBeRedeemed() => PlayerData.inventory.GetItemCount(ID) > 0;

        public bool Redeem() {
            if (!CanBeRedeemed())
                return false;
            
            PlayerData.inventory.SpendItems(ID, 1);

            scriptEvents.onRedeemBooseter?.Invoke(this);
            UIRefresh.Invoke();

            PlayerData.SetDirty();
            
            return true;
        }
        
        protected ContentAnimator animator;
        protected ContentSound sound;
        
        public abstract void OnClick();
        
        public IEnumerator PlayAnimation(Action<string> onCallback) {
            var body = AssetManager.Emit<BoosterAnimationBody>(animationName, context);
            
            if (!body) yield break;
            
            body.SetupComponent(out sound);
            body.SetupComponent(out animator);
            
            var called = false;
            
            void Call(string _ = null) {
                called = true;
                sound?.Play("Callback");
                onCallback?.Invoke(_);
            }
            
            body.transform.SetParent(field.root);
            body.transform.Reset();
            body.transform.localPosition = position;
            
            body.callback = Call;
            
            sound?.Play("Redeem");
            yield return animator?.PlayAndWait("Redeem");

            if (!called)
                Call();
            
            body.Kill();
        }
        
        public IEnumerator ShowInStore() {
            yield return Page.Get("Store")?.ShowAndWait();
            
            var list = Behaviour.Get<StoreList>();
            
            if (!list) yield break;
            
            var targetItem = StoreItem.storage
                .Items<StorePack>()
                .FirstOrDefault(i => i.items
                    .Any(i => i is StorePackItemCount spic && spic.itemID == ID));

            list.CenterTo(targetItem, 1);
        }
        
        #region Localizion

        public virtual IEnumerable GetLocalizationKeys() {
            yield break;
        }
        
        #endregion
        
        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("iconName", iconName);
            writer.Write("animationName", animationName);
            writer.Write("buttonName", buttonName);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("iconName", ref iconName);
            reader.Read("animationName", ref animationName);
            reader.Read("buttonName", ref buttonName);
        }

        #endregion
    }
    
    public class BoosterSetVariable : ContentInfoVariable {
        
        public List<string> IDs = new List<string>();

        public override void Serialize(Writer writer) {
            writer.Write("set", IDs.ToArray());
        }

        public override void Deserialize(Reader reader) {
            IDs.Reuse(reader.ReadCollection<string>("set"));
        }
    }
}