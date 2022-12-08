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
    public abstract class UXOffer : Offer {
        
        public abstract bool IsReady();

        static List<Offer> ordered = new List<Offer>();
        
        [Flags]
        public enum Parameters {
            OrderOnly = 1 << 0
        }
        
        public Parameters parameters;

        public static void Order(UXOffer offer) {
            if (offer != null)
                ordered.Add(offer);
        }
        
        public static void ShowOfferOrdered() {
            if (!ordered.IsEmpty())
                ShowOffer(ordered.Grab());
        }
        
        public static void ShowOffer() {
            Offer offer = null;
                
            if (!ordered.IsEmpty())
                offer = ordered.Grab();
                
            if (offer == null)
                offer = storage
                    .CastIfPossible<UXOffer>()
                    .Where(o => 
                        !o.parameters.HasFlag(Parameters.OrderOnly) && 
                        o.IsReady())
                    .GetRandom();

            if (offer != null) 
                ShowOffer(offer);
        }
        
        static void ShowOffer(Offer offer) {
            offer?.Show();
        }
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("parameters", parameters);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("parameters", ref parameters);
        }
    }
}