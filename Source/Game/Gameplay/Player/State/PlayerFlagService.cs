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
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Sdk.Multiplayer;

namespace Nomad.Game.Gameplay.Player.State
{
	/*
	===================================================================================

	PlayerFlagService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerFlagService : IPlayerFlagService
	{
		public IReadOnlyList<string> CurrentFlags => GetActiveFlags();

		public PlayerFlags Bits => _isDisposed ? throw new ObjectDisposedException( nameof( PlayerFlagService ) ) : _flags;
		private PlayerFlags _flags = PlayerFlags.None;

		private readonly PlayerId _playerId = PlayerId.Invalid;

		public IGameEvent<PlayerFlagsChangedEventArgs> FlagsChanged => _flagsChanged;
		private readonly IGameEvent<PlayerFlagsChangedEventArgs> _flagsChanged;

		private bool _isDisposed = false;

		/*
		===============
		PlayerFlagService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="eventFactory"></param>
		public PlayerFlagService( PlayerId playerId, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			if ( playerId == PlayerId.Invalid ) {
				throw new InvalidOperationException( "PlayerFlagService given an invalid PlayerId!" );
			}

			_playerId = playerId;

			_flagsChanged = eventFactory
				.GetEvent<PlayerFlagsChangedEventArgs>(
					PlayerFlagsChangedEventArgs.Name,
					PlayerFlagsChangedEventArgs.NameSpace
				);
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_flagsChanged.Dispose();

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
		public bool GetFlags( PlayerFlags flag )
		{
			StateGuard.ThrowIfDisposed( _isDisposed, this );
			return (_flags & flag) != 0;
		}

		/*
		===============
		ClearFlags
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void ClearFlags()
		{
			StateGuard.ThrowIfDisposed( _isDisposed, this );
			if ( _flags == PlayerFlags.None ) {
				return;
			}
			var prevFlags = _flags;
			_flags = PlayerFlags.None;
			_flagsChanged.Publish( new PlayerFlagsChangedEventArgs( _playerId, prevFlags, _flags ) );
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
		public void AddFlags( PlayerFlags flags )
		{
			StateGuard.ThrowIfDisposed( _isDisposed, this );
			if ( _flags.HasFlag( flags ) ) {
				return;
			}
			var prevFlags = _flags;
			_flags |= flags;
			_flagsChanged.Publish( new PlayerFlagsChangedEventArgs( _playerId, prevFlags, _flags ) );
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
		public void RemoveFlags( PlayerFlags flags )
		{
			StateGuard.ThrowIfDisposed( _isDisposed, this );
			if ( !_flags.HasFlag( flags ) ) {
				return;
			}
			var prevFlags = _flags;
			_flags &= ~flags;
			_flagsChanged.Publish( new PlayerFlagsChangedEventArgs( _playerId, prevFlags, _flags ) );
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
		/// <param name="clearFlags"></param>
		public void ApplyFlags( IReadOnlyList<string> flags, bool clearFlags = false )
		{
			StateGuard.ThrowIfDisposed( _isDisposed, this );
			if ( clearFlags ) {
				ClearFlags();
			}
			var prevFlags = _flags;
			for ( int i = 0; i < flags.Count; i++ ) {
				if ( Enum.TryParse( typeof( PlayerFlags ), flags[i], out var flag ) ) {
					_flags |= (PlayerFlags)flag;
				}
			}
			if ( prevFlags != _flags ) {
				_flagsChanged.Publish( new PlayerFlagsChangedEventArgs( _playerId, prevFlags, _flags ) );
			}
		}

		/*
		===============
		SetFlag
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="flagName"></param>
		/// <param name="state"></param>
		public void SetFlag( string flagName, bool state )
		{
			StateGuard.ThrowIfDisposed( _isDisposed, this );
			if ( Enum.TryParse( typeof( PlayerFlags ), flagName, out var flag ) ) {
				AddFlags( (PlayerFlags)flag );
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
		private List<string> GetActiveFlags()
		{
			StateGuard.ThrowIfDisposed( _isDisposed, this );
			List<string> flags = new List<string>();

			foreach ( var flag in Enum.GetValues<PlayerFlags>() ) {
				if ( _flags.HasFlag( flag ) ) {
					flags.Add( Enum.GetName( flag ) );
				}
			}
			return flags;
		}
	};
};
