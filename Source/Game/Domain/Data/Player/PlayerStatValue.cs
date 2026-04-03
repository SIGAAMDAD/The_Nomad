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
using System.Runtime.CompilerServices;

namespace Nomad.Game.Domain.Data.Player {
	/*
	===================================================================================
	
	PlayerStatValue
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public struct PlayerStatValue : IEquatable<PlayerStatValue> {
		public readonly StatType Type => _type;
		private readonly StatType _type;

		public float Value {
			readonly get => _value;
			set => _value = Math.Clamp( value, _limits.Min, _limits.Max );
		}
		private float _value;

		private readonly StatLimits _limits;

		/*
		===============
		PlayerStatValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="limits"></param>
		/// <param name="initialValue"></param>
		public PlayerStatValue( StatType type, StatLimits limits, float initialValue ) {
			_type = type;
			_limits = limits;
			_value = initialValue;
		}

		/*
		===============
		Equals
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool Equals( PlayerStatValue other ) {
			return other.Type == _type && other.Value == _value;
		}
	};
};