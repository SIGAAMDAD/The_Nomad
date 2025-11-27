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


using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using Godot;
using NomadCore.Systems.ConsoleSystem.Services;
using NomadCore.Systems.EventSystem.Services;
using NomadCore.Systems.SaveSystem.Services;
using NomadCore.Systems.Audio.Services;
using NomadCore.Systems.EntitySystem.Services;
using Game.Domain.Settings;
using Game.Application.Configuration;

namespace Game.Infrastructure {
	/*
	===================================================================================

	NomadBootstrapper

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class NomadBootstrapper : Node {
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

			var eventBus = ServiceRegistry.Register<IGameEventBusService>( new GameEventBus() );
			ServiceRegistry.Register<IConsoleService>( new Console( GetTree().Root, eventBus ) );
			ServiceRegistry.Register<ISaveService>( new SaveManager() );
			ServiceRegistry.Register<IAudioService>( new AudioService() );
			ServiceRegistry.Register<IEntityService>( new EntityComponentSystem( GetTree().Root ) );

			ServiceRegistry.Register<ICVarSystemService>( CVarRegistrationService.RegisterCVarSystem() );
			var settingsManager = new SettingsManager( "user://settings.ini" );
		}
	};
};