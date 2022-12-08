using System;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.UI {
    public class PageTag : PageExtension {
        public string tag;

        public bool Compare(string tag) {
            if (tag.IsNullOrEmpty())
                return false;
            return string.Equals(this.tag, tag, StringComparison.CurrentCultureIgnoreCase);
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("tag", tag);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("tag", ref tag);
        }
    }
    
    public static class PageTagExtensions {
        public static bool HasTag(this Page page, string tag) {
            if (tag.IsNullOrEmpty())
                return false;

            return page.extensions.CastIfPossible<PageTag>().Any(t => t.Compare(tag));
        } 
    }
}