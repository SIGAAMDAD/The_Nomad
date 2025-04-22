using Godot;
using System.Collections.Generic;

namespace Renown.World {
	public class ThinkerFactory {
//		private static List<long> Threads = new List<long>();

		private static void CreateThinker( Settlement location, int specificAge ) {
			location.AddThinker( Thinker.Create( location, specificAge ) );
		}
		public static void QueueThinker( Settlement location, int specificAge = -1 ) {
			CreateThinker( location, specificAge );
//			Threads.Add( WorkerThreadPool.AddTask( Callable.From( () => { CreateThinker( location ); } ) ) );
		}
		/*
		public static void WaitForFinished() {
			for ( int i = 0; i < Threads.Count; i++ ) {
				WorkerThreadPool.WaitForTaskCompletion( Threads[i] );
			}
			Threads.Clear();
		}
		*/
	};
};