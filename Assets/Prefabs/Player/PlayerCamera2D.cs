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

using Godot;
using Nomad.Core.Numerics;
using Nomad.Game.Application.Gameplay;

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================
	
	PlayerCamera2D
	
	===================================================================================
	*/
	/// <summary>
	///  
	/// </summary>

	public sealed partial class PlayerCamera2D : Camera2D
	{
		private Vector2 _joltDirection = Vector2.Zero;
		private float _shakeStrength = 0.0f;

		private const float SHAKE_FADE = 0.5f;
		private const float DIRECTIONAL_INFLUENCE = 0.7f;

		/*
		===============
		PlayerCamera2D
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public PlayerCamera2D()
		{
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready()
		{
			base._Ready();
		}

		public override void _Process( double delta )
		{
			base._Process( delta );

			if ( _shakeStrength > 0.0f ) {
				_shakeStrength = Interpolation.Lerp( _shakeStrength, 0.0f, SHAKE_FADE * (float)delta );

				if ( _joltDirection != Vector2.Zero ) {
					Vector2 baseOffset = _joltDirection.Normalized() * _shakeStrength * DIRECTIONAL_INFLUENCE;
					Offset = baseOffset;
				} else {
					Offset = new Vector2(
						RNJesus.FloatRange( -_shakeStrength, _shakeStrength ),
						RNJesus.FloatRange( -_shakeStrength, _shakeStrength )
					);
				}
			} else {
				Offset = Vector2.Zero;
			}
		}
	};
};
