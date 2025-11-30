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

using Game.Application.Common.Interfaces;
using Godot;
using NomadCore.Abstractions.Services;
using System.Collections.Concurrent;

namespace Game.Infrastructure.Godot {
	/*
	===================================================================================
	
	GodotLocalizationService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class GodotLocalizationService : ILocalizationService {
		private readonly ConcurrentDictionary<string, string> _translations = new ConcurrentDictionary<string, string>();
		private readonly ILoggerService _logger;

		public GodotLocalizationService( ILoggerService logger ) {
			_logger = logger;

			TranslationServer.SetLocale( GetSystemLanguage() );
		}

		public string GetCurrentLanguage() {
			throw new System.NotImplementedException();
		}

		public string Translate( string key, params string[] args ) {
			return null;
		}

		public string TranslatePlural( string key, params string[] args ) {
			throw new System.NotImplementedException();
		}

		private string GetSystemLanguage() {
			return OS.GetLocale();
		}
	};
};