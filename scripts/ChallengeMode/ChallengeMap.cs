/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;

namespace ChallengeMode {
	/*
	===================================================================================
	
	ChallengeMap
	
	===================================================================================
	*/
	
	public partial class ChallengeMap : Resource {
		[Export]
		public int ChallengeIndex { get; private set; }
		[Export]
		public StringName MapName { get; private set; }

		[Signal]
		public delegate void FinishedLoadingEventHandler();

		private System.Threading.Thread LoadThread;
		private PackedScene MapData;
		private Resource Quest;
		private System.Action<PackedScene, Resource> FinishedLoadingDelegate;

		/*
		===============
		OnFinishedLoading
		===============
		*/
		private void OnFinishedLoading() {
			LoadThread.Join();

			FinishedLoading -= OnFinishedLoading;
			FinishedLoadingDelegate( MapData, Quest );
		}

		/*
		===============
		Load
		===============
		*/
		/// <summary>
		/// Loads a ChallengeMap's data
		/// </summary>
		/// <param name="finishedLoading">The callback function to invoke when the map finishes loading</param>
		public void Load( System.Action<PackedScene, Resource> finishedLoading ) {
			FinishedLoading += OnFinishedLoading;
			FinishedLoadingDelegate = finishedLoading;
			LoadThread = new System.Threading.Thread( () => {
				string dir = string.Format( "res://resources/challenge_maps/" );
				MapData = ResourceLoader.Load<PackedScene>( dir + "map_" + MapName + ".tscn" );
				Quest = ResourceLoader.Load( dir + "objectives/challenge" + ChallengeIndex + ".tres" );
				CallDeferred( MethodName.EmitSignal, SignalName.FinishedLoading );
			} );
			LoadThread.Start();
		}
	};
};