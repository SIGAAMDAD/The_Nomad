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
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay;

namespace Nomad.Game.Prefabs
{
	public partial class HotSlotView : HBoxContainer
	{
		private readonly HudComponentView _view;

		private Label _indexLabel;
		private TextureRect _icon;

		public HotSlotView()
		{
			_view = new HudComponentView( this );
		}

		public void SetOpacity( float value )
		{
			Modulate = new Color( Modulate.R, Modulate.G, Modulate.B, value );
		}

		public void SetIcon( Texture2D icon )
		{
			_icon.Texture = icon;
		}

		public void SetSlotIndex( int index )
		{
			_indexLabel.Text = index.ToString();
		}

		public void Activate()
		{
			Modulate = Colors.White;
		}

		public void Deactivate()
		{
		}

		public override void _Ready()
		{
			base._Ready();

			_indexLabel = GetNode<Label>( "SlotLabel" );
			_icon = GetNode<TextureRect>( "Icon" );
		}
	};
};
