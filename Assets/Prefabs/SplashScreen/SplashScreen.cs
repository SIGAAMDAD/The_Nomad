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
using Nomad.Events.Globals;
using Nomad.Game.Application.UI.Menus;
using Nomad.UI;

namespace Nomad.Game.Prefabs {
	/*
	===================================================================================
	
	SplashScreen
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed partial class SplashScreen : EnginePanel {
		private const float HOLD_TIME = 6.0f;
		private const float FADE_TIME = 1.5f;

		private ShaderMaterial _shader;
		private Timer _timer;
		private int _stage = 0;

		private TextureRect _thirdPartyLogos;
		private TextureRect _godotLogo;
		private Label _epilepsyWarning;

		/*
		===============
		OnHoldTimeTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnHoldTimeTimeout() {
			Tween? tween = _stage < 2 ? CreateTween() : null;
			float from = 0.0f;
			float to = 1.0f;

			switch ( _stage ) {
				case 0:
					tween.Connect( Tween.SignalName.Finished, Callable.From( OnShowEngineLogo ) );
					_shader.SetShaderParameter( "edge_width", 0.075f );
					break;
				case 1:
					from = 1.0f;
					to = 0.0f;
					tween.Connect( Tween.SignalName.Finished, Callable.From( OnShowEpilepsyWarning ) );
					break;
				case 2:
					GameEventRegistry
						.GetEvent<MenuTransitionRequestedEventArgs>( MenuTransitionRequestedEventArgs.Name, MenuTransitionRequestedEventArgs.NameSpace )
						.Publish( new MenuTransitionRequestedEventArgs( MenuState.Splash, MenuState.Main ) );
					break;
			}
			tween?.TweenMethod(
				Callable.From<float>( value => _shader.SetShaderParameter( "progress", value ) ),
				from,
				to,
				FADE_TIME
			)
			.SetTrans( Tween.TransitionType.Linear );
			_stage++;
		}

		/*
		===============
		OnShowEpilepsyWarning
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnShowEpilepsyWarning() {
			_epilepsyWarning.Visible = true;
			_timer.Start();
		}

		/*
		===============
		OnShowEngineLogo
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnShowEngineLogo() {
			_shader = _godotLogo.Material as ShaderMaterial;
			_timer.Start();
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			base.OnInit();

			_godotLogo = GetNode<TextureRect>( "GodotLogo" );

			_timer = GetNode<Timer>( "Timer" );
			_timer.WaitTime = HOLD_TIME;
			_timer.Connect( Timer.SignalName.Timeout, Callable.From( OnHoldTimeTimeout ) );
			_timer.Start();

			_thirdPartyLogos = GetNode<TextureRect>( "ThirdPartyLogos" );
			_shader = _thirdPartyLogos.Material as ShaderMaterial;

			_epilepsyWarning = GetNode<Label>( "EpilepsyWarning" );
		}
	};
};