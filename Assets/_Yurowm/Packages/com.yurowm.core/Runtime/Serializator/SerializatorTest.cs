using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Console;
using Yurowm.Extensions;

namespace Yurowm.Serialization {
    public class SerializatorTest {
        
        [QuickCommand("serializator test", null, "test method")]
        public static IEnumerator Test() {
            TestSerializable original = new TestSerializable();
            original.Default();

            string story = Serializator.ToTextData(original);
            yield return story;

            TestSerializable restored = new TestSerializable();
            Serializator.FromTextData(restored, story);

            if (story == Serializator.ToTextData(restored))
                yield return YConsole.Success("Success!");
            else {
                yield return YConsole.Error("Failed!");
                yield return Serializator.ToTextData(restored);
            }
        }
    }
    
    class TestSerializable : ISerializable {
        int _int1, _int2, _int3;
        long _long1, _long2, _long3;
        float _float1, _float2, _float3;
        bool _bool1, _bool2;
        string _string1, _string2, _string3;
        string _strongKey;
        Vector2 _vector2;
        List<string> _listString;
        Dictionary<string, string> _dictString;

        TestSerializableChild _child;

        List<TestSerializableChild> _childList;

        public void Default() {
            _int1 = 24591;
            _int2 = -232;
            _int3 = 0;

            _long1 = 5112415161616126125L;
            _long2 = 0;
            _long3 = -1231516612616160;

            _float1 = 1.23f;
            _float2 = 12f / 10000000000;
            _float3 = -124.55f;

            _bool1 = true;
            _bool2 = false;

            _string1 = "Hello, World";
            _string2 = "Hello {World}";
            _string3 = "Hello\"World!\"";
            
            _strongKey = "_strongKey";

            _vector2 = new Vector2(123f / 10000000, 0.411f);

            _listString = new List<string>() {
                "First", "Second", "Third"
            };

            _dictString = new Dictionary<string, string>() { { "A", "Awesome" }, { "B", "Beautiful" }, { "C", "Cool" }
            };

            _child = new TestSerializableChild();
            _child.Default1();

            _childList = new List<TestSerializableChild>() {
                new TestSerializableChild(),
                new TestSerializableChild(),
                new TestSerializableChild()
            };
            _childList[0].Default1();
            _childList[1].Default2();
            _childList[2].Default3();
        }

        public void Serialize(Writer writer) {
            writer.Write("_int1", _int1);
            writer.Write("_int2", _int2);
            writer.Write("_int3", _int3);

            writer.Write("_long1", _long1);
            writer.Write("_long2", _long2);
            writer.Write("_long3", _long3);

            writer.Write("_float1", _float1);
            writer.Write("_float2", _float2);
            writer.Write("_float3", _float3);

            writer.Write("_bool1", _bool1);
            writer.Write("_bool2", _bool2);

            writer.Write("_string1", _string1);
            writer.Write("_string2", _string2);
            writer.Write("_string3", _string3);
            
            writer.Write("_strong\",Key:{", _strongKey);

            writer.Write("_vector2", _vector2);

            writer.Write("_listString", _listString);

            writer.Write("_dictString", _dictString);

            writer.Write("_child", _child);

            writer.Write("_childList", _childList);
        }

        public bool IsTheSame(TestSerializable obj) {
            if (obj._int1 != _int1) return false;
            if (obj._int2 != _int2) return false;
            if (obj._int3 != _int3) return false;

            if (obj._long1 != _long1) return false;
            if (obj._long2 != _long2) return false;
            if (obj._long3 != _long3) return false;

            if (obj._float1 != _float1) return false;
            if (obj._float2 != _float2) return false;
            if (obj._float3 != _float3) return false;

            if (obj._bool1 != _bool1) return false;
            if (obj._bool2 != _bool2) return false;

            if (obj._string1 != _string1) return false;
            if (obj._string2 != _string2) return false;
            if (obj._string3 != _string3) return false;
            
            if (obj._strongKey != _strongKey) return false;

            if (obj._vector2 != _vector2) return false;

            if (obj._listString == null || _listString == null ||
                obj._listString.Except(_listString).Any() || _listString.Except(obj._listString).Any())
                return false;

            if (!obj._dictString.OrderBy(kvp => kvp.Key)
                .SequenceEqual(_dictString.OrderBy(kvp => kvp.Key)))
                return false;

            if (!obj._child.IsTheSame(_child)) return false;

            if (obj._childList.Count != _childList.Count || 
                !obj._childList[0].IsTheSame(_childList[0]) ||
                !obj._childList[1].IsTheSame(_childList[1]) ||
                !obj._childList[2].IsTheSame(_childList[2]))
                return false;

            return true;
        }

        public void Deserialize(Reader reader) {
            reader.Read("_int1", ref _int1);
            reader.Read("_int2", ref _int2);
            reader.Read("_int3", ref _int3);

            reader.Read("_long1", ref _long1);
            reader.Read("_long2", ref _long2);
            reader.Read("_long3", ref _long3);

            reader.Read("_float1", ref _float1);
            reader.Read("_float2", ref _float2);
            reader.Read("_float3", ref _float3);

            reader.Read("_bool1", ref _bool1);
            reader.Read("_bool2", ref _bool2);

            reader.Read("_string1", ref _string1);
            reader.Read("_string2", ref _string2);
            reader.Read("_string3", ref _string3);
            
            reader.Read("_strong\",Key:{", ref _strongKey);

            reader.Read("_vector2", ref _vector2);

            _listString = reader.ReadCollection<string>("_listString").ToList();

            _dictString = reader.ReadDictionary<string>("_dictString").ToDictionary();

            _child = new TestSerializableChild();
            reader.Deserialize("_child", _child);

            _childList = reader.ReadCollection<TestSerializableChild>("_childList").ToList();
        }
    }

    class TestSerializableChild : ISerializable {
        public string name;
        public int level;
        public List<A> lista = new List<A>();
        public void Default1() {
            name = "Mario";
            level = 12;
        }

        public void Default2() {
            name = "Vito";
            level = 14;

            lista = new List<A>() {
                new A(),
                new A(),
                new A(),
                new A()
            };
        }

        public void Default3() {
            name = "Ann";
            level = 15;
        }

        public void Deserialize(Reader reader) {
            reader.Read("name", ref name);
            reader.Read("level", ref level);
            lista = reader.ReadCollection<A>("listA").ToList();
        }

        public void Serialize(Writer writer) {
            writer.Write("name", name);
            writer.Write("level", level);
            writer.Write("listA", lista);

        }

        public bool IsTheSame(TestSerializableChild obj) {
            if (obj.name != name) return false;
            if (obj.level != level) return false;
            return true;
        }
    }

    class A : ISerializable {
        public string textA = "abc";
        public string textB = "ABC";
        public float number = 123f;

        public void Deserialize(Reader reader) {
            reader.Read("textA", ref textA);
            reader.Read("textB", ref textB);
            reader.Read("number", ref number);
        }

        public void Serialize(Writer writer) {
            writer.Write("textA", textA);
            writer.Write("textB", textB);
            writer.Write("number", number);
        }
    }
}