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
using Nomad.Audio.Interfaces;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.OnlineServices;
using Nomad.Core.ServiceRegistry.Interfaces;

namespace Nomad.Game.Infrastructure.Root
{
	/*
	===================================================================================

	GodotFramePump

	===================================================================================
	*/
	/// <summary>
	/// Handles per-frame operations managed by the NomadFramework specifically for
	/// the <see cref="IAudioDevice"/>, <see cref="IChannelRepository"/>, and the
	/// <see cref="IOnlinePlatformService"/>.
	/// </summary>

	internal sealed class GodotFramePump
	{
		private readonly IAudioDevice _audioDevice;
		private readonly IChannelRepository _channelRepository;
		private readonly IOnlinePlatformService _onlinePlatformService;

		/*
		===============
		GodotFramePump
		===============
		*/
		/// <summary>
		/// Creates a GodotFramePump object.
		/// </summary>
		/// <param name="locator">The service locator for fetching the required services.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="locator"/> is null.</exception>
		public GodotFramePump( IServiceLocator locator )
		{
			ArgumentGuard.ThrowIfNull( locator, nameof( locator ) );

			_audioDevice = locator.GetService<IAudioDevice>();
			_channelRepository = locator.GetService<IChannelRepository>();
			_onlinePlatformService = locator.GetService<IOnlinePlatformService>();
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public void Frame( float delta )
		{
			_audioDevice.Update( delta );
			_channelRepository.Update( delta );
			_onlinePlatformService.Frame();
		}
	};
};
