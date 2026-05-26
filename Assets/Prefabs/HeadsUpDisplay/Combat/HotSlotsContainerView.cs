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
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Sdk.HeadsUpDisplay;
using Nomad.Game.Sdk.Player;

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================

	HotSlotsContainerView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class HotSlotsContainerView : VBoxContainer, IHotSlotContainerView
	{
		private readonly HotSlotView[] _slots = new HotSlotView[Constants.MAX_HOT_SLOTS];
		private readonly VSeparator _selector = new VSeparator();

		private int _currentSlot = 0;

		/*
		===============
		SetColor
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( System.Numerics.Vector4 color )
		{
			Modulate = new Color( color.X, color.Y, color.Z, color.W );
		}

		/*
		===============
		FadeOut
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void FadeOut( float fadeOutSpeedSeconds = 1.0f )
		{
			CreateTween()
				.TweenProperty( this, "modulate", Colors.Transparent, fadeOutSpeedSeconds );
		}

		/*
		===============
		SetSelectedHotSlot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="index"></param>
		public void SetSelectedHotSlot( int index )
		{
			RangeGuard.ThrowIfOutOfRange( index, 0, Constants.MAX_HOT_SLOTS );

			_currentSlot = index;
			_selector.Reparent( _slots[_currentSlot] );
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

			for ( int i = 0; i < Constants.MAX_HOT_SLOTS; i++ ) {
				var slot = GetNode<HotSlotView>( $"HotSlot{i}" );
				slot.SetSlotIndex( i + 1 );
				_slots[i] = slot;
			}

			_slots[_currentSlot].AddChild( _selector );
		}
	};
};
