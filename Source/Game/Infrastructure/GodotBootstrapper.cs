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
using Nomad.Core;
using Nomad.EngineUtils;
using Nomad.FileSystem;
using Nomad.Core.OnlineServices;
using System;
using Nomad.OnlineServices.Steam;
using Nomad.Audio.Fmod;
using Nomad.Console;
using Nomad.Input;
using Nomad.Save;
using Nomad.Networking;
using Nomad.Game.Presentation.Screens;
using Nomad.Game.Infrastructure.Root;
using Nomad.Game.Infrastructure.Godot;

namespace Nomad.Game.Infrastructure
{
	/*
	===================================================================================

	GodotBootstrapper

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class GodotBootstrapper : Node
	{
		private GodotFrameworkStartup _startup;
		private GodotFramePump? _framePump;

		/*
		===============
		_EnterTree
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _EnterTree()
		{
			base._EnterTree();

			ProcessMode = ProcessModeEnum.Always;

			_startup = new GodotFrameworkStartup();

			StartupContext context = _startup.Bootstrap();

			ConfigStartup.Configure( context.ServiceLocator );

			_framePump = new GodotFramePump( context.ServiceLocator );

			ScreenPresenterFactory.Initialize( context.ServiceLocator );
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

			_framePump?.Frame( (float)delta );
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

			_framePump = null;

			_startup?.Dispose();
			_startup = null;
		}
	};
};
