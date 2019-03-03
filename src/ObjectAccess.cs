using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Prgfx.ObjectUtils
{
    public class ObjectAccess
    {
        protected static Dictionary<Type, string[]> gettablePropertyNamesCache;

        protected static Dictionary<string, string[]> propertyGetterCache;

        public static object ObjectPropertyByPath(object subject, string path)
        {
            if (path == null) {
                return null;
            }
            var pathSegments = path.Split('.');
            foreach (var pathSegment in pathSegments)
            {
                var propertyExists = false;
                var propertyValue = GetPropertyInternal(subject, pathSegment, false, out propertyExists);
                if (propertyExists) {
                    subject = propertyValue;
                } else {
                    return null;
                }
            }
            return subject;
        }

        private static object GetPropertyInternal(object subject, string propertyName, bool forceDirectAccess, out bool propertyExists)
        {
            propertyExists = false;
            if (subject == null)
            {
                return null;
            }
            if (subject is System.Collections.IDictionary)
            {
                propertyExists = ((System.Collections.IDictionary)subject).Contains(propertyName);
                if (propertyExists)
                {
                    return ((System.Collections.IDictionary)subject)[propertyName];
                }
                return null;
            }
            if (int.TryParse(propertyName, out int pathKey) && (subject is System.Collections.ICollection))
            {
                propertyExists = ((System.Collections.ICollection)subject).Count > pathKey;
                if (propertyExists)
                {
                    var i = 0;
                    foreach (var item in (System.Collections.ICollection)subject)
                    {
                        if (i++ == pathKey)
                        {
                            return item;
                        }
                    }
                }
            }
            try
            {
                var cacheIdentifier = subject.GetType().ToString() + "|" + propertyName;
                InitializePropertyGetterCache(cacheIdentifier, subject, propertyName);
                var accessFlags = BindingFlags.Public | BindingFlags.Instance;
                if (forceDirectAccess)
                {
                    accessFlags |= BindingFlags.NonPublic;
                }
                if (!string.IsNullOrEmpty(propertyGetterCache[cacheIdentifier][1])) {
                    var getter = subject.GetType().GetMethod(propertyGetterCache[cacheIdentifier][1], accessFlags);
                    if (getter != null)
                    {
                        propertyExists = true;
                        return getter.Invoke(subject, null);
                    }
                }
                var field = subject.GetType().GetField(propertyName, accessFlags);
                if (field != null)
                {
                    propertyExists = true;
                    return field.GetValue(subject);
                }
                var property = subject.GetType().GetProperty(propertyName, accessFlags);
                if (property != null)
                {
                    propertyExists = true;
                    return property.GetValue(subject);
                }
            }
            catch (PropertyNotAccessibleException)
            {
            }
            return null;
        }

        protected static void InitializePropertyGetterCache(string cacheIdentifier, object subject, string propertyName)
        {
            if (propertyGetterCache == null)
            {
                propertyGetterCache = new Dictionary<string, string[]>();
            }
            if (propertyGetterCache.ContainsKey(cacheIdentifier))
            {
                return;
            }
            propertyGetterCache[cacheIdentifier] = new string[2];
            var upperCasePropertyName = UcFirst(propertyName);
            string[] getterMethodNames = { "get" + upperCasePropertyName, "is" + upperCasePropertyName, "has" + upperCasePropertyName };
            foreach (var getterMethodName in getterMethodNames)
            {
                if (subject.GetType().GetMethod(getterMethodName) != null)
                {
                    propertyGetterCache[cacheIdentifier] = new string[] { null, getterMethodName };
                    return;
                }
                var UcfGetterMethodName = UcFirst(getterMethodName);
                if (subject.GetType().GetMethod(UcfGetterMethodName) != null)
                {
                    propertyGetterCache[cacheIdentifier] = new string[] { null, UcfGetterMethodName };
                    return;
                }
            }
            if (!(subject is System.Collections.IDictionary))
            {
                return;
            }
            var dict = (System.Collections.IDictionary)subject;
            foreach (var k in dict.Keys)
            {
                if (k.ToString() == propertyName)
                {
                    propertyGetterCache[cacheIdentifier] = new string[] { propertyName, null };
                    return;
                }
            }
        }

        public static string[] GetGettablePropertyNames(object subject)
        {
            if (gettablePropertyNamesCache == null)
            {
                gettablePropertyNamesCache = new Dictionary<Type, string[]>();
            }
            if (!gettablePropertyNamesCache.ContainsKey(subject.GetType()))
            {
                var result = new List<string>();
                foreach (var methodInfo in subject.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var methodName = methodInfo.Name;
                    var methodNameLength = methodName.Length;
                    if (methodNameLength > 2 && methodName.Substring(0, 2).ToLower() == "is")
                    {
                        result.Add(LcFirst(methodName.Substring(2)));
                    }
                    else if (methodNameLength > 3)
                    {
                        var methodNamePrefix = methodName.Substring(0, 3).ToLower();
                        if (methodNamePrefix == "get" || methodNamePrefix == "has")
                        {
                            result.Add(LcFirst(methodName.Substring(3)));
                        }
                    }
                }
                var propertyNames = result.Distinct();
                gettablePropertyNamesCache[subject.GetType()] = propertyNames.ToArray();
            }
            return gettablePropertyNamesCache[subject.GetType()];
        }

        private static string LcFirst(string v)
        {
            return v[0].ToString().ToLower() + v.Substring(1);
        }

        private static string UcFirst(string v)
        {
            return v[0].ToString().ToUpper() + v.Substring(1);
        }

        public static object GetProperty(object subject, string propertyName, bool forceDirectAccess = false)
        {
            var propertyExists = false;
            var propertyValue = GetPropertyInternal(subject, propertyName, forceDirectAccess, out propertyExists);
            if (propertyExists)
            {
                return propertyValue;
            }
            throw new PropertyNotAccessibleException($"The property \"{propertyName}\" on the subject was not accessible.");
        }
    }
}
