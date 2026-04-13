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
using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerFlagService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerFlagService : IPlayerFlagService {
		public IReadOnlyList<string> CurrentFlags => GetActiveFlags();

		private PlayerFlags _flags = PlayerFlags.None;

		private bool _isDisposed = false;

		public IGameEvent<PlayerFlagsChangedEventArgs> FlagsChanged => _flagsChanged;
		private readonly IGameEvent<PlayerFlagsChangedEventArgs> _flagsChanged;

		/*
		===============
		PlayerFlagService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		public PlayerFlagService( IGameEventRegistryService eventFactory ) {
			_flagsChanged = eventFactory
				.GetEvent<PlayerFlagsChangedEventArgs>( EventNames.PLAYER_FLAGS_CHANGED, EventNames.NAMESPACE );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			if ( !_isDisposed ) {
				_flagsChanged?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		GetFlags
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool GetFlags( PlayerFlags flag ) {
			return ( _flags & flag ) != 0;
		}

		/*
		===============
		ClearFlags
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void ClearFlags() {
			_flags = PlayerFlags.None;
		}

		/*
		===============
		AddFlags
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="flags"></param>
		public void AddFlags( PlayerFlags flags ) {
			_flags |= flags;
		}

		/*
		===============
		RemoveFlags
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="flags"></param>
		public void RemoveFlags( PlayerFlags flags ) {
			_flags &= ~flags;
		}

		/*
		===============
		ApplyFlags
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="flags"></param>
		public void ApplyFlags( IReadOnlyList<string> flags ) {
			ClearFlags();

			for ( int i = 0; i < flags.Count; i++ ) {
				if ( Enum.TryParse( typeof( PlayerFlags ), flags[ i ], out var flag ) ) {
					AddFlags( (PlayerFlags)flag );
				}
			}
		}

		/*
		===============
		GetActiveFlags
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private List<string> GetActiveFlags() {
			List<string> flags = new List<string>();

			foreach ( var flag in Enum.GetValues<PlayerFlags>() ) {
				if ( _flags.HasFlag( flag ) ) {
					flags.Add( flag.ToString() );
				}
			}
			return flags;
		}
	};
};