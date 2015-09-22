using System;
using System.Collections.Generic;

namespace Woof.Core {

    /// <summary>
    /// A dictionary with several missing but useful methods added
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class XDictionary<TKey, TValue> : Dictionary<TKey, TValue> {

        /// <summary>
        /// Adds specific key and value if specified key doesn't exist in dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddIfNotExists(TKey key, TValue value) {
            if (!ContainsKey(key)) Add(key, value);
        }

        /// <summary>
        /// Performs the specified action on each element of the XDictionary
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<KeyValuePair<TKey, TValue>> action) {
            foreach (var p in this) action(p);
        }

        /// <summary>
        /// Returns all keys for which the value is equal to the specified value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<TKey> KeysMatchingByValue(TValue value) {
            var keys = new List<TKey>();
            ForEach(p => { if (p.Value.Equals(value)) keys.Add(p.Key); });
            return keys;
        }

        /// <summary>
        /// Returns first key matched for the specified value or default key type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TKey FirstOrDefaultKeyFor(TValue value) {
            foreach (var p in this) if (p.Value.Equals(value)) return p.Key;
            return default(TKey);
        }

        /// <summary>
        /// If key doesnt exists, it's added, if it exists, its replaced with new value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void InsertOrUpdate(TKey key, TValue value) {
            if (ContainsKey(key)) this[key] = value;
            else Add(key, value);
        }

        /// <summary>
        /// Each key containing specified value will be removed and a new key with specified value will be added
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ReplaceValue(TKey key, TValue value) {
            KeysMatchingByValue(value).ForEach(k => Remove(k));
            InsertOrUpdate(key, value);
        }

    }

}