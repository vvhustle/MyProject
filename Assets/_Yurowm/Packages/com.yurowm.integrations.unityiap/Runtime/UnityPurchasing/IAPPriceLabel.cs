using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.UI;

namespace Yurowm.Services {
    public class IAPPriceLabel : LabelTextProviderBehaviour {
        public string IAP_ID;
        
        public override string GetText() {
            #if UNITY_IAP
            if (!IAP_ID.IsNullOrEmpty())
                return Integration.Get<UnityIAPIntegration>()?.GetIAP(IAP_ID)?.Price;
            #endif
            
            return "N/A";
        }
    }
}