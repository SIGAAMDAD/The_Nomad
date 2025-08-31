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
using ResourceCache;

namespace Multiplayer {
	/*
	===================================================================================
	
	Announcer
	
	===================================================================================
	*/
	
	public partial class Announcer : Node {
		private static AudioStreamPlayer AudioChannel;

		private static void Announce( AudioStream audio ) {
			AudioChannel.Stream = audio;
			AudioChannel.Play();
		}

		public static void PrepareToFight() => Announce( AudioCache.GetStream( "res://sounds/announcer/prepare.wav" ) );
		public static void TakenLead() => Announce( AudioCache.GetStream( "res://sounds/announcer/takenlead.wav" ) );
		public static void TiedLead() => Announce( AudioCache.GetStream( "res://sounds/announcer/tiedlead.wav" ) );
		public static void LostLead() => Announce( AudioCache.GetStream( "res://sounds/announcer/lostlead.wav" ) );
		public static void Fight() => Announce( AudioCache.GetStream( "res://sounds/announcer/fight.wav" ) );
		public static void BlueFlagReturned() => Announce( AudioCache.GetStream( "res://sounds/announcer/blueflagreturned.ogg" ) );
		public static void BlueFlagDropped() => Announce( AudioCache.GetStream( "res://sounds/announcer/blueflagdropped.ogg" ) );
		public static void BlueFlagTaken() => Announce( AudioCache.GetStream( "res://sounds/announcer/blueflagtaken.ogg" ) );
		public static void BlueScores() => Announce( AudioCache.GetStream( "res://sounds/announcer/bluescore.ogg" ) );
		public static void RedFlagReturned() => Announce( AudioCache.GetStream( "res://sounds/announcer/redflagreturned.ogg" ) );
		public static void RedFlagDropped() => Announce( AudioCache.GetStream( "res://sounds/announcer/redflagdropped.ogg" ) );
		public static void RedFlagTaken() => Announce( AudioCache.GetStream( "res://sounds/announcer/redflagtaken.ogg" ) );
		public static void RedScores() => Announce( AudioCache.GetStream( "res://sounds/announcer/redscore.ogg" ) );

		public override void _Ready() {
			base._Ready();

			AudioChannel = GetNode<AudioStreamPlayer>( "AudioStreamPlayer" );
			AudioChannel.SetProcess( false );
			AudioChannel.SetProcessInternal( false );

			SetProcess( false );
			SetProcessInternal( false );
		}
	};
};