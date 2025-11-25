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

using Menus.Settings;
using System;
using Settings;

namespace Menus.Settings.Options {
	/*
	===================================================================================
	
	GameplayOptionManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class GameplayOptionManager : OptionContainer {
		private GameplayConfig? Temp;

		private SelectionNodes.OptionList? HUDPreset;
		private SelectionNodes.OptionCheckbox? EnableTutorials;
		private SelectionNodes.OptionCheckbox? EquipWeaponOnPickup;
		private SelectionNodes.OptionSlider? ScreenShakeIntensity;

		//
		// Immersion Settings
		//

		private SelectionNodes.OptionCheckbox? MapMarkers;
		private SelectionNodes.OptionCheckbox? WeatherEffectsGameplay;
		private SelectionNodes.OptionCheckbox? DayNightCycleEffectsGameplay;
		private SelectionNodes.OptionCheckbox? RealisticFirearms;
		private SelectionNodes.OptionCheckbox? AutoInform;
		private SelectionNodes.OptionCheckbox? ShowRenownStats;

		//
		// Difficulty Settings
		//

		private SelectionNodes.OptionList? DifficultyPreset;
		private SelectionNodes.OptionList? FirelinkLimit;
		private SelectionNodes.OptionList? AIDifficulty;
		private SelectionNodes.OptionCheckbox? NPCPermaDeath;

		/*
		===============
		SetConfig
		===============
		*/
		/// <summary>
		/// Initializes the gameplay configuration values.
		/// </summary>
		/// <param name="config"></param>
		public override void SetConfig( in ConfigHandler config ) {
			Temp = config as GameplayConfig;
		}

		/*
		===============
		LinkDifficultyNodes
		===============
		*/
		private void LinkDifficultyNodes() {
			DifficultyPreset = GetNode<SelectionNodes.OptionList>( "OptionsContainer/Difficulty/OptionsContainer/DifficultyPreset" );
			FirelinkLimit = GetNode<SelectionNodes.OptionList>( "OptionsContainer/Difficulty/OptionsContainer/FirelinkLimit" );
		}

		/*
		===============
		LinkImmersionNodes
		===============
		*/
		private void LinkImmersionNodes() {
		}

		/*
		===============
		LinkGameplayNodes
		===============
		*/
		private void LinkGameplayNodes() {
			HUDPreset = GetNode<SelectionNodes.OptionList>( "OptionsContainer/HUDPreset" );
		}

		/*
		===============
		LinkNodes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void LinkNodes() {
			LinkDifficultyNodes();
			LinkImmersionNodes();
		}

		/*
		===============
		MethodName
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnVisibilityChanged() {
		}
	};
};