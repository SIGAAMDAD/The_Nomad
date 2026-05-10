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

namespace Game.Infrastructure.UI.Nodes.NomadLabel
{
	/*
	===================================================================================

	NomadLabel

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class NomadLabel : Label
	{
		public bool IsFocused => _isFocused;
		private bool _isFocused = false;

		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void OnFocused()
		{
			_isFocused = true;
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void OnUnfocused()
		{
			_isFocused = false;
		}

		public override void _Ready()
		{
			base._Ready();

			FocusEntered += OnFocused;
			FocusExited += OnUnfocused;
		}
	};
};
