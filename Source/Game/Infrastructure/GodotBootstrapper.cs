/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Godot;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Audio.Interfaces;
using Nomad.Logger;
using Nomad.Events;
using Nomad.CVars;
using Nomad.Logger.Private.Sinks;
using Nomad.Core;
using Nomad.Core.CVars;
using Nomad.EngineUtils;
using Nomad.FileSystem;
using Nomad.Core.Logger;
using Nomad.Core.FileSystem;
using System;
using Nomad.OnlineServices.Steam;
using Nomad.Audio.Fmod;
using Nomad.Console;
using Nomad.Input;
using Nomad.Save;
using Nomad.Networking;

namespace Nomad.Game.Infrastructure
{
	/*
	===================================================================================

	NomadBootstrapper

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class GodotBootstrapper : Node
	{
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
		public override void _Ready()
		{
			base._Ready();

			GD.Print( $"Initializing NomadFramework... {AppContext.BaseDirectory}, {System.Environment.CurrentDirectory}" );

			var serviceFactory = ServiceRegistry.Instance;
			var serviceLocator = ServiceLocator.Instance;

			_bootstrapper = new NomadFrameworkBootstrapper( serviceFactory, serviceLocator )
				.AddBootstrapper( new LoggerBootstrapper() )
				.AddBootstrapper( new EventBootstrapper() )
				.AddBootstrapper( new CVarBootstrapper() )
				.AddBootstrapper( new EngineBootstrapper() )
				.AddBootstrapper( new FileSystemBootstrapper() )
				.AddBootstrapper( new SteamBootstrapper() )
				.AddBootstrapper( new ConsoleBootstrapper() )
				.AddBootstrapper( new FMODBootstrapper() )
				.AddBootstrapper( new InputBootstrapper() )
				.AddBootstrapper( new SaveBootstrapper() )
				.AddBootstrapper( new NetworkBootstrapper() );

			_bootstrapper.Bootstrap();

			var cvarSystem = serviceLocator.GetService<ICVarSystemService>();

			_audioService = serviceLocator.GetService<IAudioDevice>();
			_channelRepository = serviceLocator.GetService<IChannelRepository>();

			var fileSystem = serviceLocator.GetService<IFileSystem>();
			var configFile = cvarSystem.Register(
				new CVarCreateInfo<string> {
					Name = "game.ConfigPath",
					DefaultValue = $"{fileSystem.GetUserDataPath()}/UserConfig.ini",
					Description = "The path to the configuration file.",
					Flags = CVarFlags.Init | CVarFlags.ReadOnly
				}
			);

			if ( fileSystem.FileExists( configFile.Value ) ) {
				cvarSystem.Load( fileSystem, configFile.Value );
			}

			ProcessMode = ProcessModeEnum.Always;
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
		public override void _Process( double delta )
		{
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
		/// <summary>
		///
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree();

			_bootstrapper?.Dispose();
		}
	};
};
