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
using Renown;

/*
===================================================================================

Hitbox

===================================================================================
*/
/// <summary>
/// An Area2D meant for stuff like weakpoints, headshots, etc.
/// </summary>

public partial class Hitbox : Area2D {
	/// <summary>
	/// The owner of the Hitbox
	/// </summary>
	[Export]
	private Entity Parent;

	[Signal]
	public delegate void HitEventHandler( Entity source, float damageAmount );

	/*
	===============
	OnHit
	===============
	*/
	/// <summary>
	/// Called when a WeaponEntity collides with a Hitbox
	/// </summary>
	/// <param name="source">The damage's source</param>
	/// <param name="damageAmount">The amount of damage</param>
	public void OnHit( Entity source, float damageAmount ) {
		EmitSignalHit( source, damageAmount );
	}

	/*
	===============
	_Ready
	===============
	*/
	/// <summary>
	/// godot initialization override
	/// </summary>
	public override void _Ready() {
		base._Ready();

		SetMeta( "Owner", Parent );
	}
};