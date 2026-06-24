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
using Godot;
using Nomad.Game.Infrastructure.Streaming;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Traversal
{
	internal sealed class TraversalDatabaseProvider : ITraversalDatabaseRegistry
	{
		private const int FallbackSourceId = 0;

		private readonly List<TraversalGraphSource> _activeSources = new List<TraversalGraphSource>( 16 );
		private readonly Dictionary<TraversalDatabase, TraversalGraph> _graphCache = new Dictionary<TraversalDatabase, TraversalGraph>( 32 );

		private TraversalGraphSource _fallbackSource;
		private int _nextSourceId = FallbackSourceId + 1;

		public int Version { get; private set; }

		public int GraphCount => _activeSources.Count > 0
			? _activeSources.Count
			: (_fallbackSource != null ? 1 : 0);

		public TraversalGraphSource GetGraphSource( int index )
		{
			if ( _activeSources.Count > 0 ) {
				return _activeSources[index];
			}

			if ( index == 0 && _fallbackSource != null ) {
				return _fallbackSource;
			}

			throw new ArgumentOutOfRangeException( nameof( index ) );
		}

		public bool TryGetGraph( int sourceId, out TraversalGraph graph )
		{
			if ( _fallbackSource != null && _fallbackSource.SourceId == sourceId ) {
				graph = _fallbackSource.Graph;
				return true;
			}

			for ( int i = 0; i < _activeSources.Count; i++ ) {
				TraversalGraphSource source = _activeSources[i];
				if ( source.SourceId == sourceId ) {
					graph = source.Graph;
					return true;
				}
			}

			graph = null;
			return false;
		}

		public void SetFallbackDatabase( TraversalDatabase database )
		{
			if ( database == null ) {
				if ( _fallbackSource != null ) {
					_fallbackSource = null;
					Version++;
				}

				return;
			}

			TraversalGraph graph;
			try {
				graph = GetOrCreateGraph( database );
			} catch ( Exception ex ) {
				GD.PushWarning( $"Unable to register fallback traversal database: {ex.Message}" );
				return;
			}

			_fallbackSource = new TraversalGraphSource(
				FallbackSourceId,
				default,
				database,
				graph,
				isFallback: true
			);

			Version++;
		}

		public int Register( RegionId regionId, TraversalDatabase database )
		{
			if ( database == null ) {
				return -1;
			}

			TraversalGraph graph;
			try {
				graph = GetOrCreateGraph( database );
			} catch ( Exception ex ) {
				GD.PushWarning( $"Unable to register traversal database for region {regionId}: {ex.Message}" );
				return -1;
			}

			int sourceId = _nextSourceId++;
			_activeSources.Add(
				new TraversalGraphSource(
					sourceId,
					regionId,
					database,
					graph,
					isFallback: false
				)
			);

			Version++;
			return sourceId;
		}

		public void Unregister( int registrationId )
		{
			if ( registrationId < FallbackSourceId + 1 ) {
				return;
			}

			for ( int i = 0; i < _activeSources.Count; i++ ) {
				if ( _activeSources[i].SourceId != registrationId ) {
					continue;
				}

				int last = _activeSources.Count - 1;
				_activeSources[i] = _activeSources[last];
				_activeSources.RemoveAt( last );
				Version++;
				return;
			}
		}

		private TraversalGraph GetOrCreateGraph( TraversalDatabase database )
		{
			if ( _graphCache.TryGetValue( database, out TraversalGraph graph ) ) {
				return graph;
			}

			graph = TraversalGraph.FromDatabase( database );
			_graphCache[database] = graph;
			return graph;
		}
	}
}
