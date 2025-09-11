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
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PlayerSystem.Stats {
	/*
	===================================================================================
	
	StatManager

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class StatManager : IStatManager {
		public static readonly float BASE_MAX_HEALTH = 100.0f;
		public static readonly float BASE_MAX_RAGE = 100.0f;
		public static readonly float BASE_MAX_SANITY = 100.0f;

		public float this[ string statName ] {
			get {
				if ( Stats.TryGetValue( statName, out PlayerStat<float> stat ) ) {
					return stat.Value;
				}
				return 0.0f;
			}
			set {
				if ( Stats.TryGetValue( statName, out PlayerStat<float> stat ) ) {
					stat.Value = value;
				}
			}
		}
		
		private readonly IReadOnlyDictionary<string, PlayerStat<float>>? Stats = null;
		private readonly Player? Owner;

		public StatManager( Player? owner ) {
			ArgumentNullException.ThrowIfNull( owner );

			Owner = owner;

			Stats = new Dictionary<string, PlayerStat<float>>() {
				{ "Health", new PlayerStat<float>( BASE_MAX_HEALTH, BASE_MAX_HEALTH / 2.0f, float.MaxValue ) },
				{ "Rage", new PlayerStat<float>( BASE_MAX_RAGE, BASE_MAX_RAGE / 2.0f, float.MaxValue ) },
				{ "Sanity", new PlayerStat<float>( BASE_MAX_SANITY, BASE_MAX_SANITY / 2.0f, BASE_MAX_SANITY ) }
			};
		}

		private StatManager() {
		}

		/*
		===============
		Save
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Save( SaveSystem.SaveSectionWriter writer ) {
			writer.SaveInt( "StatCount", Stats.Count );

			int index = 0;
			foreach ( var stat in Stats ) {
				writer.SaveString( $"State{index}Name", stat.Key );
				writer.SaveFloat( $"Stat{index}Min", stat.Value.MinValue );
				writer.SaveFloat( $"Stat{index}Max", stat.Value.MaxValue );
				writer.SaveFloat( $"Stat{index}Value", stat.Value.Value );
				index++;
			}
		}

		/*
		===============
		Load
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Load( SaveSystem.SaveSectionReader reader ) {
			int statCount = reader.LoadInt( "StatCount" );
			var statCache = new Dictionary<string, PlayerStat<float>>( statCount );

			for ( int i = 0; i < statCount; i++ ) {
				statCache.Add(
					reader.LoadString( $"Stat{i}Name" ),
					new PlayerStat<float>(
						reader.LoadFloat( $"Stat{i}Value" ),
						reader.LoadFloat( $"Stat{i}Min" ),
						reader.LoadFloat( $"Stat{i}Max" )
					)
				);
			}
		}

		/*
		===============
		SetHealth
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetHealth( float health ) {
			SetStatValue( "Health", health );
		}

		/*
		===============
		SetRage
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetRage( float rage ) {
			SetStatValue( "Rage", rage );
		}

		/*
		===============
		SetSanity
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetSanity( float sanity ) {
			SetStatValue( "Sanity", sanity );
		}

		/*
		===============
		GetHealth
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetHealth() {
			return GetStatValue( "Health" );
		}

		/*
		===============
		GetRage
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetRage() {
			return GetStatValue( "Rage" );
		}

		/*
		===============
		GetSanity
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetSanity() {
			return GetStatValue( "Sanity" );
		}

		/*
		===============
		SetStatValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="statName"></param>
		/// <param name="newValue"></param>
		public void SetStatValue( string statName, float newValue ) {
			ArgumentException.ThrowIfNullOrEmpty( statName );

			if ( !Stats.TryGetValue( statName, out PlayerStat<float> stat ) ) {
				Console.PrintError( $"StatManager.SetStatValue: no entry for {statName}" );
				return;
			}

			stat.Value = Mathf.Clamp( newValue, stat.MinValue, stat.MaxValue );
		}

		/*
		===============
		SetStatMaxValue
		===============
		*/
		/// <summary>
		/// Sets the maximum value for <paramref name="statName"/>s
		/// </summary>
		/// <param name="statName">The name of the statistic</param>
		/// <param name="maxValue"></param>
		public void SetStatMaxValue( string statName, float maxValue ) {
			ArgumentException.ThrowIfNullOrEmpty( statName );

			if ( !Stats.TryGetValue( statName, out PlayerStat<float> stat ) ) {
				Console.PrintError( $"StatManager.SetStatMaxValue: no entry for {statName}" );
				return;
			}
			stat.MaxValue = maxValue;
		}

		/*
		===============
		SetStatMinValue
		===============
		*/
		/// <summary>
		/// Sets the minimum value for <paramref name="statName"/>s
		/// </summary>
		/// <param name="statName">The name of the statistic</param>
		/// <param name="minValue"></param>
		public void SetStatMinValue( string statName, float minValue ) {
			ArgumentException.ThrowIfNullOrEmpty( statName );

			if ( !Stats.TryGetValue( statName, out PlayerStat<float> stat ) ) {
				Console.PrintError( $"StatManager.SetStatMaxValue: no entry for {statName}" );
				return;
			}
			stat.MinValue = minValue;
		}

		/*
		===============
		GetStatValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="statName"></param>
		/// <returns></returns>
		public float GetStatValue( string statName ) {
			ArgumentException.ThrowIfNullOrEmpty( statName );

			if ( !Stats.TryGetValue( statName, out PlayerStat<float> stat ) ) {
				Console.PrintError( $"StatManager.GetStatValue: no entry for {statName}" );
				return 0.0f;
			}
			return stat.Value;
		}

		/*
		===============
		GetStatMaxValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="statName"></param>
		/// <returns></returns>
		public float GetStatMaxValue( string statName ) {
			ArgumentException.ThrowIfNullOrEmpty( statName );

			if ( !Stats.TryGetValue( statName, out PlayerStat<float> stat ) ) {
				Console.PrintError( $"StatManager.GetStatMaxValue: no entry for {statName}" );
				return 0.0f;
			}
			return stat.MaxValue;
		}

		/*
		===============
		GetStatMinValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="statName"></param>
		/// <returns></returns>
		public float GetStatMinValue( string statName ) {
			ArgumentException.ThrowIfNullOrEmpty( statName );

			if ( !Stats.TryGetValue( statName, out PlayerStat<float> stat ) ) {
				Console.PrintError( $"StatManager.GetStatMinValue: no entry for {statName}" );
				return 0.0f;
			}
			return stat.MinValue;
		}
	};
};