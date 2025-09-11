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

/*
===================================================================================

ImmersionSettings

===================================================================================
*/

public partial class ImmersionSettings : Resource {
	//
	// Immersion Settings
	//

	/// <summary>
	/// If enabled, the game will make the player engage with quest discovery diagetically instead
	/// of automatic map markers
	/// </summary>
	[Export] public bool TavernRumors { get; private set; } = false;

	/// <summary>
	/// Allows the dynamic weather cycle of each biome to impact gameplay in specific ways
	/// </summary>
	[Export] public bool WeatherEffectsGameplay { get; private set; } = false;

	/// <summary>
	/// Allows the day-night cycle to be used as a diagetic deadline for quests
	/// </summary>
	[Export] public bool DayNightCycleEffectsGameplay { get; private set; } = false;

	/// <summary>
	/// Enables semi-realistic firearm mechanics. Manual reloading, and weather impacts firearms in various
	/// ways
	/// </summary>
	[Export] public bool RealisticFirearms { get; private set; } = false;

	/// <summary>
	/// Enables viewership of the internal renown numbers. This is not recommended as it removes a whole layer of immersion
	/// </summary>
	[Export] public bool ShowRenownStats { get; private set; } = false;

	/// <summary>
	/// Makes the Galakan Gossip automatically show up in the player's inventory instead of having them buy it in-game.
	/// </summary>
	[Export] public bool AutoInform { get; private set; } = false;
};