using System.Collections.Generic; 
using System.Diagnostics; 
using System;
using System.Data.Linq;
using System.Linq;

namespace DownLib
{
	public class ConcurrentPriorityQueue<TValue>
	{ 
		private readonly object _syncLock = new object(); 
		private readonly SortedDictionary<int,Queue<TValue>> _innerDic 
			= new SortedDictionary<int,Queue<TValue>>();
		
		public ConcurrentPriorityQueue() {} 

		public void Enqueue(int priority, TValue value)
		{ 
			lock (_syncLock) {
				if (_innerDic.ContainsKey(priority)) {
					var v = _innerDic[priority];
					v.Enqueue(value);
				} else {
					var q = new Queue<TValue>();
					q.Enqueue(value);
					_innerDic.Add(priority, q);
					Console.WriteLine("Enqueue        : {0}/{1}", priority, value);
				}
			}
		} 

		public bool TryDequeue(out TValue result) 
		{ 
			result = default(TValue); 
			lock (_syncLock) 
			{ 
				if (_innerDic.Count > 0) 
				{ 
					int maxkey = _innerDic.Keys.Last();
					Queue<TValue> values = _innerDic[maxkey];
					Debug.Assert(values.Count != 0);
					result = values.Dequeue();
					if (values.Count == 0)
						_innerDic.Remove(maxkey);
					Console.WriteLine("Extracting {0}/{1}",maxkey,result);
					return true; 
				} 
			} 
			return false; 
		} 

		public bool TryPeek(out TValue result) 
		{ 
			result = default(TValue); 
			lock (_syncLock) 
			{ 
				if (_innerDic.Count > 0) 
				{ 
					int maxkey = _innerDic.Keys.Last();
					Queue<TValue> values = _innerDic[maxkey];
					Debug.Assert(values.Count != 0);
					result = values.Peek();
					return true; 
				} 
			} 
			return false; 
		} 
		
		public void Clear() { lock(_syncLock) _innerDic.Clear(); } 
		
		public bool IsEmpty { get { return Count == 0; } } 
		
		public int Count 
		{ 
			get { lock (_syncLock) return _innerDic.Count; } 
		} 

		bool IsSynchronized { get { return true; } } 
		
		object SyncRoot { get { return _syncLock; } } 
	} 
}


