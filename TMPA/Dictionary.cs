using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Tmpa {
	public static class Dictionary {
		public static Dictionary<Key, List<Value>> GetNewListDictionary<Key, Value>(this IEnumerable<Value> values, Func<Value, Key> getKey) { return values.GetNewListDictionary(getKey, getValue: value => value); }
		public static Dictionary<Key, List<Value>> GetNewListDictionary<Item, Key, Value>(this IEnumerable<Item> items, Func<Item, Key> getKey, Func<Item, Value> getValue) {
			var dictionary = new Dictionary<Key, List<Value>>();
			foreach(var item in items) dictionary.GetValueOrCreateValue(getKey(item)).Add(getValue(item));
			return dictionary; }

		public static Value GetValue<Key, Value>(this IReadOnlyDictionary<Key, Value> dictionary, Key key) { return dictionary[key]; }
		public static Item GetValueOrKey<Item>(this IReadOnlyDictionary<Item, Item> dictionary, Item key) {
			Item value;
			return dictionary.TryGetValue(key, out value) ? value : key; }
		public static Value GetValueOrCreateValue<Key, Value>(this IDictionary<Key, Value> dictionary, Key key) where Value : new() {
			Value value;
			if(dictionary.TryGetValue(key, out value)) return value;
			value = new Value();
			dictionary.Add(key, value);
			return value; }
	
		public static void AddBidirectionally<Item>(this IDictionary<Item, Item> dictionary, Item key, Item value) {
			dictionary.Add(key, value);
			dictionary.Add(value, key); }

		public static void AddChain<Item>(this IDictionary<Item, Item> dictionary, Item[] chain) {
			var nextWrappingValueIndex = 1;
			foreach(var key in chain) dictionary.Add(key, chain[nextWrappingValueIndex++ % chain.Length]); } } }