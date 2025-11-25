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

using System;
using CVars;

namespace Menus.Settings.Options {
	/*
	===================================================================================
	
	NetworkingOptionManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class NetworkingOptionManager : OptionContainer {
		private NetworkingConfig? Temp;

		private SelectionNodes.OptionCheckbox? CODLobbies;
		private SelectionNodes.OptionCheckbox? BountyHuntEnabled;
		private SelectionNodes.OptionCheckbox? ChatFilterEnabled;

		/*
		===============
		NetworkingOptionManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public NetworkingOptionManager( NetworkingConfig? tmp ) {
			ArgumentNullException.ThrowIfNull( tmp );

			Temp = tmp;
		}

		public override void SetConfig( in ConfigHandler config ) {
			Temp = config as NetworkingConfig;
		}

		/*
		===============
		LinkNodes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void LinkNodes() {
			CODLobbies = GetNode<SelectionNodes.OptionCheckbox>( "OptionsContainer/CODLobbies" );
			BountyHuntEnabled = GetNode<SelectionNodes.OptionCheckbox>( "OptionsContainer/BountyHuntEnabled" );
			ChatFilterEnabled = GetNode<SelectionNodes.OptionCheckbox>( "OptionsContainer/ChatFilter" );
		}

		/*
		===============
		OnVisibilityChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnVisibilityChanged() {
		}
	};
};