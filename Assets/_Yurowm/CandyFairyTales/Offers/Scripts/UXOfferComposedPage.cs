using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ComposedPages;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Offers {
    public abstract class UXOfferComposedPage : UXOffer {
        
        public abstract Page GetPage();

        public override void Show() {
            ComposedPage.ByID("Offer").Show(GetPage());
        }
    }
}