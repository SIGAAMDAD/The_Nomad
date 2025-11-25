/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using EventSystem;
using Godot;
using System;

namespace Game.Infrastructure.UI.NomadUI.Menus {
	/*
	===================================================================================
	
	MenuController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class MenuController : BaseMenu {
		[Export]
		protected BaseMenu[] Menus;

		public int State { get; private set; }

		public readonly GameEvent MenuStateChanged = new GameEvent( nameof( MenuStateChanged ) );

		/*
		===============
		DisableMenu
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="menu"></param>
		private static void DisableMenu( BaseMenu menu ) {
			menu.SetDeferred( BaseMenu.PropertyName.Visible, false );
			menu.SetDeferred( BaseMenu.PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );
		}

		/*
		===============
		EnableMenu
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="menu"></param>
		private static void EnableMenu( BaseMenu menu ) {
			menu.SetDeferred( BaseMenu.PropertyName.Visible, true );
			menu.SetDeferred( BaseMenu.PropertyName.ProcessMode, (long)ProcessModeEnum.Always );
		}

		/*
		===============
		OnSetMenu
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		private void OnSetMenu( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is SetMenuEventData setMenu ) {
				for ( int i = 0; i < Menus.Length; i++ ) {
					if ( Menus[ i ].Name == setMenu.Name ) {
						EnableMenu( Menus[ i ] );
					} else {
						DisableMenu( Menus[ i ] );
					}
				}
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
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

			for ( int i = 0; i < Menus.Length; i++ ) {
				Menus[ i ].SetMenu.Subscribe( this, OnSetMenu );
				Menus[ i ].BindController( this );
			}
		}
	};
};