using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics; 
using System.Net;

namespace DownLib {
	public class Loader {
		private static string CACHE_STR = "./Cache";
		//private static Loader _loader;
		private int _threadCount;
		private readonly int MAX_THREAD_COUNT;
		private ShortDictionary<string, byte[]> _dic;
		public delegate void ItemFinishedDelegate(string url, byte[] data);
		//ConcurrentQueue<string> _queue;
		ConcurrentPriorityQueue<string> _queue;
		public event ItemFinishedDelegate ItemFinished;
		/*
		public static Loader Instance { 
			get { 
				if (_loader == null)
					_loader = new Loader();
				return _loader; 
			} 
		}*/
		public Loader(int threadsN) {
			MAX_THREAD_COUNT = threadsN;
			_threadCount = 0;
			_dic = new ShortDictionary<string, byte[]>(10);
			_queue = new ConcurrentPriorityQueue<string>();
		}

		private void runDownload(string url)
		{
			System.Threading.Interlocked.Increment(ref _threadCount);
			WebClient w = new WebClient();
			w.DownloadDataCompleted += onDownloadCompleted;
			w.DownloadDataAsync(new Uri(url), url);
			Console.WriteLine("Getting        : {0}", url);
		}
	
		public void addTarget(string url, int priority)
		{
			if (_dic.ContainsKey(url)) {
				Console.WriteLine("Memory cache   : {0}", url);
				if (ItemFinished != null)
					ItemFinished(url, _dic [url]);
			} else if (_threadCount == MAX_THREAD_COUNT) {
				//Console.WriteLine("Enqueue        : {0}", url);
				_queue.Enqueue(priority, url);
			} else 
				runDownload(url);
		}

		private void cache (string url, byte[] bytes, string path)
		{
			Debug.Assert(bytes != null);
			path = CACHE_STR + "/" + path;
			int i = path.LastIndexOf ('/');
			var dir = path.Substring (0, i);
			System.IO.Directory.CreateDirectory (dir);
			_dic.Add (url, bytes);
			try {
				System.IO.File.WriteAllBytes (path, bytes);
			} catch (NullReferenceException e) {
				throw e;
			}
			Console.WriteLine("Saved          :{0}", path);
		}

		private void evalQueue()
		{
			string url;
			while (true) {
				if (_queue.Count == 0) 
					return;
				
				if (!_queue.TryDequeue(out url)) {
					System.Threading.Thread.Sleep(10);
				} else {
					break;
				}
			}
			Console.WriteLine("Dequeue        : {0}", url);
			runDownload(url);			
		}

		void onDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			System.Threading.Interlocked.Decrement(ref _threadCount);
			evalQueue();

			var url = e.UserState as String;
			var filename = url;
			if (filename.StartsWith("http://"))
				filename = filename.Substring(7);

			try {
				cache(url, e.Result, filename);
				if (ItemFinished != null)
					ItemFinished(url, e.Result);
			} catch (Exception exc) {
				if (exc.InnerException is System.Net.WebException) {
					var wexc = exc.InnerException as System.Net.WebException;
		
					if (wexc.Status == WebExceptionStatus.ProtocolError) {
						if (((HttpWebResponse)wexc.Response).StatusCode == HttpStatusCode.NotFound) {
							Console.WriteLine("Error 404      : {0}", url);
							return;
						}
						if (((HttpWebResponse)wexc.Response).StatusCode == HttpStatusCode.InternalServerError) {
							Console.WriteLine("Error 500      : {0}", url);
							return;
						}
					}
				}
				throw exc;
			}
		}

		#region Clear chache
		public void clearMemoryCache()
		{
			_dic.Clear();
		}

		public void clearDiskCache()
		{
			System.IO.Directory.Delete(CACHE_STR, true);
			System.IO.Directory.CreateDirectory(CACHE_STR);
		}
		#endregion
	}
}
