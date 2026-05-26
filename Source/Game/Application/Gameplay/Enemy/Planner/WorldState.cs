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

namespace Nomad.Game.Application.Gameplay.Enemy.Planner
{
	public readonly struct WorldState
	{
		private readonly ulong _lo;
		private readonly ulong _hi;

		public WorldState( ulong lo, ulong hi )
		{
			_lo = lo;
			_hi = hi;
		}

		public bool Get( WorldKey key )
		{
			int index = (int)key;
			if ( index < 64 ) {
				ulong mask = 1UL << index;
				return (_lo & mask) != 0;
			}

			index -= 64;
			ulong hiMask = 1UL << index;
			return (_hi & hiMask) != 0;
		}

		public WorldState Set( WorldKey key, bool value )
		{
			int index = (int)key;
			ulong lo = _lo;
			ulong hi = _hi;

			if ( index < 64 ) {
				ulong mask = 1UL << index;
				if ( value ) {
					lo |= mask;
				} else {
					lo &= ~mask;
				}
			} else {
				index -= 64;
				ulong mask = 1UL << index;
				if ( value ) {
					hi |= mask;
				} else {
					hi &= ~mask;
				}
			}

			return new WorldState( lo, hi );
		}

		public bool Meets( WorldCondition[] conditions )
		{
			for ( int i = 0; i < conditions.Length; i++ ) {
				if ( Get( conditions[i].Key ) != conditions[i].Value ) {
					return false;
				}
			}

			return true;
		}

		public WorldState Apply( WorldEffect[] effects )
		{
			WorldState state = this;

			for ( int i = 0; i < effects.Length; i++ ) {
				state = state.Set( effects[i].Key, effects[i].Value );
			}

			return state;
		}

		public int CountUnmet( WorldCondition[] conditions )
		{
			int count = 0;

			for ( int i = 0; i < conditions.Length; i++ ) {
				if ( Get( conditions[i].Key ) != conditions[i].Value ) {
					count++;
				}
			}

			return count;
		}

		public bool Equals( WorldState other )
		{
			return _lo == other._lo && _hi == other._hi;
		}

		public override bool Equals( object obj )
		{
			return obj is WorldState other && Equals( other );
		}

		public override int GetHashCode()
		{
			unchecked {
				return (_lo.GetHashCode() * 397) ^ _hi.GetHashCode();
			}
		}

		public static bool operator ==( WorldState left, WorldState right ) => left.Equals( right );
		public static bool operator !=( WorldState left, WorldState right ) => !left.Equals( right );
	}
}
