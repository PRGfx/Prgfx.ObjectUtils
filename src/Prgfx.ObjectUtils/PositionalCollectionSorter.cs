using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Prgfx.ObjectUtils
{
    public class PositionalCollectionSorter
    {
        /// <summary>
        /// As "after {key}" etc. keep the keys as string, we will handle them as such internally
        /// </summary>
        protected Dictionary<string, object> Subject;

        /// <summary>
        /// To handle keys internally more easily, we handle them as strings, but want to return them
        /// (and access the subject's values) with the correct key type
        /// </summary>
        protected Dictionary<string, object> KeyAlias;

        /// <summary>
        /// Runtime cache with the result of the calculation
        /// </summary>
        protected string[] sortedKeysInternal;

        /// <summary>
        /// Flag if the runtime cache can be used
        /// </summary>
        protected bool initialized;

        /// <summary>
        /// Property path to access the position value from in an item
        /// </summary>
        protected string PositionalPropertyPath;

        /// <summary>
        /// All keys with position "start ...", grouped by weight (or 0)
        /// </summary>
        protected Dictionary<int, List<string>> StartKeys;

        /// <summary>
        /// All keys without position or positioned through their index in the subject
        /// </summary>
        protected Dictionary<int, List<string>> MiddleKeys;

        /// <summary>
        /// All keys with position "end ...", grouped by weight (or 0)
        /// </summary>
        protected Dictionary<int, List<string>> EndKeys;

        /// <summary>
        /// All keys with position "before {key} ..." grouped by {key} and weight (or 0)
        /// </summary>
        protected Dictionary<string, Dictionary<int, List<string>>> BeforeKeys;

        /// <summary>
        /// All keys with position "after {key} ..." grouped by {key} and weight (or 0)
        /// </summary>
        protected Dictionary<string, Dictionary<int, List<string>>> AfterKeys;

        protected Regex PatternStart = new Regex(@"start(?: ([0-9]+))?");

        protected Regex PatternEnd = new Regex(@"end(?: ([0-9]+))?");

        protected Regex PatternBefore = new Regex(@"before (\S+)(?: ([0-9]+))?");

        protected Regex PatternAfter = new Regex(@"after (\S+)(?: ([0-9]+))?");

        /// <summary>
        /// Allows to sort items in an enumerable by a defined property on a {position-string}
        /// 
        /// The {position-string} supports one of the following syntax:
        ///   start ({weight})
        ///   end ({weight})
        ///   before {key} ({weight})
        ///   after {key} ({weight})
        ///   {numerical-order}
        /// 
        /// where "weight" is the priority that defines which of two conflicting positions overrules the other,
        /// "key" is a string that references another key in the $subject and "numerical-order" is an integer
        /// that defines the order independently from the other keys.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="positionalPropertyPath"></param>
        public PositionalCollectionSorter(IEnumerable subject, string positionalPropertyPath = "position")
        {
            this.KeyAlias = new Dictionary<string, object>();
            this.Subject = new Dictionary<string, object>();
            var n = 0;
            foreach (var item in subject)
            {
                if (item == null)
                {
                    continue;
                }
                string key;
                var valueType = item.GetType();
                if (valueType.IsGenericType)
                {
                    var baseType = valueType.GetGenericTypeDefinition();
                    if (baseType == typeof(KeyValuePair<,>))
                    {
                        var kvpKey = valueType.GetProperty("Key").GetValue(item, null);
                        var kvpValue = valueType.GetProperty("Value").GetValue(item, null);
                        key = kvpKey.ToString();
                        this.Subject[key] = kvpValue;
                        this.KeyAlias.Add(key, kvpKey);
                        continue;
                    }
                }
                key = n.ToString();
                this.Subject[key] = item;
                this.KeyAlias.Add(key, n);
                n++;
            }
            this.PositionalPropertyPath = positionalPropertyPath;
            BeforeKeys = new Dictionary<string, Dictionary<int, List<string>>>();
            AfterKeys = new Dictionary<string, Dictionary<int, List<string>>>();
            MiddleKeys = new Dictionary<int, List<string>>();
            StartKeys = new Dictionary<int, List<string>>();
            EndKeys = new Dictionary<int, List<string>>();
        }

        public SortedDictionary<object, object> ToDictionary()
        {
            var sortedKeys = GenerateSortedKeys();
            var sortedDict = new SortedDictionary<object, object>();
            foreach (var key in sortedKeys)
            {
                sortedDict.Add(key, this.Subject[key]);
            }
            return sortedDict;
        }

        /// <summary>
        /// Return the keys of the subject to then iterate over to access the subject's items in sorted order.
        /// </summary>
        /// <returns></returns>
        public object[] GetSortedKeys()
        {
            var sortedKeys = GenerateSortedKeys();
            return sortedKeys.Select(s => KeyAlias[s]).ToArray();
        }

        public string[] GenerateSortedKeys()
        {
            if (initialized)
            {
                return this.sortedKeysInternal;
            }
            IndexKeys();
            var resultStart = new List<string>();
            var resultMiddle = new List<string>();
            var resultEnd = new List<string>();
            var processedKeys = new List<string>();
            Func<Dictionary<int, List<string>>, bool, int[]> sortedWeights = (Dictionary<int, List<string>> dict, bool asc) =>
                asc ? dict.Keys.OrderBy(k => (int)k).ToArray() : dict.Keys.OrderByDescending(k => (int)k).ToArray();
            Action<List<string>, List<string>> addKeys = null;
            addKeys = (List<string> keys, List<string> result) =>
            {
                foreach (var sKey in keys)
                {
                    if (this.BeforeKeys.ContainsKey(sKey))
                    {
                        var beforeKeys = this.BeforeKeys[sKey];
                        foreach (var i in sortedWeights(beforeKeys, true))
                        {
                            addKeys(beforeKeys[i], result);
                        }
                    }
                    result.Add(sKey);
                    if (this.AfterKeys.ContainsKey(sKey))
                    {
                        var afterKeys = this.AfterKeys[sKey];
                        foreach (var i in sortedWeights(afterKeys, false))
                        {
                            addKeys(afterKeys[i], result);
                        }
                    }
                    processedKeys.Add(sKey);
                }
            };
            foreach (var i in sortedWeights(this.StartKeys, false))
            {
                addKeys(this.StartKeys[i], resultStart);
            }
            foreach (var i in sortedWeights(this.MiddleKeys, true))
            {
                addKeys(this.MiddleKeys[i], resultMiddle);
            }
            foreach (var i in sortedWeights(this.EndKeys, true))
            {
                addKeys(this.EndKeys[i], resultEnd);
            }

            // orphaned keys
            foreach (var kvp in this.BeforeKeys)
            {
                var referenceKey = kvp.Key;
                if (processedKeys.Contains(referenceKey))
                {
                    continue;
                }
                foreach (var i in sortedWeights(this.BeforeKeys[referenceKey], false))
                {
                    addKeys(this.BeforeKeys[referenceKey][i], resultStart);
                }
            }
            foreach (var kvp in this.AfterKeys)
            {
                var referenceKey = kvp.Key;
                if (processedKeys.Contains(referenceKey))
                {
                    continue;
                }
                foreach (var i in sortedWeights(this.AfterKeys[referenceKey], false))
                {
                    addKeys(this.AfterKeys[referenceKey][i], resultMiddle);
                }
            }

            this.sortedKeysInternal = resultStart.Concat(resultMiddle).Concat(resultEnd).ToArray();
            return this.sortedKeysInternal;
        }

        /// <summary>
        /// Get all items' "position" property and index the item keys accordingly. From that index the sorted keys will be composed later.
        /// </summary>
        protected void IndexKeys()
        {
            foreach (var kvp in this.KeyAlias)
            {
                var sKey = kvp.Key;
                var oKey = kvp.Value;
                var value = this.Subject[sKey];
                if (value == null)
                {
                    continue;
                }
                var positionValue = ObjectAccess.ObjectPropertyByPath(value, PositionalPropertyPath);
                var position = positionValue == null ? null : positionValue.ToString();
                if (position != null && position.StartsWith("start"))
                {
                    if (IndexStartKey(sKey, position))
                    {
                        continue;
                    }
                }
                else if (position != null && position.StartsWith("end"))
                {
                    if (IndexEndKey(sKey, position))
                    {
                        continue;
                    }
                }
                else if (position != null && position.StartsWith("after"))
                {
                    if (IndexAfterKey(sKey, position))
                    {
                        continue;
                    }
                }
                else if (position != null && position.StartsWith("before"))
                {
                    if (IndexBeforeKey(sKey, position))
                    {
                        continue;
                    }
                }
                else
                {
                    IndexMiddleKey(sKey, position, oKey);
                    continue;
                }
                // invalid keyword-key
                IndexMiddleKey(sKey, null, oKey);
            }
        }

        private bool IndexStartKey(string sKey, string position)
        {
            Match m = PatternStart.Match(position);
            if (!m.Success)
            {
                return false;
            }
            int weight = 0;
            var weightValue = m.Groups[1].Value;
            if (!string.IsNullOrEmpty(weightValue))
            {
                int.TryParse(weightValue, out weight);
            }
            if (!StartKeys.ContainsKey(weight))
            {
                StartKeys[weight] = new List<string>();
            }
            StartKeys[weight].Add(sKey);
            return true;
        }

        private bool IndexEndKey(string sKey, string position)
        {
            Match m = PatternEnd.Match(position);
            if (!m.Success)
            {
                return false;
            }
            int weight = 0;
            var weightValue = m.Groups[1].Value;
            if (!string.IsNullOrEmpty(weightValue))
            {
                int.TryParse(weightValue, out weight);
            }
            if (!EndKeys.ContainsKey(weight))
            {
                EndKeys[weight] = new List<string>();
            }
            EndKeys[weight].Add(sKey);
            return true;
        }

        private void IndexMiddleKey(string sKey, string position, object oKey)
        {
            int index;
            if (position == null || !int.TryParse(position, out index))
            {
                index = 0;
            }
            if (oKey is int)
            {
                index = (int)oKey;
            }
            if (!MiddleKeys.ContainsKey(index))
            {
                MiddleKeys[index] = new List<string>();
            }
            MiddleKeys[index].Add(sKey);
        }

        private bool IndexBeforeKey(string sKey, string position)
        {
            Match m = PatternBefore.Match(position);
            if (!m.Success)
            {
                return false;
            }
            var reference = m.Groups[1].Value;
            int weight = 0;
            var weightValue = m.Groups[2].Value;
            if (!string.IsNullOrEmpty(weightValue))
            {
                int.TryParse(weightValue, out weight);
            }
            if (!BeforeKeys.ContainsKey(reference))
            {
                BeforeKeys[reference] = new Dictionary<int, List<string>>();
            }
            if (!BeforeKeys[reference].ContainsKey(weight))
            {
                BeforeKeys[reference][weight] = new List<string>();
            }
            BeforeKeys[reference][weight].Add(sKey);
            return true;
        }

        private bool IndexAfterKey(string sKey, string position)
        {
            Match m = PatternAfter.Match(position);
            if (!m.Success)
            {
                return false;
            }
            var reference = m.Groups[1].Value;
            int weight = 0;
            var weightValue = m.Groups[2].Value;
            if (!string.IsNullOrEmpty(weightValue))
            {
                int.TryParse(weightValue, out weight);
            }
            if (!AfterKeys.ContainsKey(reference))
            {
                AfterKeys[reference] = new Dictionary<int, List<string>>();
            }
            if (!AfterKeys[reference].ContainsKey(weight))
            {
                AfterKeys[reference][weight] = new List<string>();
            }
            AfterKeys[reference][weight].Add(sKey);
            return true;
        }
    }
}