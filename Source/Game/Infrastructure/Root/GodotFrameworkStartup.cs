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

using System;
using Godot;
using Nomad.Audio.Fmod;
using Nomad.Console;
using Nomad.Core;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.CVars;
using Nomad.EngineUtils;
using Nomad.Events;
using Nomad.FileSystem;
using Nomad.Input;
using Nomad.Logger;
using Nomad.Networking;
using Nomad.OnlineServices.Steam;
using Nomad.Save;

namespace Nomad.Game.Infrastructure.Root
{
	/*
	===================================================================================

	GodotFrameworkStartup

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class GodotFrameworkStartup : IDisposable
	{
		private NomadFrameworkBootstrapper? _bootstrapper;

		public StartupContext Bootstrap()
		{
			GD.Print(
				$"Initializing NomadFramework..."
			);

			var serviceFactory = ServiceRegistry.Instance;
			var serviceLocator = ServiceLocator.Instance;

			_bootstrapper = CreateBootstrapper( serviceFactory, serviceLocator );
			_bootstrapper.Bootstrap();

			return new StartupContext {
				ServiceRegistry = serviceFactory,
				ServiceLocator = serviceLocator,
				Bootstrapper = _bootstrapper
			};
		}

		public void Dispose()
		{
			_bootstrapper?.Dispose();
			_bootstrapper = null;
		}

		private static NomadFrameworkBootstrapper CreateBootstrapper( IServiceRegistry serviceFactory, IServiceLocator serviceLocator )
		{
			return new NomadFrameworkBootstrapper( serviceFactory, serviceLocator )
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
		}
	};
};
