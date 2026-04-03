using Godot;
using Nomad.Core.Events;
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

	public partial class SplashScreen : EnginePanel {
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
		protected override void OnInit() {                
			for ( int i = 0; i < _screens.Length; i++ ) {
				_screens[ i ].Finished.Subscribe( OnLogoFinished );
			}
		}
	};
};
