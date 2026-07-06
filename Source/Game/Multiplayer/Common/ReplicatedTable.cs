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
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;

namespace Nomad.Game.Multiplayer.Common
{
	/*
	===================================================================================

	ReplicatedTable

	===================================================================================
	*/
	/// <summary>
	/// Host/client helper for small replicated key-value state.
	///
	/// Host methods advance the local version.
	/// Replicated apply methods set the local version to the host-provided version.
	/// </summary>

	internal sealed class ReplicatedTable<TKey, TValue>
		where TKey : notnull
	{
		public int Count => _values.Count;
		public uint Version => _version;

		private readonly Dictionary<TKey, TValue> _values;
		private readonly EqualityComparer<TValue> _valueComparer;
		private uint _version;

		public ReplicatedTable()
			: this( 0 )
		{
		}

		public ReplicatedTable( int capacity )
		{
			_values = capacity > 0
				? new Dictionary<TKey, TValue>( capacity )
				: new Dictionary<TKey, TValue>();

			_valueComparer = EqualityComparer<TValue>.Default;
		}

		public bool ContainsKey( TKey key )
		{
			return _values.ContainsKey( key );
		}

		/*
		===============
		TryGet
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGet( TKey key, out TValue value )
		{
			return _values.TryGetValue( key, out value );
		}

		/*
		===============
		SetHost
		===============
		*/
		/// <summary>
		/// Host-authoritative set. Advances table version on actual change.
		/// </summary>
		public bool SetHost(
			TKey key,
			TValue value,
			out ReplicatedTableChange<TKey, TValue> change
		)
		{
			bool hadPrevious = _values.TryGetValue( key, out TValue previous );

			if ( hadPrevious && _valueComparer.Equals( previous, value ) ) {
				change = default;
				return false;
			}

			_values[key] = value;

			uint version = AdvanceVersion();

			change = new ReplicatedTableChange<TKey, TValue>(
				ReplicatedTableChangeType.Set,
				key,
				previous,
				value,
				hadPrevious,
				version
			);

			return true;
		}

		/*
		===============
		ApplyReplicatedSet
		===============
		*/
		/// <summary>
		/// Client-side replicated set. Applies only if version is newer.
		/// </summary>
		public bool ApplyReplicatedSet(
			TKey key,
			TValue value,
			uint version,
			out ReplicatedTableChange<TKey, TValue> change
		)
		{
			if ( IsStaleVersion( version ) ) {
				change = default;
				return false;
			}

			bool hadPrevious = _values.TryGetValue( key, out TValue previous );

			if ( hadPrevious && _valueComparer.Equals( previous, value ) && version == _version ) {
				change = default;
				return false;
			}

			_values[key] = value;
			_version = version;

			change = new ReplicatedTableChange<TKey, TValue>(
				ReplicatedTableChangeType.Set,
				key,
				previous,
				value,
				hadPrevious,
				version
			);

			return true;
		}

		/*
		===============
		RemoveHost
		===============
		*/
		/// <summary>
		/// Host-authoritative remove. Advances table version if the key existed.
		/// </summary>
		public bool RemoveHost(
			TKey key,
			out ReplicatedTableChange<TKey, TValue> change
		)
		{
			if ( !_values.TryGetValue( key, out TValue previous ) ) {
				change = default;
				return false;
			}

			_values.Remove( key );

			uint version = AdvanceVersion();

			change = new ReplicatedTableChange<TKey, TValue>(
				ReplicatedTableChangeType.Remove,
				key,
				previous,
				default,
				true,
				version
			);

			return true;
		}

		/*
		===============
		ApplyReplicatedRemove
		===============
		*/
		/// <summary>
		/// Client-side replicated remove. Applies only if version is newer.
		/// </summary>
		public bool ApplyReplicatedRemove(
			TKey key,
			uint version,
			out ReplicatedTableChange<TKey, TValue> change
		)
		{
			if ( IsStaleVersion( version ) ) {
				change = default;
				return false;
			}

			bool hadPrevious = _values.TryGetValue( key, out TValue previous );
			if ( hadPrevious ) {
				_values.Remove( key );
			}

			_version = version;

			change = new ReplicatedTableChange<TKey, TValue>(
				ReplicatedTableChangeType.Remove,
				key,
				previous,
				default,
				hadPrevious,
				version
			);

			return true;
		}

		/*
		===============
		ClearHost
		===============
		*/
		public bool ClearHost( out uint version )
		{
			if ( _values.Count == 0 ) {
				version = _version;
				return false;
			}

			_values.Clear();
			version = AdvanceVersion();
			return true;
		}

		/*
		===============
		ApplyReplicatedClear
		===============
		*/
		public bool ApplyReplicatedClear( uint version )
		{
			if ( IsStaleVersion( version ) ) {
				return false;
			}

			_values.Clear();
			_version = version;
			return true;
		}

		/*
		===============
		ResetLocal
		===============
		*/
		/// <summary>
		/// Clears all state without producing a replicated change.
		/// Use on session shutdown/local teardown.
		/// </summary>
		public void ResetLocal()
		{
			_values.Clear();
			_version = 0;
		}

		public bool IsStaleVersion( uint version )
		{
			if ( version == 0 || _version == 0 ) {
				return false;
			}

			return version <= _version;
		}

		public int CopyKeys( TKey[] destination )
		{
			ArgumentGuard.ThrowIfNull( destination, nameof( destination ) );

			int index = 0;
			foreach ( TKey key in _values.Keys ) {
				if ( index >= destination.Length ) {
					break;
				}

				destination[index++] = key;
			}

			return index;
		}

		public int CopyValues( TValue[] destination )
		{
			ArgumentGuard.ThrowIfNull( destination, nameof( destination ) );

			int index = 0;
			foreach ( TValue value in _values.Values ) {
				if ( index >= destination.Length ) {
					break;
				}

				destination[index++] = value;
			}

			return index;
		}

		public int CopyEntries( ReplicatedTableEntry<TKey, TValue>[] destination )
		{
			ArgumentGuard.ThrowIfNull( destination, nameof( destination ) );

			int index = 0;
			foreach ( KeyValuePair<TKey, TValue> pair in _values ) {
				if ( index >= destination.Length ) {
					break;
				}

				destination[index++] = new ReplicatedTableEntry<TKey, TValue>(
					pair.Key,
					pair.Value
				);
			}

			return index;
		}

		private uint AdvanceVersion()
		{
			unchecked {
				_version++;
			}

			return _version;
		}
	}
};
