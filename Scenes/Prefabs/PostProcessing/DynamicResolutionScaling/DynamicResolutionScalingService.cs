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

using Game.Application.Common.Models;
using Game.Application.Configuration.Enums;
using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using NomadCore.Interfaces.ConsoleSystem;
using System;
using System.Numerics;

namespace Game.Prefabs {
	/*
	===================================================================================
	
	DynamicResolutionScalingService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class DynamicResolutionScalingService {
		public WindowSize ScreenSize { get; private set; }
		public Vector2 ScaleFactor { get; private set; }

		private float _targetFrames;
		private float _renderScale;

		public DynamicResolutionScalingService() {
			var cvarSystem = ServiceRegistry.Get<ICVarSystemService>();
			var screenSize = cvarSystem.GetCVar<WindowResolution>( "display.WindowResolution" ).Value.GetSize();

			var drsTargetFrames = cvarSystem.GetCVar<int>( "display.DRSTargetFrames" );
			drsTargetFrames.ValueChanged.Subscribe( this, OnTargetFramesChanged );
			_targetFrames = drsTargetFrames.Value;

			ScreenSize = screenSize with { width = screenSize.width / 2, height = screenSize.height / 2 };
			ScaleFactor = new Vector2( screenSize.width / ScreenSize.width, screenSize.height / ScreenSize.height );
		}

		/*
		===============
		Update
		===============
		*/
		public void Update( double currentFps ) {
			float newScale = _renderScale;
			if ( currentFps < _targetFrames * 0.9f ) {
				newScale = Math.Max( 0.5f, newScale - 0.1f );
			} else if ( currentFps < _targetFrames * 1.1f && newScale < 1.0f ) {
				newScale = Math.Min( 1.0f, newScale + 0.1f );
			}
			if ( newScale != _renderScale ) {
				_renderScale = newScale;
				ScaleFactor = new Vector2( _renderScale );
			}
		}

		/*
		===============
		OnTargetFramesChanged
		===============
		*/
		private void OnTargetFramesChanged( in ICVarValueChangedEventData<int> args ) {
			_targetFrames = args.Value;
		}
	};
};