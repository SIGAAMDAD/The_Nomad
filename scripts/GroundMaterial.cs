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

public enum GroundMaterialType : int {
	Stone,
	Sand,
	Wood,
	Water,

	Count
};

/*
===================================================================================

GroundMaterial

===================================================================================
*/

public partial class GroundMaterial : Area2D {
	[Export]
	private GroundMaterialType Type;

	/*
	===============
	OnBodyEntered
	===============
	*/
	private void OnBodyEntered( Node2D body ) {
		if ( body is Player player && player != null ) {
			player.SetGroundMaterial( Type );
		} else if ( body is Multiplayer.NetworkPlayer node && node != null ) {
			node.SetGroundMaterial( Type );
		}
	}

	/*
	===============
	OnBodyExited
	===============
	*/
	private void OnBodyExited( Node2D body ) {
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

		Connect( Area2D.SignalName.BodyEntered, Callable.From<Node2D>( OnBodyEntered ) );
		Connect( Area2D.SignalName.BodyExited, Callable.From<Node2D>( OnBodyExited ) );
	}
};