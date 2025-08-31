/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using ResourceCache;
using System;
using System.Collections.Generic;

namespace Renown.Thinkers.GoapCache {
	/*
	===================================================================================
	
	GoapAllocator
	
	===================================================================================
	*/
	/// <summary>
	/// Allocates the appropriate sets of goals, actions, and sensors for a provided thinkerId
	/// </summary>

	public class GoapAllocator {
		/*
		===============
		GetActionList
		===============
		*/
		/// <summary>
		/// Constructs a list of valid <see cref="MountainGoap.Action"/> from a file cache
		/// in res://resources/goap_cache/[insert thinker id]_goap_list.tres
		/// </summary>
		/// <param name="thinkerId">The id of the thinker</param>
		/// <returns>List of valid actions provided in the cache file</returns>
		/// <exception cref="ArgumentException">Thrown if thinkerId is null or empty</exception>
		public static List<MountainGoap.Action> GetActionList( string thinkerId ) {
			if ( thinkerId == null || thinkerId.Length == 0 ) {
				throw new ArgumentException( "thinkerId is invalid (null or empty)" );
			}
			EntityGoapData? data = (EntityGoapData?)PreLoader.GetResource( "res://resources/goap_cache/" + thinkerId + "_goap_list.tres" );
			ArgumentNullException.ThrowIfNull( data );

			List<MountainGoap.Action> actions = new List<MountainGoap.Action>( data.AllowedActions.Count );
			for ( int i = 0; i < data.AllowedActions.Count; i++ ) {
				actions.Add( GoapActionCache.Cache[ data.AllowedActions[ i ] ] );
			}
			return actions;
		}

		/*
		===============
		GetGoalList
		===============
		*/
		/// <summary>
		/// Constructs a list of valid <see cref="MountainGoap.Goal"/> from a file cache
		/// in res://resources/goap_cache/[insert thinker id]_goap_list.tres
		/// </summary>
		/// <param name="thinkerId">The id of the thinker</param>
		/// <returns>List of valid goals provided in the cache file</returns>
		/// <exception cref="ArgumentException">Thrown if thinkerId is null or empty</exception>
		public static List<MountainGoap.BaseGoal> GetGoalList( string thinkerId ) {
			if ( thinkerId == null || thinkerId.Length == 0 ) {
				throw new ArgumentException( "thinkerId is invalid (null or empty)" );
			}
			EntityGoapData? data = (EntityGoapData?)PreLoader.GetResource( "res://resources/goap_cache/" + thinkerId + "_goap_list.tres" );
			ArgumentNullException.ThrowIfNull( data );

			List<MountainGoap.BaseGoal> goals = new List<MountainGoap.BaseGoal>( data.AllowedGoals.Count );
			for ( int i = 0; i < data.AllowedGoals.Count; i++ ) {
				goals.Add( GoapGoalCache.Cache[ data.AllowedGoals[ i ] ] );
			}
			return goals;
		}

		/*
		===============
		GetSensorList
		===============
		*/
		/// <summary>
		/// Constructs a list of valid <see cref="MountainGoap.Sensor"/> from a file cache
		/// in res://resources/goap_cache/[insert thinker id]_goap_list.tres
		/// </summary>
		/// <param name="thinkerId">The id of the thinker</param>
		/// <returns>List of valid actions provided in the actions cache file</returns>
		/// <exception cref="ArgumentException">Thrown if thinkerId is null or empty</exception>
		public static List<MountainGoap.Sensor> GetSensorList( string thinkerId ) {
			if ( thinkerId == null || thinkerId.Length == 0 ) {
				throw new ArgumentException( "thinkerId is invalid (null or empty)" );
			}
			EntityGoapData? data = (EntityGoapData?)PreLoader.GetResource( "res://resources/goap_cache/" + thinkerId + "_goap_list.tres" );
			ArgumentNullException.ThrowIfNull( data );

			List<MountainGoap.Sensor> sensors = new List<MountainGoap.Sensor>( data.AllowedSensors.Count );
			for ( int i = 0; i < data.AllowedSensors.Count; i++ ) {
				sensors.Add( GoapSensorCache.Cache[ data.AllowedSensors[ i ] ] );
			}
			return sensors;
		}

		/*
		===============
		GetGoapData
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="thinkerId"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		private static EntityGoapData GetGoapData( string thinkerId ) {
			if ( thinkerId == null || thinkerId.Length == 0 ) {
				throw new ArgumentException( "thinkerId is invalid (null or empty)" );
			}
			EntityGoapData? data = (EntityGoapData?)PreLoader.GetResource( "res://resources/goap_cache/" + thinkerId + "_goap_list.tres" );
			ArgumentNullException.ThrowIfNull( data );

			return data;
		}
	};
};