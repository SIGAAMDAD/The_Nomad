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

using System.Collections.Generic;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Input.Interfaces;
using Nomad.Input.ValueObjects;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	BindingButton
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class BindingButton : EngineHorizontalContainer {
		public string Mapping { private get; set; }
		public string BindName { private get; set; }

		private InputActionDefinition? FindActionDefinition() {
			var bindResolver = ServiceLocator.GetService<IBindResolver>();
			var actions = bindResolver.GetBindMapping( Mapping );

			for ( int i = 0; i < actions.Count; i++ ) {
				if ( actions[ i ].Name.Equals( BindName, System.StringComparison.InvariantCulture ) ) {
					return actions[ i ];
				}
			}
			return null;
		}
		
		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			base.OnInit();

			var firstBind = FindChild<NomadButton>( "Bind1" );
			var secondBind = FindChild<NomadButton>( "Bind2" );

			InputActionDefinition? action = FindActionDefinition();
			if ( action == null ) {
				firstBind.Text = secondBind.Text = "UNBOUND";
				return;
			}

			if ( action.Bindings.Length >= 1 ) {
				firstBind.Text = action.Bindings[ 0 ].
			}
			if ( action.Bindings.Length >= 2 ) {
				
			}

			firstBind.Text = "";
		}
	};
};
