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

namespace Nomad.Game.Presentation.Screens.LoadingScreen
{
	/*
	===================================================================================

	LoadingScreen

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class LoadingScreenGodotView : Control, ILoadingScreenView
	{
		private Label _tipLabel;
		private ShaderMaterial _progressBar;
		private LoadingScreenPresenter _presenter;

		public void SetTipText( string text )
		{
			if ( _tipLabel == null ) {
				return;
			}

			_tipLabel.SetDeferred( Label.PropertyName.Text, text );
		}

		public new void Show()
		{
			SetDeferred( Control.PropertyName.Visible, true );
		}

		public new void Hide()
		{
			SetDeferred( Control.PropertyName.Visible, false );
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

			_tipLabel = GetNode<Label>( "Label" );

			if ( GetNode<TextureRect>( "ProgressBar" ).Material is ShaderMaterial material ) {
				_progressBar = material;
			}

			_presenter = ScreenPresenterFactory.CreateLoadingScreenPresenter( this );
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree();

			_presenter?.Dispose();
			_presenter = null;
		}
	};
};
