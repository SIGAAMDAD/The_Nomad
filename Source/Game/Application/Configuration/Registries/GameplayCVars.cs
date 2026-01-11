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

using Nomad.CVars;

namespace Game.Application.Configuration.Registries {
	/*
	===================================================================================
	
	GameplayCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class GameplayCVars {
		/*
		===============
		Register
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public static void Register( ICVarSystemService cvarSystem ) {
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: new( "game.ScreenShakeIntensity" ),
					DefaultValue: 1.0f,
					Description: new( "Scales the intensity of how much the game will jitter the camera. Set to lower values for less jolting." ),
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<int>(
					Name: new( "game.EnemyTacticalIntelligence" ),
					DefaultValue: 0,
					Description: new( "Controls how much planning can be executed for an enemy GOAP agent. Directly impacts performance." ),
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: new( "game.PlayerDamageScale" ),
					DefaultValue: 1.0f,
					Description: new( "Scales how much damage the player receives." ),
					Flags: CVarFlags.Archive
				)
			);
		}
	};
};