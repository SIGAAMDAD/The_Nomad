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

using Game.Application.UI;
using Game.Application.UI.EventHandlers;
using Game.Application.UI.Menus;
using Game.Infrastructure;
using Godot;
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.Logger;

namespace Game.Application {
	/*
	===================================================================================
	
	ApplicationBootstrapper
	
	===================================================================================
	*/
	/// <summary>
	/// Initializes the Application layer.
	/// </summary>
	
	public sealed partial class ApplicationBootstrapper : Node {
		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			var bootstrapper = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" );
			var locator = bootstrapper.ServiceLocator;
			var serviceFactory = bootstrapper.ServicesFactory;
			var rootNode = GetTree().Root;

			var logger = locator.GetService<ILoggerService>();
			var eventFactory = locator.GetService<IGameEventRegistryService>();

			var eventHandler = serviceFactory.RegisterSingleton<AudioUIEventHandler>( new AudioUIEventHandler( locator.GetService<IEmitterFactory>(), eventFactory) );

			var menuManager = new MenuManager( this );
			AddChild( menuManager );
			menuManager.TransitionToMenu( MenuState.Main );
		}
	};
};