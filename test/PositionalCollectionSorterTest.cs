using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Prgfx.ObjectUtils
{

    public class PositionalCollectionSorterTestData : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            // Position end should put element to end
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("second", "end"),
                    TestItem("first1", null),
                }),
                "__meta.position",
                new object[] { "first1", "second" }
            };
            // Position start should put element to start
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("second", null),
                    TestItem("first2", "start"),
                }),
                "__meta.position",
                new object[] { "first2", "second" }
            };
            // Position start should respect priority
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("second", "start 50"),
                    TestItem("first3", "start 52"),
                }),
                "__meta.position",
                new object[] { "first3", "second" }
            };
            // Position end should respect priority
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("second", "end 17"),
                    TestItem("first4", "end"),
                }),
                "__meta.position",
                new object[] { "first4", "second" }
            };
            // Positional numbers are in the middle
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("last", "end"),
                    TestItem("second", 17),
                    TestItem("first5", 5),
                    TestItem("third", 18),
                }),
                "__meta.position",
                new object[] { "first5", "second", "third", "last" }
            };
            // Position before adds before named element if present
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("second", null),
                    TestItem("first6", "before second"),
                }),
                "__meta.position",
                new object[] { "first6", "second" }
            };
            // Position before adds after start if named element is not present
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("third", null),
                    TestItem("second", "before third"),
                    TestItem("first7", "before unknown"),
                }),
                "__meta.position",
                new object[] { "first7", "second", "third" }
            };
            // Position before uses priority when referencing the same element; The higher the priority the closer before the element gets added.
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("third", null),
                    TestItem("second", "before third"),
                    TestItem("first8", "before third 12"),
                }),
                "__meta.position",
                new object[] { "second", "first8", "third" }
            };
            // Position before works recursively
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("third", null),
                    TestItem("second", "before third"),
                    TestItem("first9", "before second"),
                }),
                "__meta.position",
                new object[] { "first9", "second", "third" }
            };
            // Position after adds after named element if present
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("second", "after first10"),
                    TestItem("first10", null),
                }),
                "__meta.position",
                new object[] { "first10", "second" }
            };
            // Position after adds before end if named element is not present
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("third", "end"),
                    TestItem("second", "after unknown"),
                    TestItem("first11", null),
                }),
                "__meta.position",
                new object[] { "first11", "second", "third" }
            };
            // Position after uses priority when referencing the same element; The higher the priority the closer after the element gets added.
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("third", "after first"),
                    TestItem("second", "after first12 12"),
                    TestItem("first12", null),
                }),
                "__meta.position",
                new object[] { "first12", "second", "third" }
            };
            // Position after works recursively
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("third", "after second"),
                    TestItem("second", "after first13"),
                    TestItem("first13", null),
                }),
                "__meta.position",
                new object[] { "first13", "second", "third" }
            };
            // Keys may contain special characters
            yield return new object[] {
                new Dictionary<string, object>(new KeyValuePair<string, object>[]{
                    TestItem("thi:rd", "end"),
                    TestItem("sec.ond", "before thi:rd"),
                    TestItem("fir-st", "before sec.ond"),
                }),
                "__meta.position",
                new object[] { "fir-st", "sec.ond", "thi:rd" }
            };
            yield return new object[] {
                new Dictionary<object, string>(){
                    { 2, "foo" },
                    { 1, "bar" },
                    { "z", "baz" },
                    { "a", "quux" },
                },
                null,
                new object[] { "z", "a", 1, 2 },
            };
        }

        protected KeyValuePair<string, object> TestItem(string key, object position)
        {
            var value = new Dictionary<string, object>(){
                { "__meta", new Dictionary<string, object>() { 
                    { "position", position }
                    }
                }
            };
            return new KeyValuePair<string, object>(
                key,
                position != null ? value : new object()
            );
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class PositionalCollectionSorterTest
    {
        [Theory]
        [ClassData(typeof(PositionalCollectionSorterTestData))]
        public void GetSortedKeysTest(IEnumerable subject, string positionPropertyPath, object[] expected)
        {
            var sorter = new PositionalCollectionSorter(subject, positionPropertyPath);
            var result = sorter.GetSortedKeys();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ReadmeTest()
        {
            var subject = new Dictionary<string, object>{ 
                { "key1", new { position = "start" } },
                { "key2", new { position = "before key1" } },
                { "key3", new { position = "after key2" } }
            };
            var sorter = new PositionalCollectionSorter(subject);
            Assert.Equal(new object[]{"key2", "key3", "key1"}, sorter.GetSortedKeys());
        }
    }
}