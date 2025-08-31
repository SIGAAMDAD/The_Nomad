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

namespace PlayerSystem {
	/*
	===================================================================================
	
	PlayerStat
	
	===================================================================================
	*/
	/// <summary>
	/// For storing player stats such as max health, max rage, etc.
	/// made this way to allow for rune and boon buffs/debuffs
	/// </summary>
	/// <typeparam name="T">The type of stat, must be a primitive</typeparam>
	
	public struct PlayerStat<T> where T : struct {
		/// <summary>
		/// The value assigned to the struct at compile time
		/// </summary>
		public readonly T BaseValue;

		/// <summary>
		/// The maximum value allowed for the stat assigned at compile time
		/// </summary>
		public readonly T MaxValue;

		/// <summary>
		/// The maximum value allowed for the stat assigned at compile time
		/// </summary>
		public readonly T MinValue;

		/// <summary>
		/// The value that can be changed and the value that is read at runtime
		/// </summary>
		public T Value;

		/*
		===============
		PlayerStat
		===============
		*/
		/// <summary>
		/// Constructs a PlayerStat with the provided values
		/// </summary>
		/// <param name="baseValue"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public PlayerStat( T baseValue, T min, T max ) {
			BaseValue = baseValue;
			MaxValue = max;
			MinValue = min;

			Value = BaseValue;
		}
	};
};