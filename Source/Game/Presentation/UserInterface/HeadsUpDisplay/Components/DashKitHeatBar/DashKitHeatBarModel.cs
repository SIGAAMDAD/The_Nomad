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
using System.Drawing;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Application.Configuration.Enums;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.DashKitHeatBar
{
	/*
	===================================================================================
	
	DashKitHeatBarModel
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class DashKitHeatBarModel : IDashKitHeatBarModel
	{
		public float BurnoutAmount { get; private set; }
		public float MaxBurnout { get; private set; }
		public HUDPreset Preset { get; private set; }
		public Color Color { get; private set; }

		private readonly IDisposable _resourceChanged;
		private readonly IDisposable _dashModuleChanged;

		/*
		===============
		DashKitHeatBarModel
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public DashKitHeatBarModel( IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_resourceChanged = eventFactory
				.GetEvent<PlayerResourceChangedEventArgs>( $"{Constants.LOCAL_GUID}:{PlayerResourceChangedEventArgs.Name}", PlayerResourceChangedEventArgs.NameSpace )
				.Subscribe( OnResourceChanged );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			_resourceChanged.Dispose();
			_dashModuleChanged.Dispose();
		}

		/*
		===============
		OnResourceChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnResourceChanged( in PlayerResourceChangedEventArgs args )
		{
			if ( args.Resource != PlayerResourceType.JumpKitHeat ) {
				return;
			}
			BurnoutAmount = args.NewValue;
		}
	};
};
