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

namespace PlayerSystem.Stats {
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
		/// The maximum value allowed for the stat
		/// </summary>
		public T MaxValue;

		/// <summary>
		/// The maximum value allowed for the stats
		/// </summary>
		public T MinValue;

		/// <summary>
		/// The base value loaded or determined at compile time
		/// </summary>
		public readonly T BaseValue;

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
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="maxValue"></param>
		/// <param name="minValue"></param>
		public PlayerStat( T value, T minValue, T maxValue ) {
			BaseValue = value;

			Value = value;
			MinValue = minValue;
			MaxValue = maxValue;
		}

		/*
		===============
		Reset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Reset() {
			Value = BaseValue;
		}
	};
};