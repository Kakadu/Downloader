using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DownLib {
	public class ShortDictionary<TKey,TValue> {
		int _length;
		Dictionary<TKey,TValue> _dic;
		Queue<TKey> _q;

		public ShortDictionary (int length) {
			_length = length;
			_dic = new Dictionary<TKey, TValue>();
			_q = new Queue<TKey>();
		}

		public void Add (TKey key, TValue v) {
			Debug.Assert (_q.Count <= _length);
			_q.Enqueue (key);
			if (_q.Count > _length)
				_dic.Remove(_q.Dequeue());
			_dic.Add(key,v);
		}
		public bool ContainsKey(TKey k)
		{
			return _dic.ContainsKey(k);
		}

		public TValue this [TKey k] {
			set { Add(k,value); }
			get { return _dic [k];}
		}

		public void Clear()
		{
			_dic.Clear();
		}
	}
}

