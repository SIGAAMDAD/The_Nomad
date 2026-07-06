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

using System;
using Godot;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Player.Stats;
using Nomad.Game.Prefabs;
using NumericsVector3 = System.Numerics.Vector3;

namespace Nomad.Game.Gameplay.Player
{
	/*
	===================================================================================

	PlayerSpawnApplicator

	===================================================================================
	*/
	/// <summary>
	/// Applies a spawn profile's data to the current player state.
	/// </summary>

	internal sealed class PlayerSpawnApplicator : IPlayerSpawnApplicator
	{
		public void Apply( IPlayerBase player, PlayerSpawnProfileDefinition profile, IPlayerDerivedStatService derivedStats, IPlayerResourceService resources, IPlayerFlagService flags, in PlayerSpawnContext context )
		{
			if ( player is PlayerBase playerBase ) {
				ApplySpawnTransform( playerBase.Prefab, in context );
			}

			ApplyResource( profile.Health, PlayerResourceType.Health, derivedStats.GetValue( DerivedStatType.EffectiveHealthMax ), resources );
			ApplyResource( profile.Rage, PlayerResourceType.Rage, derivedStats.GetValue( DerivedStatType.EffectiveRageMax ), resources );
			ApplyResource( profile.Sanity, PlayerResourceType.Sanity, derivedStats.GetValue( DerivedStatType.EffectiveSanityMax ), resources );

			foreach ( var pair in profile.FlagOverrides ) {
				flags.SetFlag( pair.Key, pair.Value );
			}
		}

		private static void ApplySpawnTransform( PlayerPrefab prefab, in PlayerSpawnContext context )
		{
			NumericsVector3 position = context.SpawnWorldPosition;
			prefab.GlobalPosition = new Vector3( position.X, position.Y, position.Z );
			prefab.Velocity = Vector3.Zero;
		}

		private static void ApplyResource( SpawnValueRule rule, PlayerResourceType type, float maxValue, IPlayerResourceService resources )
		{
			switch ( rule.Mode ) {
				case SpawnValueMode.Preserve:
					break;
				case SpawnValueMode.Full:
					resources.SetValue( type, maxValue );
					break;
				case SpawnValueMode.PercentOfMax:
					resources.SetValue( type, maxValue * rule.Value );
					break;
				case SpawnValueMode.Absolute:
					resources.SetValue( type, rule.Value );
					break;
				case SpawnValueMode.Zero:
					resources.SetValue( type, 0.0f );
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( rule ) );
			}
		}
	};
};
