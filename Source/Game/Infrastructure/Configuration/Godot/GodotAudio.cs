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

using Game.Infrastructure.Configuration.Interfaces;
using Godot;
using NomadCore.Abstractions.Services;
using System;

namespace Game.Infrastructure.Configuration.Godot {
	/*
	===================================================================================
	
	GodotAudio
	
	===================================================================================
	*/
	/// <summary>
	/// Handles godot audio driver functionalities.
	/// </summary>
	
	public sealed class GodotAudio : IAudioConfig {
		public string AudioDriver => _audioDriverName;
		private readonly string _audioDriverName;

		public int OutputDevice => _outputDeviceIndex;
		private int _outputDeviceIndex;

		public string[] OutputAudioDevices => _outputAudioDevices;
		private readonly string[] _outputAudioDevices;

		private readonly ILoggerService? _logger;

		/*
		===============
		GodotAudio
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="service"></param>
		/// <param name="logger"></param>
		public GodotAudio( ICVarSystemService? service, ILoggerService? logger ) {
			ArgumentNullException.ThrowIfNull( service );
			ArgumentNullException.ThrowIfNull( logger );

			_logger = logger;

			_audioDriverName = AudioServer.GetDriverName();

			_outputAudioDevices = AudioServer.GetOutputDeviceList();
			for ( int i = 0; i < _outputAudioDevices.Length; i++ ) {
				if ( string.Equals( _outputAudioDevices[ i ], AudioServer.OutputDevice ) ) {
					_outputDeviceIndex = i;
					break;
				}
			}

			_logger?.PrintLine( $"GodotAudio: initialized audio backend using driver '{_audioDriverName}' and output device '{_outputAudioDevices[ _outputDeviceIndex ]}'" );
		}

		/*
		===============
		SetOutputDevice
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="deviceIndex"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void SetOutputDevice( int deviceIndex ) {
			if ( deviceIndex < 0 || deviceIndex >= OutputAudioDevices.Length ) {
				throw new ArgumentOutOfRangeException( nameof( deviceIndex ) );
			}
			_outputDeviceIndex = deviceIndex;
			_logger?.PrintLine( $"GodotAudio.SetOutputDevice: setting audio output device to '{_outputAudioDevices[ _outputDeviceIndex ]}'" );
		}
	};
};