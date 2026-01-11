using Godot;

namespace Game.Presentation.Screens.LoadingScreen {
	/*
	===================================================================================
	
	LoadingScreen
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class LoadingScreen : CanvasLayer {
		[Export]
		private StringName[] _tipList;

		private int _currentTip = 0;

		private readonly Timer _tipTimer = new Timer() {
			WaitTime = 4.5f,
			OneShot = false,
		};

		/*
		===============
		OnSwitchTip
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSwitchTip() {
			_currentTip = ;
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

			_tipTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSwitchTip ) );
		}
	};
};