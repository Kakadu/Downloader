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
		private readonly SortedDictionary<int,Queue<TValue>> _minHeap 
			= new SortedDictionary<int,Queue<TValue>>();
		
		public ConcurrentPriorityQueue() {} 

		public void Enqueue(int priority, TValue value)
		{ 
			lock (_syncLock) {
				if (_minHeap.ContainsKey(priority)) {
					var v = _minHeap[priority];
					v.Enqueue(value);
				} else {
					var v = new Queue<TValue>();
					v.Enqueue(value);
					_minHeap.Add(priority, v);
				}
			}
		} 

		public bool TryDequeue(out TValue result) 
		{ 
			result = default(TValue); 
			lock (_syncLock) 
			{ 
				if (_minHeap.Count > 0) 
				{ 
					//int maxkey = _minHeap.Keys[_minHeap.Count-1];
					int maxkey = _minHeap.Keys.Last();
					Queue<TValue> values = _minHeap[maxkey];
					Debug.Assert(values.Count != 0);
					result = values.Dequeue();
					if (values.Count == 0)
						_minHeap.Remove(maxkey);
					//_minHeap.Add(maxkey, values);
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
				if (_minHeap.Count > 0) 
				{ 
					int maxkey = _minHeap.Keys.Last();
					Queue<TValue> values = _minHeap[maxkey];
					Debug.Assert(values.Count != 0);
					result = values.Peek();
					return true; 
				} 
			} 
			return false; 
		} 
		
		public void Clear() { lock(_syncLock) _minHeap.Clear(); } 
		
		public bool IsEmpty { get { return Count == 0; } } 
		
		public int Count 
		{ 
			get { lock (_syncLock) return _minHeap.Count; } 
		} 

		bool IsSynchronized { get { return true; } } 
		
		object SyncRoot { get { return _syncLock; } } 
	} 
}


