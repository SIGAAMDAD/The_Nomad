using Godot;
using Nomad.Core.Events;
using Nomad.EngineUtils.UserInterface;
using Nomad.Events.Global;

namespace Game.Prefabs.SplashScreen {
	/*
	===================================================================================
	
	Logo
	
	===================================================================================
	*/
	/// <summary>
	/// Handles logo showcasing in the splash screen.
	/// </summary>
	
	public partial class Logo : EnginePanel {
		[Export]
		private float _duration = 1.0f;
		[Export]
		private EnginePanel _logo;

		public IGameEvent<EmptyEventArgs> Finished => _finished;
		private readonly IGameEvent<EmptyEventArgs> _finished;

		/*
		===============
		Logo
		===============
		*/
		/// <summary>
		/// Creates a Logo node.
		/// </summary>
		public Logo() {
			_finished = GameEventRegistry.GetEvent<EmptyEventArgs>( nameof( Finished ), "Logo" );
		}

		/*
		===============
		OnFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnFinished() {
			_finished.Publish( default );
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
			if ( (Node)_logo is VideoStreamPlayer player ) {
				player.Connect( VideoStreamPlayer.SignalName.Finished, Callable.From( OnFinished ) );
			} else {
				// TODO: event?
				var timer = new Timer() {
					WaitTime = _duration
				};
				timer.CallDeferred( Timer.MethodName.Start );
				timer.Connect( Timer.SignalName.Timeout, Callable.From( OnFinished ) );
			}
		}
	};
};
