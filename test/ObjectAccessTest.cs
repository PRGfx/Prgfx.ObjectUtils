using System.Collections.Generic;
using Xunit;

namespace Prgfx.ObjectUtils
{
    public class ObjectAccessTest
    {

        Fixtures.DummyClassWithGettersAndSetters dummyObject;

        Fixtures.DummyClassWithNesting dummyObject2;

        public ObjectAccessTest()
        {
            dummyObject = new Fixtures.DummyClassWithGettersAndSetters();
            dummyObject.SetProperty("string1");
            dummyObject.SetAnotherProperty(42);

            dummyObject2 = new Fixtures.DummyClassWithNesting();
        }

        [Fact]
        public void GetPropertyReturnsExpectedValueForGetterProperty()
        {
            var property = ObjectAccess.GetProperty(dummyObject, "property");
            Assert.Equal("string1", property);
        }

        [Fact]
        public void GetPropertyReturnsExpectedValueForPublicProperty()
        {
            var property = ObjectAccess.GetProperty(dummyObject, "publicProperty2");
            Assert.Equal(42, property);
        }

        [Fact]
        public void GetPropertyReturnsExpectedValueForUnexposedPropertyIfForceDirectAccessIsTrue()
        {
            var property = ObjectAccess.GetProperty(dummyObject, "unexposedProperty", true);
            Assert.Equal("unexposed", property);
        }

        
        [Fact(Skip="Inapplicable")]
        public void GetPropertyRetrunsExpectedValueForUnknownPropertyIfForceDirectAccessIsTrue()
        {
        }

        [Fact]
        public void GetPropertyReturnsPropertyNotAccessibleExcpeitonForNotExistingPropertyIfForceDirectAccessIsTrue()
        {
            Assert.Throws<PropertyNotAccessibleException>(() => ObjectAccess.GetProperty(dummyObject, "notExistingProperty", true));
        }

        [Fact]
        public void GetPropertyReturnsPropertyNotAccessibleExcpeitonIfPropertyDoesNotExist()
        {
            Assert.Throws<PropertyNotAccessibleException>(() => ObjectAccess.GetProperty(dummyObject, "notExistingProperty"));
        }

        [Fact]
        public void GetPropertyReturnsThrowsExceptionIfArrayKeyDoesNotExist()
        {
            Assert.Throws<PropertyNotAccessibleException>(() => ObjectAccess.GetProperty(new Dictionary<string, object>(), "notExistingProperty"));
        }

        [Fact]
        public void GetPropertyTriesToCallABooleanIsGetterMethodIfExists()
        {
            var property = ObjectAccess.GetProperty(dummyObject, "booleanProperty");
            Assert.Equal("method called True", property);
        }

        [Fact]
        public void GetPropertyTriesToCallABooleanHasGetterMethodIfItExists()
        {
            var property = ObjectAccess.GetProperty(dummyObject, "anotherBooleanProperty");
            Assert.Equal(false, property);

            dummyObject.SetAnotherBooleanProperty(true);
            property = ObjectAccess.GetProperty(dummyObject, "anotherBooleanProperty");
            Assert.Equal(true, property);
        }

        [Fact(Skip="Inapplicable")]
        public void GetPropertyThrowsExceptionIfThePropertyNameIsNotAString()
        {
        }

        [Fact]
        public void GetPropertyCanAccessPropertiesOfADictObject()
        {
            var dict = new Dictionary<string, object>();
            dict["key"] = "value";
            var result = ObjectAccess.GetProperty(dict, "key");
            Assert.Equal("value", result);
        }

        [Fact(Skip="Is this really the desired behavior?")]
        public void GetPropertyCallsGettersBeforeAccessingFields()
        {
            var dict = new Dictionary<string, object>();
            dict["Count"] = "value";
            var result = ObjectAccess.GetProperty(dict, "Count");
            Assert.Equal(dict.Count, result);
        }

        [Fact]
        public void GetPropertyThrowsExceptionIfDictDoesNotcontainKey()
        {
            var dict = new Dictionary<string, object>();
            Assert.Throws<PropertyNotAccessibleException>(() => ObjectAccess.GetProperty(dict, "notExistingKey"));
        }

        [Fact]
        public void GetPropertyCanAccessElementsOfAnArray()
        {
            var arr = new string[]{"foo", "bar", "baz"};
            Assert.Equal("bar", ObjectAccess.GetProperty(arr, "1"));
        }

        [Fact]
        public void GetPropertyCanAccessElementsOfAList()
        {
            var arr = new List<string>(){"foo", "bar", "baz"};
            Assert.Equal("bar", ObjectAccess.GetProperty(arr, "1"));
        }

        [Fact]
        public void GetPropertyThrowsExceptionIfListIsOutOfBounds()
        {
            var arr = new List<string>(){"foo", "bar", "baz"};
            Assert.Throws<PropertyNotAccessibleException>(() => ObjectAccess.GetProperty(arr, "4"));
        }

        [Fact]
        public void GetPropertyPathCanGetPropertiesOfAnObject()
        {
            Assert.Equal("inner value", ObjectAccess.ObjectPropertyByPath(dummyObject2, "nestedClass.innerValue"));
            Assert.Equal("value 1", ObjectAccess.ObjectPropertyByPath(dummyObject2, "nestedDictionary.key1"));
            Assert.Equal("value1", ObjectAccess.ObjectPropertyByPath(dummyObject2, "nestedDictionary.1.0"));
            Assert.Equal("baz", ObjectAccess.ObjectPropertyByPath(dummyObject2, "nestedList.2"));
            Assert.Equal("ab", ObjectAccess.ObjectPropertyByPath(dummyObject2, "nestedArray.0"));
        }

        [Fact]
        public void GetPropertyPathReturnsNullForNonExistingPropertyPath()
        {
            Assert.Null(ObjectAccess.ObjectPropertyByPath(dummyObject2, "nestedClass.foo"));
        }

        [Fact]
        public void GetPropertyPathReturnsNullIfSubjectIsNoObject()
        {
            var str = "Hello world";
            Assert.Null(ObjectAccess.ObjectPropertyByPath(str, "property2"));
            Assert.Null(ObjectAccess.ObjectPropertyByPath(str, "0"));
        }

        [Fact]
        public void GetProeprtyPathReturnsNullIfSubjectOnPathIsNoObject()
        {
            var subject = new Dictionary<string, object>() { {"foo", "Hello World"} };
            Assert.Null(ObjectAccess.ObjectPropertyByPath(subject, "foo.bar"));
        }
    }
}