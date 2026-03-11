using System;
using Godot;

namespace Game.Presentation.Screens.LoadingScreen {
	/*
	===================================================================================
	
	LoadingScreenController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class LoadingScreenController : IDisposable {
		private int _currentTip = 0;

		private readonly StringName[] _tipList;
		private readonly System.Threading.Timer _switchTipTimer;
		private readonly Label _tipLabel;

		/*
		===============
		LoadingScreenController
		===============
		*/
		/// <summary>
		/// Creates a LoadingScreenController.
		/// </summary>
		/// <param name="owner"></param>
		public LoadingScreenController( LoadingScreen owner, StringName[] tipList ) {
			_tipList = tipList;
			_tipLabel = owner.GetNode<Label>( "%TipLabel" );
			_switchTipTimer = new System.Threading.Timer( OnSwitchTip, null, TimeSpan.FromMilliseconds( 4500 ), TimeSpan.FromMilliseconds( 4500 ) );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
		}

		/*
		===============
		OnSwitchTip
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void OnSwitchTip( object? state ) {
			_currentTip = Random.Shared.Next( 0, _tipList.Length - 1 );
			_tipLabel.Text = _tipList[ _currentTip ];
		}
	};
};