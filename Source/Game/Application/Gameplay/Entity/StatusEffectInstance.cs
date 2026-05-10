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
using System.Timers;
using Nomad.Game.Domain.Data.Entities;

namespace Nomad.Game.Application.Gameplay.Entity
{
	/*
	===================================================================================

	StatusEffectInstance

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class StatusEffectInstance
	{
		private readonly StatusEffectDefinition _definition;
		private readonly Timer _timer;

		public StatusEffectInstance( StatusEffectDefinition definition )
		{
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );

			_timer = new Timer() {
				Enabled = true,
				AutoReset = false,
				Interval = definition.Duration
			};
			_timer.Elapsed += OnFinished;
		}

		public void Stack()
		{
			switch ( _definition.DurationPolicy ) {
				case StatusEffectDurationPolicy.Fixed:
					break; // do nothing
				case StatusEffectDurationPolicy.RefreshOnReapply:
					_timer.Stop();
					_timer.Start();
					break;
				case StatusEffectDurationPolicy.ExtendOnReapply:
					_timer.Interval += _definition.Duration;
					break;
			}
		}

		/*
		===============
		OnFinished
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFinished( object? sender, ElapsedEventArgs e )
		{
		}
	};
};
