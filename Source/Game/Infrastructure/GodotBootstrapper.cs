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
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Audio.Interfaces;
using Nomad.Logger;
using Nomad.Events;
using Nomad.CVars;
using Nomad.Logger.Private.Sinks;
using Game.Application.Configuration.Registries;
using Nomad.Core;
using Nomad.Core.CVars;
using Nomad.EngineUtils;
using Nomad.FileSystem;
using Nomad.Core.Logger;
using Nomad.Core.FileSystem;
using System;
using Nomad.OnlineServices.Steam;
using Nomad.Audio.Fmod;
using Nomad.Core.EngineUtils;

namespace Game.Infrastructure {
	/*
	===================================================================================

	NomadBootstrapper

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class GodotBootstrapper : Node {
		private IAudioDevice _audioService;
		private IChannelRepository _channelRepository;

		private NomadFrameworkBootstrapper _bootstrapper;

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

			GD.Print( $"Initializing NomadFramework... {AppContext.BaseDirectory}, {System.Environment.CurrentDirectory}" );

			var serviceFactory = ServiceRegistry.Instance;
			var serviceLocator = ServiceLocator.Instance;
			
			_bootstrapper = new NomadFrameworkBootstrapper( serviceFactory, serviceLocator )
				.AddBootstrapper( new LoggerBootstrapper() )
				.AddBootstrapper( new EventBootstrapper() )
				.AddBootstrapper( new CVarBootstrapper() )
				.AddBootstrapper( new EngineServiceBootstrapper() )
				.AddBootstrapper( new FileSystemBootstrapper() )
				.AddBootstrapper( new SteamBootstrapper() );

			_bootstrapper.Bootstrap();

			var cvarSystem = serviceLocator.GetService<ICVarSystemService>();
			AudioCVars.Register( cvarSystem );

			var logger = serviceLocator.GetService<ILoggerService>();
			logger.AddSink( new FileSink( cvarSystem, serviceLocator.GetService<IFileSystem>() ) );

			FMODBootstrapper.Initialize( serviceLocator, serviceFactory );

			_audioService = serviceLocator.GetService<IAudioDevice>();
			_channelRepository = serviceLocator.GetService<IChannelRepository>();

			var fileSystem = serviceLocator.GetService<IFileSystem>();
			var engineService = serviceLocator.GetService<IEngineService>();
			var configFile = cvarSystem.Register(
				new CVarCreateInfo<string> {
					Name = Nomad.Core.Constants.CVars.Console.DEFAULT_CONFIG_FILE,
					DefaultValue = engineService.GetStoragePath( "Config/default.ini", StorageScope.StreamingAssets ),
					Description = "The default configuration file.",
					Flags = CVarFlags.Init | CVarFlags.ReadOnly
				}
			);
			if ( fileSystem.FileExists( configFile.Value ) ) {
				cvarSystem.Load( fileSystem, configFile.Value );
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

		/*
		===============
		_ExitTree
		===============
		*/
		public override void _ExitTree() {
			base._ExitTree();

			_bootstrapper?.Dispose();
		}
	};
};
