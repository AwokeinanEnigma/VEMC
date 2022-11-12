using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TiledSharp
{
    public class TmxList<T> : KeyedCollection<string, T> where T : ITmxElement
    {
        private Dictionary<string, int> nameCount
            = new Dictionary<string, int>();

        public new void Add(T t)
        {
            var tName = t.Name;

            // Rename duplicate entries by appending a number
            if (this.Contains(tName))
                nameCount[tName] += 1;
            else
                nameCount.Add(tName, 0);
            base.Add(t);
        }

        protected override string GetKeyForItem(T item)
        {
            var name = item.Name;
            var count = nameCount[name];

            var dupes = 0;

            // For duplicate keys, append a counter
            // For pathological cases, insert underscores to ensure uniqueness
            while (Contains(name))
            {
                name = name + String.Concat(Enumerable.Repeat("_", dupes))
                            + count.ToString();
                dupes++;
            }

            return name;
        }
    }
}
