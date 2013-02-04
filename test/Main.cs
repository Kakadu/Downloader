using System;
using System.Collections.Generic;
using DownLib;

namespace test {
	class MainClass {
		static Loader loader = new Loader(5);

		public static void Main (string[] args)
		{
			var targets = new List<String> ();
			for (int i=0; i<10; i++) {
				var url = 
					String.Format ("http://thumbs.wallbase.cc/rozne/thumb-{0}.jpg", 43795 + i);
				targets.Add (url);
			}

			foreach (var url in targets) {
				loader.addTarget (url);
			}
			System.Threading.Thread.Sleep(2000);
			Console.WriteLine("");
			foreach (var url in targets) {
				loader.addTarget (url);
			}

			while (true) {
				System.Threading.Thread.Sleep(1000);
			}
		}
	}
}
