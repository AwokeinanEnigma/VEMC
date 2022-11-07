using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TiledSharp
{
	// Token: 0x0200006E RID: 110
	public class TmxList<T> : KeyedCollection<string, T> where T : ITmxElement
	{
		// Token: 0x060003F4 RID: 1012 RVA: 0x00019C7C File Offset: 0x00017E7C
		public new void Add(T t)
		{
			Tuple<TmxList<T>, string> tuple = Tuple.Create<TmxList<T>, string>(this, t.Name);
			if (base.Contains(t.Name))
			{
				Dictionary<Tuple<TmxList<T>, string>, int> dictionary;
				Tuple<TmxList<T>, string> key;
				(dictionary = TmxList<T>.nameCount)[key = tuple] = dictionary[key] + 1;
			}
			else
			{
				TmxList<T>.nameCount.Add(tuple, 0);
			}
			base.Add(t);
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00019CE0 File Offset: 0x00017EE0
		protected override string GetKeyForItem(T t)
		{
			Tuple<TmxList<T>, string> key = Tuple.Create<TmxList<T>, string>(this, t.Name);
			int num = TmxList<T>.nameCount[key];
			int num2 = 0;
			string text = t.Name;
			while (base.Contains(text))
			{
				text = t.Name + string.Concat(Enumerable.Repeat<string>("_", num2)) + num;
				num2++;
			}
			return text;
		}

		// Token: 0x04000293 RID: 659
		public static Dictionary<Tuple<TmxList<T>, string>, int> nameCount = new Dictionary<Tuple<TmxList<T>, string>, int>();
	}
}
