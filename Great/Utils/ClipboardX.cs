using System;
using System.Collections.Generic;

namespace Great2.Utils
{
    public static class ClipboardX
    {
        private static Dictionary<string, object> items = new Dictionary<string, object>();

        public static bool AddItem(string key, object value)
        {
            if (items.ContainsKey(key))
                return false;

            items.Add(key, value);
            return true;
        }

        public static T GetItem<T>(string key)
        {
            if (items.ContainsKey(key))
            {
                object item = items[key];

                if (item is T)
                    return (T)item;
                else
                    return (T)Convert.ChangeType(item, typeof(T));
            }

            return default(T);
        }

        public static bool Contains(string key)
        {
            return items.ContainsKey(key);
        }

        public static bool Contains(object value)
        {
            return items.ContainsValue(value);
        }

        public static void Clear()
        {
            items.Clear();
        }
    }
}
