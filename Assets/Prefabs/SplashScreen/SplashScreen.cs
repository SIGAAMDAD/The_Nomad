using Godot;
using Nomad.Core.Events;

namespace Game.Prefabs.SplashScreen {
	/*
	===================================================================================

	SplashScreen

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class SplashScreen : Control {
		[Export]
		private Logo[] _screens;

		private int _screenIndex = 0;

		/*
		===============
		OnLogoFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnLogoFinished( in EmptyEventArgs args ) {
			_screenIndex++;
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

			for ( int i = 0; i < _screens.Length; i++ ) {
				_screens[ i ].Finished.Subscribe( OnLogoFinished );
			}
		}
	};
};
