using System;
using System.Collections.Generic;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace YMatchThree.Core {

    [SerializeShort]
    public class ContentInfo : ISerializable {
        public LevelContent Reference { get; private set; }
        public Type BaseType { get; private set; }
        public string ID => Reference?.ID;

        public ContentInfo() {}
        
        public ContentInfo(LevelContent reference) : this() {
            Reference = reference;
            BaseType = reference.GetContentBaseType();
        }
        
        public ContentInfo(LevelContent reference, IEnumerable<ContentInfoVariable> variables) : this(reference) {
            this.variables = variables?.ToArray();
        }

        #region Variables
        ContentInfoVariable[] variables;
        
        public IEnumerable<ContentInfoVariable> Variables() {
            if (variables == null)
                variables = Reference.EmitVariables().ToArray();
            for (int i = 0; i < variables.Length; i++)
                yield return variables[i];
        }
        
        public T GetVariable<T>() where T : ContentInfoVariable {
            return Variables().CastOne<T>();
        }
        #endregion
        
        #region ISeializable
        public void Serialize(Writer writer) {
            writer.Write("ID", Reference?.ID);
            variables.ForEach(v => v.Serialize(writer));
        }
    
        public void Deserialize(Reader reader) {
            string contentID = reader.Read<string>("ID");
            Reference = LevelContent.storage.items.FirstOrDefault(c => c.ID == contentID);
            if (!Reference) return;
            BaseType = Reference.GetContentBaseType();
            Variables().ForEach(v => v.Deserialize(reader));
        }
        #endregion
    }
}