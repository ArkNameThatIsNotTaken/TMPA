using System.Collections.Generic;
namespace Tmpa {
	public class NullKeyDictionary<Key, Value> {
		private bool containsNullKey_ = false;
		private Value nullKeyValue_ = default(Value);
		private readonly Dictionary<Key, Value> dictionary_ = new Dictionary<Key, Value>();

		public Value this[Key key] {
			get {
				if(key != null) return dictionary_[key];
				if(!containsNullKey_) throw new KeyNotFoundException();
				return nullKeyValue_; }
			set {
				if(key != null) {
					dictionary_[key] = value;
					return; }
				nullKeyValue_ = value;
				containsNullKey_ = true; } }
	
		public Value GetValueOrDefault(Key key) {
			if(key == null) return nullKeyValue_;
			return dictionary_.ContainsKey(key) ? dictionary_[key] : default(Value); } } }