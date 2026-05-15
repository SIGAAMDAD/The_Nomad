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
using Nomad.Core.Events;
using Nomad.Core.Input;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Events.Extensions;
using Nomad.Events.Globals;
using Nomad.Game.Presentation.Widgets.NomadLabel;
using Nomad.Input.Interfaces;
using Nomad.Input.ValueObjects;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================

	BindingButton

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class BindingButton : EngineHorizontalContainer
	{
		private ISubscriptionHandle _keyboardEvent;

		private bool _isRebinding = false;
		private int _rebindIndex = 0;

		private InputActionDefinition? _action = null;

		/*
		===============
		SetBind
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="mapping"></param>
		/// <param name="bindName"></param>
		public void SetBind( string mapping, string bindName )
		{
			var title = GetNode<NomadLabel>( "Title" );
			title.Text = bindName;

			var firstBind = GetNode<Button>( "Bind1" );
			var secondBind = GetNode<Button>( "Bind2" );

			_action = FindActionDefinition( mapping, bindName );
			if ( _action == null ) {
				firstBind.Text = secondBind.Text = "UNBOUND";
				return;
			}

			if ( _action.Bindings.Length >= 1 ) {
				firstBind.Text = _action.Bindings[0].ToString();
			}
			if ( _action.Bindings.Length >= 2 ) {
				secondBind.Text = _action.Bindings[1].ToString();
			} else {
				secondBind.Text = "UNBOUND";
			}
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void OnInit()
		{
			base.OnInit();

			_keyboardEvent = GameEventRegistry
				.GetEvent<KeyboardEventArgs>( KeyboardEventArgs.Name, KeyboardEventArgs.NameSpace )
				.Where( e => _isRebinding )
				.Subscribe( OnSetBindKey );
		}

		private void OnSetBindKey( in KeyboardEventArgs args )
		{
			if ( _action == null ) {
				return;
			}
		}

		/*
		===============
		FindActionDefinition
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="mapping"></param>
		/// <param name="bindName"></param>
		/// <returns></returns>
		private static InputActionDefinition? FindActionDefinition( string mapping, string bindName )
		{
			var bindResolver = ServiceLocator.GetService<IBindResolver>();
			var actions = bindResolver.GetBindMapping( mapping );

			if ( actions == null ) {
				return null;
			}
			for ( int i = 0; i < actions.Count; i++ ) {
				if ( actions[i].Name.Equals( bindName, System.StringComparison.InvariantCulture ) ) {
					return actions[i];
				}
			}
			return null;
		}
	};
};
