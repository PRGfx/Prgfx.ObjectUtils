# Prgfx.ObjectUtils
Ports some functionality from [Neos Flow](https://flow.neos.io/) utilities to C#.

## ObjectAccess
(see [Neos.Utility.ObjectHandling](https://github.com/neos/flow-development-collection/tree/master/Neos.Utility.ObjectHandling))
Utility to access properties of an object using getters, offset or (public) properties (or fields) as fit.
```C#
// get a property if available or throw Prgfx.ObjectUtils.PropertyNotAccessibleException
// will check getters like GetPropertyName, HasPropertyName and IsPropertyName if necessary
Prgfx.ObjectUtils.ObjectAccess.GetProperty(subject, "propertyName");
// access non-public properties by forcing access
Prgfx.ObjectUtils.ObjectAccess.GetProperty(subject, "protectedProperty", true);

// get nested values by path
// parts of a path are simply . separated and can access different kinds of data structures
Prgfx.ObjectUtils.ObjectAccess.ObjectPropertyByPath(subject, "list.2.dict.key");
```

## PositionalCollectionSorter
(see [Neos.Utility.Arrays](https://github.com/neos/flow-development-collection/tree/master/Neos.Utility.Arrays))
Utility to primarily get the key of an enumerable data set ordered by a position-string:
```
start (<weight>)
end (<weight>)
before <key> (<weight>)
after <key> (<weight>)
<numerical-order>
```
Here "weight" is the priority of items within a grouping, so `start` and `after` are sorted by descending weight, `end` and `before` by ascending weight.

The position-string will be determined by a property path (accessed with `ObjectAccess`), given as second argument when constructing the sorter and can thus be a nested property.
```c#
var subject = new Dictionary<string, object>{ 
    { "key1", new { position = "start" } },
    { "key2", new { position = "before key1" } },
    { "key3", new { position = "after key2" } }
};
var sorter = new PositionalCollectionSorter(subject, "position");
sorter.GetSortedKeys(); // {"key2", "key3", "key1"}
```
