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

namespace Renown.World {
	public readonly struct WorldTimestamp {
		public readonly uint SavedYear = 0;
		public readonly uint SavedMonth = 0;
		public readonly uint SavedDay = 0;

		public WorldTimestamp() {
			SavedYear = WorldTimeManager.Year;
			SavedMonth = WorldTimeManager.Month;
			SavedDay = WorldTimeManager.Day;
		}
		public WorldTimestamp( uint Year, uint Month, uint Day ) {
			SavedYear = Year;
			SavedMonth = Month;
			SavedDay = Day;
		}
		public WorldTimestamp( WorldTimestamp other ) {
			SavedYear = other.SavedYear;
			SavedMonth = other.SavedMonth;
			SavedDay = other.SavedDay;
		}

		public bool LaterThan( WorldTimestamp other ) => SavedYear > other.SavedYear && SavedMonth > other.SavedMonth && SavedDay > other.SavedDay;
		public bool EarlierThan( WorldTimestamp other ) => SavedYear < other.SavedYear && SavedMonth < other.SavedMonth && SavedDay < other.SavedDay;

		public bool LaterThanOrSame( WorldTimestamp other ) => SavedYear >= other.SavedYear && SavedMonth >= other.SavedMonth && SavedDay >= other.SavedDay;
		public bool EarlierThanOrSame( WorldTimestamp other ) => SavedYear <= other.SavedYear && SavedMonth <= other.SavedMonth && SavedDay <= other.SavedDay;

		public static bool operator >( WorldTimestamp a, WorldTimestamp b ) => a.LaterThan( b );
		public static bool operator <( WorldTimestamp a, WorldTimestamp b ) => a.EarlierThan( b );

		public static bool operator >=( WorldTimestamp a, WorldTimestamp b ) => a.LaterThanOrSame( b );
		public static bool operator <=( WorldTimestamp a, WorldTimestamp b ) => a.EarlierThanOrSame( b );

		public static bool operator ==( WorldTimestamp a, WorldTimestamp b ) => a.Equals( b );
		public static bool operator !=( WorldTimestamp a, WorldTimestamp b ) => !a.Equals( b );

		public bool Equals( WorldTimestamp b ) {
			return SavedYear == b.SavedYear && SavedMonth == b.SavedMonth && SavedDay == b.SavedDay;
		}
	};
};