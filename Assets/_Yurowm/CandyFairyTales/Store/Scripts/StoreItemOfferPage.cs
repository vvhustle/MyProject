using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ComposedPages;
using Yurowm.Extensions;
using Yurowm.UI;
using Page = Yurowm.ComposedPages.Page;

namespace Yurowm.Store {
    public abstract class StoreItemOfferPage : Page, IUIRefresh {
        string layoutName;
        
        public StoreItemOfferPage(string layoutName = nameof(StoreItemComposed)) {
            this.layoutName = layoutName;
            backgroundClose = true;
        }
        
        public abstract IEnumerable<StoreGood> GetGoods();
        
        StoreGood[] goods;
        
        public bool closeOnBuy = false;
        
        public override void Building() { 
            if (goods == null)
                goods = GetGoods()
                    .NotNull()
                    .Distinct()
                    .Take(3)
                    .ToArray();
            
            if (goods.IsEmpty())
                return;
            
            void SetNotActual() {
                isActual = false;
            }
            
            if (goods.Length == 1) {
                var storeItem = AddElement<StoreItemComposed>(layoutName);
                if (storeItem) {
                    goods[0].SetupBody(storeItem.button); 
                    if (closeOnBuy)
                        storeItem.button.buy.onClick.AddListener(SetNotActual);
                }
            } else {
                var container = AddElement<ComposedContainer>("ComposedHorizontalLayout");
                foreach (var pack in goods) {
                    var storeItem = container.AddElement<StoreItemComposed>(layoutName);
                    if (!storeItem)
                        break;
                    
                    pack.SetupBody(storeItem.button);
                    if (closeOnBuy)
                        storeItem.button.buy.onClick.AddListener(SetNotActual);
                }
            }
            
        }
    }
}