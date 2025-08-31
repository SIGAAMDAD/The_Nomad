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
	public partial class RageBar : ProgressBar {
		private Timer ShowTimer;

		public float Rage {
			set {
				ProcessMode = ProcessModeEnum.Pausable;
				Modulate = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
				ShowTimer.Start();
				Value = value;
			}
		}

		public void Init( float nRage ) {
			MaxValue = nRage;
			Value = nRage;

			ShowTimer = new Timer();
			ShowTimer.Name = "ShowTimer";
			ShowTimer.OneShot = true;
			ShowTimer.WaitTime = 4.5f;
			ShowTimer.Connect( "timeout", Callable.From(
				() => {
					Tween Tweener = CreateTween();
					Tweener.TweenProperty( this, "modulate", new Color( 0.0f, 0.0f, 0.0f, 0.0f ), 1.0f );
					Tweener.Connect( "finished", Callable.From( () => { ProcessMode = ProcessModeEnum.Disabled; } ) );
				}
			) );
			AddChild( ShowTimer );
		}
	};
};