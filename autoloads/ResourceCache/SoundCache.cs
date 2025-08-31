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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace ResourceCache {
	public enum SoundEffect : uint {
		MoveGravel0,
		MoveGravel1,
		MoveGravel2,
		MoveGravel3,
		MoveWood0,
		MoveWood1,
		MoveWood2,
		MoveWood3,
		MoveSand0,
		MoveSand1,
		MoveSand2,
		MoveSand3,
		MoveWater0,
		MoveWater1,
		MoveStone0,
		MoveStone1,
		MoveStone2,
		MoveStone3,

		PlayerMoveGravel0,
		PlayerMoveGravel1,
		PlayerMoveGravel2,
		PlayerMoveGravel3,
		PlayerMoveWood0,
		PlayerMoveWood1,
		PlayerMoveWood2,
		PlayerMoveWood3,
		PlayerMoveSand0,
		PlayerMoveSand1,
		PlayerMoveSand2,
		PlayerMoveSand3,
		PlayerMoveWater0,
		PlayerMoveWater1,
		PlayerMoveStone0,
		PlayerMoveStone1,
		PlayerMoveStone2,
		PlayerMoveStone3,

		PlayerArmFoley,

		PlayerPain0,
		PlayerPain1,
		PlayerPain2,

		ParryLight,
		ParryHeavy,
		IronGrip,

		ChangeWeapon,

		Slide0,
		Slide1,

		ShotgunShell0,
		ShotgunShell1,

		TryOpenDoor,
		OpenDoor0,
		OpenDoor1,
		OpenDoor2,

		DashBurn0,
		DashBurn1,
		DashExplosion,
		DashChargeup,

		SlowmoBegin,
		SlowmoEnd,

		Count
	};
	public partial class SoundCache : Node {
		public static readonly ConcurrentDictionary<SoundEffect, AudioStream>? StreamCache = new ConcurrentDictionary<SoundEffect, AudioStream>();
		private static readonly IReadOnlyDictionary<SoundEffect, string> StreamPaths = new Dictionary<SoundEffect, string>() {
			[SoundEffect.MoveGravel0] = "sounds/env/move_gravel_0.ogg",
			[SoundEffect.MoveGravel1] = "sounds/env/move_gravel_1.ogg",
			[SoundEffect.MoveGravel2] = "sounds/env/move_gravel_2.ogg",
			[SoundEffect.MoveGravel3] = "sounds/env/move_gravel_3.ogg",
			[SoundEffect.MoveWood0] = "sounds/env/move_wood_0.ogg",
			[SoundEffect.MoveWood1] = "sounds/env/move_wood_1.ogg",
			[SoundEffect.MoveWood2] = "sounds/env/move_wood_2.ogg",
			[SoundEffect.MoveWood3] = "sounds/env/move_wood_3.ogg",
			[SoundEffect.MoveSand0] = "sounds/env/move_sand_0.ogg",
			[SoundEffect.MoveSand1] = "sounds/env/move_sand_1.ogg",
			[SoundEffect.MoveSand2] = "sounds/env/move_sand_2.ogg",
			[SoundEffect.MoveSand3] = "sounds/env/move_sand_3.ogg",
			[SoundEffect.MoveWater0] = "sounds/env/moveWater0.ogg",
			[SoundEffect.MoveWater1] = "sounds/env/moveWater1.ogg",
			[SoundEffect.MoveStone0] = "sounds/env/move_stone_0.ogg",
			[SoundEffect.MoveStone1] = "sounds/env/move_stone_1.ogg",
			[SoundEffect.MoveStone2] = "sounds/env/move_stone_2.ogg",
			[SoundEffect.MoveStone3] = "sounds/env/move_stone_3.ogg",

			[SoundEffect.PlayerMoveGravel0] = "sounds/player/move_gravel_0.wav",
			[SoundEffect.PlayerMoveGravel1] = "sounds/player/move_gravel_1.wav",
			[SoundEffect.PlayerMoveGravel2] = "sounds/player/move_gravel_2.wav",
			[SoundEffect.PlayerMoveGravel3] = "sounds/player/move_gravel_3.wav",
			[SoundEffect.PlayerMoveWood0] = "sounds/player/move_wood_0.wav",
			[SoundEffect.PlayerMoveWood1] = "sounds/player/move_wood_1.wav",
			[SoundEffect.PlayerMoveWood2] = "sounds/player/move_wood_2.wav",
			[SoundEffect.PlayerMoveWood3] = "sounds/player/move_wood_3.wav",
			[SoundEffect.PlayerMoveSand0] = "sounds/player/move_sand_0.wav",
			[SoundEffect.PlayerMoveSand1] = "sounds/player/move_sand_1.wav",
			[SoundEffect.PlayerMoveSand2] = "sounds/player/move_sand_2.wav",
			[SoundEffect.PlayerMoveSand3] = "sounds/player/move_sand_3.wav",
			[SoundEffect.PlayerMoveWater0] = "sounds/player/moveWater0.wav",
			[SoundEffect.PlayerMoveWater1] = "sounds/player/moveWater1.wav",
			[SoundEffect.PlayerMoveStone0] = "sounds/player/move_stone_0.wav",
			[SoundEffect.PlayerMoveStone1] = "sounds/player/move_stone_1.wav",
			[SoundEffect.PlayerMoveStone2] = "sounds/player/move_stone_2.wav",
			[SoundEffect.PlayerMoveStone3] = "sounds/player/move_stone_3.wav",

			[SoundEffect.PlayerArmFoley] = "sounds/player/arm_foley.wav",

			[SoundEffect.PlayerPain0] = "sounds/player/pain_0.ogg",
			[SoundEffect.PlayerPain1] = "sounds/player/pain_1.ogg",
			[SoundEffect.PlayerPain2] = "sounds/player/pain_2.ogg",

			[SoundEffect.ParryLight] = "sounds/player/parry_light.wav",
			[SoundEffect.ParryHeavy] = "sounds/player/parry_heavy.ogg",
			[SoundEffect.IronGrip] = "sounds/player/iron_grip.wav",

			[SoundEffect.ChangeWeapon] = "sounds/player/change_weapon.wav",

			[SoundEffect.Slide0] = "sounds/player/slide_0.ogg",
			[SoundEffect.Slide1] = "sounds/player/slide_1.ogg",

			[SoundEffect.ShotgunShell0] = "sounds/env/shotgun_shell_0.wav",
			[SoundEffect.ShotgunShell1] = "sounds/env/shotgun_shell_1.wav",

			[SoundEffect.TryOpenDoor] = "sounds/env/try_open_door.wav",
			[SoundEffect.OpenDoor0] = "sounds/env/open_door_0.wav",
			[SoundEffect.OpenDoor1] = "sounds/env/open_door_1.wav",
			[SoundEffect.OpenDoor2] = "sounds/env/open_door_2.wav",

			[SoundEffect.DashBurn0] = "sounds/player/dashkit_burn_0.wav",
			[SoundEffect.DashBurn1] = "sounds/player/dashkit_burn_1.wav",
			[SoundEffect.DashExplosion] = "sounds/player/dash_explosion.ogg",
			[SoundEffect.DashChargeup] = "sounds/player/dash_chargeup.ogg",

			[SoundEffect.SlowmoBegin] = "sounds/player/slowmo_begin.wav",
			[SoundEffect.SlowmoEnd] = "sounds/player/slowmo_end.ogg",
		};

		public static AudioStream? GetEffectRange( SoundEffect baseEffect, int range ) {
			ArgumentNullException.ThrowIfNull( StreamCache );
			if ( range < 0 || range > StreamCache.Count ) {
				throw new ArgumentOutOfRangeException( $"range ({range}) is less than 0 or greater than the amount of sound effects loaded" );
			} else if ( baseEffect < 0 || baseEffect >= SoundEffect.Count ) {
				throw new ArgumentOutOfRangeException( $"baseEffect ({baseEffect}) is not a valid SoundEffect enum" );
			}
			
			return StreamCache[ (SoundEffect)( (int)baseEffect + RNJesus.IntRange( 0, range ) ) ];
		}

		/*
		===============
		_EnterTree
		===============
		*/
		public override void _EnterTree() {
			base._EnterTree();
			ProcessMode = ProcessModeEnum.Disabled;

			Console.PrintLine( "Initializing SoundCache..." );

			System.Threading.Tasks.Parallel.For( 0, (int)SoundEffect.Count, ( index ) => {
				string path = $"res://{StreamPaths[ (SoundEffect)index ]}";
				StreamCache.TryAdd( (SoundEffect)index, AudioCache.GetStream( path ) );
			} );
		}
	};
};