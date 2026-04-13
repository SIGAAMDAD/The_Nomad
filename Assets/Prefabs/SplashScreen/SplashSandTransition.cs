using Godot;
using System;

namespace Nomad.Game.Prefabs {
	public partial class SplashSandTransition : Control {
		private const float HOLD_TIME = 3.0f;
		private const float FADE_TIME = 1.5f;

		private ShaderMaterial _shader;
		private Timer _timer;
		private bool _showingEngine = false;
		private TextureRect _godotLogo;

		public event Action Finished;

		/*
		===============
		OnHoldTimeTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnHoldTimeTimeout() {
			var tween = CreateTween();
			tween.TweenMethod(
				Callable.From<float>( value => _shader.SetShaderParameter( "progress", value ) ),
				1.0f,
				0.0f,
				FADE_TIME
			)
			.SetTrans( Tween.TransitionType.Linear );

			if ( _showingEngine ) {
				tween.Connect( Tween.SignalName.Finished, Callable.From( () => Finished?.Invoke() ) );
			} else {
				tween.Connect( Tween.SignalName.Finished, Callable.From( OnShowEngineLogo ) );
			}
			_showingEngine = true;
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
			_godotLogo.Visible = true;
			GetNode<TextureRect>( "TextureRect" ).Visible = false;

			_timer.Start();
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			_godotLogo = GetNode<TextureRect>( "GodotLogo" );

			_timer = GetNode<Timer>( "Timer" );
			_timer.WaitTime = HOLD_TIME;
			_timer.Connect( Timer.SignalName.Timeout, Callable.From( OnHoldTimeTimeout ) );
			_timer.Start();

			_shader = GetNode<TextureRect>( "TextureRect" ).Material as ShaderMaterial;
		}
	};
};