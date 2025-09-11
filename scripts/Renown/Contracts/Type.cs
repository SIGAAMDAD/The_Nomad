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

namespace Renown.Contracts {
	public enum Type : uint {
		/// <summary>
		/// fixed-price bounty, most usual target is a politician
		/// </summary>
		Assassination,

		/// <summary>
		/// You kidnap a motherfucker for money. The proof is the kidnapped target returned to
		/// the guildhall in a mostly intact state.
		/// </summary>
		Kidnapping,

		/// <summary>
		/// 
		/// </summary>
		Extortion,

		Extraction,

		/// <summary>
		/// an assassination but more for less important entities, this
		/// can include the player.
		/// 
		/// the price of a bounty can and most likely will increase over time
		/// </summary>
		Bounty,

		Count
	};
};