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

using System.Collections.Generic;
using Nomad.Game.Domain.Data.Player;

namespace Nomad.Game.Application.Gameplay.Player.Stats
{
	/*
	===================================================================================

	PlayerStatDependencyGraph

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerStatDependencyGraph
	{
		private readonly Dictionary<BaseStatType, DerivedStatType[]> _baseToDerived = new();
		private readonly Dictionary<DerivedStatType, DerivedStatType[]> _derivedToDerived = new();

		/*
		===============
		PlayerStatDependencyGraph
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public PlayerStatDependencyGraph()
		{
		}

		/*
		===============
		CreateDefault
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public static PlayerStatDependencyGraph CreateDefault()
		{
			PlayerStatDependencyGraph graph = new PlayerStatDependencyGraph();
			graph.ConfigureDefaultDependencies();
			return graph;
		}

		/*
		===============
		ConfigureDefaultDependencies
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void ConfigureDefaultDependencies()
		{
			_baseToDerived[BaseStatType.InventoryWeight] = [
				DerivedStatType.MovementSpeedMultiplier,
				DerivedStatType.EffectiveMovementSpeed
			];

			_baseToDerived[BaseStatType.EncumbranceThreshold] = [
				DerivedStatType.MovementSpeedMultiplier,
				DerivedStatType.EffectiveMovementSpeed
			];

			_baseToDerived[BaseStatType.BaseMovementSpeed] = [
				DerivedStatType.EffectiveMovementSpeed
			];

			_baseToDerived[BaseStatType.BaseDashSpeed] = [
				DerivedStatType.EffectiveDashSpeed
			];

			_baseToDerived[BaseStatType.BaseHealth] = [
				DerivedStatType.EffectiveHealthMax
			];

			_baseToDerived[BaseStatType.BaseRage] = [
				DerivedStatType.EffectiveRageMax
			];

			_baseToDerived[BaseStatType.BaseSanity] = [
				DerivedStatType.EffectiveSanityMax
			];

			_derivedToDerived[DerivedStatType.MovementSpeedMultiplier] = [
				DerivedStatType.EffectiveMovementSpeed
			];

			_derivedToDerived[DerivedStatType.DashSpeedMultiplier] = [
				DerivedStatType.EffectiveDashSpeed
			];
		}

		/*
		===============
		CollectAffectedFromBase
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <param name="output"></param>
		public void CollectAffectedFromBase( BaseStatType type, HashSet<DerivedStatType> output )
		{
			if ( _baseToDerived.TryGetValue( type, out var values ) ) {
				for ( int i = 0; i < values.Length; i++ ) {
					output.Add( values[i] );
				}
			}
		}

		/*
		===============
		CollectAffectedFromDerived
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <param name="output"></param>
		public void CollectAffectedFromDerived( DerivedStatType type, HashSet<DerivedStatType> output )
		{
			if ( _derivedToDerived.TryGetValue( type, out var values ) ) {
				for ( int i = 0; i < values.Length; i++ ) {
					output.Add( values[i] );
				}
			}
		}
	};
};
