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

namespace PlayerSystem.Upgrades {
	public interface IDashModule {
		public abstract StringName Name { get; }
		public abstract StringName Description { get; }

		/// <summary>
		/// Does this module allow the player to overheat the dashkit?
		/// </summary>
		public abstract bool CanBurnout { get; }

		/// <summary>
		/// The maximum amount of burnout that is allowed before overcharge
		/// </summary>
		public abstract float BurnoutMax { get; }

		/// <summary>
		/// The amount of time before the dashkit cools down after being used
		/// </summary>
		public abstract float BurnoutCooldown { get; }
		
		/// <summary>
		/// The amount of time in milliseconds that each dash gets
		/// </summary>
		public abstract float DashDuration { get; }

		public abstract void ApplyEffect( DashKit dashKit );
	};
};