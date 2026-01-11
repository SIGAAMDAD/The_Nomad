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

using Godot;
using Game.Application.Common.Models.Interfaces;
using Game.Application.Configuration.Services;
using Game.Infrastructure.Configuration.Godot.Services;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Core.ServiceRegistry.Services;
using Nomad.Audio.Interfaces;
using Nomad.Logger;
using Nomad.Core.Logger;
using Nomad.Events;
using Nomad.Console;
using Nomad.CVars;
using Nomad.Audio.Fmod;
using Nomad.Logger.Private.Sinks;
using Game.Application.Configuration.Registries;

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
		public IServiceLocator ServiceLocator => _serviceLocator;
		private IServiceLocator _serviceLocator;

		public IServiceRegistry ServicesFactory => _serviceFactory;
		private ServiceCollection _serviceFactory;

		private IAudioDevice _audioService;
		private IChannelRepository _channelRepository;

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

			_serviceFactory = new ServiceCollection();
			_serviceLocator = new ServiceLocator( _serviceFactory );

			LoggerBootstrapper.Initialize( _serviceFactory, _serviceLocator );
			var logger = _serviceLocator.GetService<ILoggerService>();

			logger.PrintLine( "NomadBootstrapper: Initializing NomadBackend..." );

			EventSystemBootstrapper.Initialize( _serviceLocator, _serviceFactory );
			ConsoleBootstrapper.Initialize( _serviceLocator, _serviceFactory, GetTree().Root );

			var cvarSystem = _serviceLocator.GetService<ICVarSystemService>();
			AudioCVars.Register( cvarSystem );

			logger.AddSink( new FileSink( cvarSystem ) );

			_serviceFactory.RegisterSingleton<IGraphicsSettingsService>( new GraphicsSettingsService( cvarSystem ) );
			_serviceFactory.RegisterSingleton<IDisplaySettingsService>( new DisplaySettingsService( cvarSystem, new GodotDisplay( GetViewport().GetViewportRid(), cvarSystem, logger ) ) );

			FMODBootstrapper.Initialize( _serviceLocator, _serviceFactory );
			_serviceFactory.RegisterSingleton<IAudioSettingsService>( new AudioSettingsService( cvarSystem, _serviceLocator.GetService<IAudioDevice>() ) );

			_audioService = _serviceLocator.GetService<IAudioDevice>();
			_channelRepository = _serviceLocator.GetService<IChannelRepository>();

			if ( FileAccess.FileExists( "user://settings.ini" ) ) {
				cvarSystem.Load( "user://settings.ini" );
			}
		}

		/*
		===============
		_Process
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ) {
			base._Process( delta );

			float deltaTime = (float)delta;
			_audioService?.Update( deltaTime );
			_channelRepository?.Update( deltaTime );
		}

		public override void _Notification( int what ) {
			base._Notification( what );

			switch ( (long)what ) {
				case NotificationWMCloseRequest:
					System.Environment.Exit( 0 );
					break;
			}
		}

		/*
		===============
		_ExitTree
		===============
		*/
		public override void _ExitTree() {
			base._ExitTree();

			_serviceFactory?.Dispose();
			_serviceLocator?.Dispose();
		}
	};
};