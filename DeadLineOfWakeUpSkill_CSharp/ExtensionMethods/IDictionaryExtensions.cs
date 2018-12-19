using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace DeadLineOfWakeUpSkill_CSharp.ExtensionMethods
{
    static class DictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            return dict.TryGetValue(key, out TValue result) ? result : default(TValue);
        }

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue addValue)
        {
            bool canAdd = !dict.ContainsKey(key);

            if (canAdd)
                dict.Add(key, addValue);

            return canAdd;
        }
    }
}
