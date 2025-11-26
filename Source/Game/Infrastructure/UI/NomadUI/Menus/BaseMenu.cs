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
using System.Runtime.InteropServices;

namespace Game.Infrastructure.UI.NomadUI.Menus {
	/*
	===================================================================================
	
	BaseMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class BaseMenu : Control {
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		public readonly struct SetMenuEventData : IEventArgs {
			public readonly string Name;
			public SetMenuEventData( string? name ) {
				ArgumentException.ThrowIfNullOrEmpty( name );

				Name = name;
			}
		};

		public readonly GameEvent SetMenu = new GameEvent( nameof( SetMenu ) );

		/*
		===============
		BindController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="controller"></param>
		public virtual void BindController( MenuController controller ) {
		}
	};
};