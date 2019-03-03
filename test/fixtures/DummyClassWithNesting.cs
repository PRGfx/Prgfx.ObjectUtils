using System.Collections.Generic;

namespace Prgfx.ObjectUtils.Fixtures
{
    class DummyClassWithNesting
    {

        public NestedDummyClass nestedClass;

        public Dictionary<string, object> nestedDictionary;

        public List<string> nestedList;

        public string[] nestedArray;

        public DummyClassWithNesting()
        {
            this.nestedClass = new NestedDummyClass();
            this.nestedDictionary = new Dictionary<string, object>() {
                { "key1", "value 1" },
                { "1", new string[]{ "value1", "value2" } },
            };
            this.nestedList = new List<string>() { "foo", "bar", "baz" };
            this.nestedArray = new string[]{ "ab", "cde", "fgh" };
        }

        public class NestedDummyClass
        {
            public string innerValue = "inner value";
        }
    }
}