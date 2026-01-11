/*
===========================================================================
The Nomad Framework
Copyright (C) 2025 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Game.Application.Common.Models.ValueObjects;
using Game.Application.Configuration.Enums;
using Godot;
using Nomad.Core.Abstractions;

namespace Game.Infrastructure.Configuration.Godot.ValueObjects {
	/*
	===================================================================================
	
	Monitor
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public readonly record struct Monitor : IValueObject<Monitor> {
		public int MonitorIndex => _monitorIndex;
		private readonly int _monitorIndex;
		
		public int RefreshRate => _refreshRate;
		private readonly int _refreshRate;

		public WindowResolution ScreenSize => _screenSize;
		private readonly WindowResolution _screenSize;

		/*
		===============
		Monitor
		===============
		*/
		/// <summary>
		/// Creates a Monitor object
		/// </summary>
		/// <param name="monitorIndex"></param>
		public Monitor( int monitorIndex ) {
			_monitorIndex = monitorIndex;
			_refreshRate = (int)DisplayServer.ScreenGetRefreshRate( monitorIndex );
			_screenSize = (WindowResolution)(WindowSize)DisplayServer.ScreenGetSize( monitorIndex );
		}
	};
};