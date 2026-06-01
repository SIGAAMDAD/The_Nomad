/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Core.CVars;
using Nomad.Game.Sdk.Configuration;
using Nomad.Game.Sdk.Gameplay;
using AutoAimModeType = Nomad.Game.Sdk.Configuration.AutoAimMode;
using HUDPresetType = Nomad.Game.Sdk.Configuration.HUDPreset;

namespace Nomad.Game.Application.Gameplay
{
	internal static partial class GameplayCVarRegistry
	{
		[CVar( "game.Mode", GameplayMode.Single, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<GameplayMode> Mode = new CVarDefinition<GameplayMode>( "game.Mode" );

		[CVar( "game.Gameplay.NPCPermaDeath", false, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<bool> NPCPermaDeath = new CVarDefinition<bool>( "game.Gameplay.NPCPermaDeath" );

		[CVar( "game.Gameplay.CanWither", false, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<bool> CanWither = new CVarDefinition<bool>( "game.Gameplay.CanWither" );

		[CVar( "game.Gameplay.AutoAimMode", AutoAimModeType.Off, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<AutoAimModeType> AutoAimMode = new CVarDefinition<AutoAimModeType>( "game.Gameplay.AutoAimMode" );

		[CVar( "game.Gameplay.HUDPreset", HUDPresetType.Full, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<HUDPresetType> HUDPreset = new CVarDefinition<HUDPresetType>( "game.Gameplay.HUDPreset" );

		[CVar( "game.Gameplay.CombatHUDPreset", HUDPresetType.Full, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<HUDPresetType> CombatHUDPreset = new CVarDefinition<HUDPresetType>( "game.Gameplay.CombatHUDPreset" );

		[CVar( "game.Gameplay.NeutralHUDPreset", HUDPresetType.Full, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<HUDPresetType> NeutralHUDPreset = new CVarDefinition<HUDPresetType>( "game.Gameplay.NeutralHUDPreset" );

		[CVar( "game.Gameplay.ExplorationHUDPreset", HUDPresetType.Full, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<HUDPresetType> ExplorationHUDPreset = new CVarDefinition<HUDPresetType>( "game.Gameplay.ExplorationHUDPreset" );

		[CVar( "game.Gameplay.WeatherImpactsGameplay", false, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<bool> WeatherImpactsGameplay = new CVarDefinition<bool>( "game.Gameplay.WeatherImpactsGameplay" );

		[CVar( "game.Gameplay.RealisticFirearms", false, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<bool> RealisticFirearms = new CVarDefinition<bool>( "game.Gameplay.RealisticFirearms" );

		[CVar( "game.Gameplay.AutoInform", true, Group = "Gameplay", Flags = CVarFlags.Hidden )]
		public static readonly CVarDefinition<bool> AutoInform = new CVarDefinition<bool>( "game.Gameplay.AutoInform" );
	};
};
