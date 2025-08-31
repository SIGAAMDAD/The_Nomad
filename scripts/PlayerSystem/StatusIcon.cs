/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

using Godot;

namespace PlayerSystem {
	public partial class StatusIcon : TextureRect {
		private StatusEffect StatusEffect;

		public override void _Ready() {
			base._Ready();

			LevelData.Instance.ThisPlayer.Die += ( source, target ) => { SetProcess( false ); Hide(); };

//			Material = ResourceLoader.Load<Material>( "res://resources/materials/status_icon.tres" );
//			Material.Set( "shader_parameter/progress", 1.0f );
//			SetProcess( false );
		}
		public override void _Process( double delta ) {
//			Material.Set( "shader_parameter/progress", Mathf.Lerp( 1.0f, 0.0f, StatusEffect.GetDuration() / StatusEffect.GetTimeLeft() ) );
		}

		public void Start( StatusEffect effect ) {
			Show();
			SetProcess( true );
			StatusEffect = effect;
			Texture = StatusEffect.Icon;

			StatusEffect.Timeout += Stop;
		}
		public void Stop() {
			Hide();
			SetProcess( false );

			StatusEffect.Timeout -= Stop;
		}
	};
};