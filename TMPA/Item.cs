using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Tmpa {
	public static class Item {
		public static IReadOnlyList<Item> GetCache<Item>(this IEnumerable<Item> items) { return items.ToList(); }

		public static IEnumerable<int> IndexEachEqual<Item>(this IEnumerable<Item> items, Item item) { return items.Select((item2, index) => Equals(item2, item) ? index : -1).Nonequal(-1); }

		public static IEnumerable<Item> Equal<Item>(this IEnumerable<Item> items, Item item) { return items.Where(item2 => Equals(item2, item)); }
		public static IEnumerable<Item> Nonequal<Item>(this IEnumerable<Item> items, Item item) { return items.Where(item2 => !Equals(item2, item)); }

		public static IEnumerable<Item> ValueEqual<Item, Value>(this IEnumerable<Item> items, Func<Item, Value> getValue, Value value) { return items.Where(item => Equals(getValue(item), value)); }
		
		public static int IndexOf<Item>(this IEnumerable<Item> items, Item item, int indexMinimum = 0) {
			var index = -1;
			foreach(var item2 in items) {
				index++;
				if(index < indexMinimum) continue;
				if(Equals(item2, item)) return index; }
			return -1; }

		public static Item ItemAt<Item>(this IReadOnlyList<Item> items, int index) { return items[index]; }

		public static bool ContainsDuplicates<Item>(this IEnumerable<Item> items) { return items.GetCache().ContainsDuplicates(); }
		public static bool ContainsDuplicates<Item>(this IReadOnlyList<Item> items) { return items.Distinct().Count() != items.Count; }

		public static string Join(this IEnumerable items, string separator) { return string.Join(separator, items.Cast<object>().ToArray()); } } }