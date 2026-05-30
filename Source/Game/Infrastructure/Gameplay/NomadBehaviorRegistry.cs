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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Infrastructure.Gameplay
{
	/*
	===================================================================================

	NomadBehaviorRegistry

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class NomadBehaviorRegistry : INomadBehaviorRegistry
	{
		private readonly struct BehaviorKey : IEquatable<BehaviorKey>
		{
			public readonly Type BehaviorType;
			public readonly InternString BehaviorId;

			public BehaviorKey( Type behaviorType, InternString behaviorId )
			{
				BehaviorType = behaviorType;
				BehaviorId = behaviorId;
			}

			public bool Equals( BehaviorKey other )
			{
				return BehaviorType == other.BehaviorType && BehaviorId.Equals( other.BehaviorId );
			}

			public override bool Equals( object? obj )
			{
				return obj is BehaviorKey other && Equals( other );
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(
					BehaviorType.GetHashCode(),
					BehaviorId.GetHashCode()
				);
			}
		};

		private interface IBehaviorFactoryEntry
		{
			Type BehaviorType { get; }
			Type DefinitionType { get; }
			object Create( IModuleContext? context, object definition );
		}

		private sealed class BehaviorFactoryEntry<TBehavior, TDefinition> : IBehaviorFactoryEntry
			where TBehavior : class
		{
			private readonly NomadBehaviorFactory<TBehavior, TDefinition> _factory;
			private readonly IModuleContext? _ownerContext;

			public Type BehaviorType => typeof( TBehavior );
			public Type DefinitionType => typeof( TDefinition );

			public BehaviorFactoryEntry(
				IModuleContext? ownerContext,
				NomadBehaviorFactory<TBehavior, TDefinition> factory
			)
			{
				_ownerContext = ownerContext;
				_factory = factory;
			}

			public TBehavior Create( IModuleContext? context, TDefinition definition )
			{
				IModuleContext resolvedContext = context ?? _ownerContext ??
					throw new InvalidOperationException(
						$"Behavior '{typeof( TBehavior ).Name}' was registered without an owning module context."
					);

				return _factory( resolvedContext, definition ) ??
					throw new InvalidOperationException( $"Behavior factory for '{typeof( TBehavior ).Name}' returned null." );
			}

			object IBehaviorFactoryEntry.Create( IModuleContext? context, object definition )
			{
				return Create( context, (TDefinition)definition );
			}
		}

		private readonly ConcurrentDictionary<BehaviorKey, IBehaviorFactoryEntry> _factories = new();

		/*
		===============
		Add
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="behaviorId"></param>
		/// <param name="factory"></param>
		public void Add<TBehavior, TDefinition>(
			string behaviorId,
			NomadBehaviorFactory<TBehavior, TDefinition> factory
		)
			where TBehavior : class
		{
			Add( new InternString( behaviorId ), factory );
		}

		/*
		===============
		Add
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="behaviorId"></param>
		/// <param name="factory"></param>
		public void Add<TBehavior, TDefinition>(
			InternString behaviorId,
			NomadBehaviorFactory<TBehavior, TDefinition> factory
		)
			where TBehavior : class
		{
			Register( behaviorId, factory );
		}

		/*
		===============
		Register
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="behaviorId"></param>
		/// <param name="factory"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Register<TBehavior, TDefinition>(
			InternString behaviorId,
			NomadBehaviorFactory<TBehavior, TDefinition> factory
		)
			where TBehavior : class
		{
			ArgumentGuard.ThrowIfNull( factory, nameof( factory ) );

			var behaviorKey = new BehaviorKey( typeof( TBehavior ), behaviorId );
			var entry = new BehaviorFactoryEntry<TBehavior, TDefinition>( null, factory );

			if ( !_factories.TryAdd( behaviorKey, entry ) ) {
				throw new InvalidOperationException( $"Behavior '{(string)behaviorId}' registered twice." );
			}
		}

		/*
		===============
		Register
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="behaviorId"></param>
		/// <param name="ownerContext"></param>
		/// <param name="factory"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Register<TBehavior, TDefinition>(
			InternString behaviorId,
			IModuleContext ownerContext,
			NomadBehaviorFactory<TBehavior, TDefinition> factory
		)
			where TBehavior : class
		{
			ArgumentGuard.ThrowIfNull( ownerContext, nameof( ownerContext ) );
			ArgumentGuard.ThrowIfNull( factory, nameof( factory ) );

			var behaviorKey = new BehaviorKey( typeof( TBehavior ), behaviorId );
			var entry = new BehaviorFactoryEntry<TBehavior, TDefinition>( ownerContext, factory );

			if ( !_factories.TryAdd( behaviorKey, entry ) ) {
				throw new InvalidOperationException( $"Behavior '{(string)behaviorId}' registered twice." );
			}
		}

		/*
		===============
		Contains
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <param name="behaviorId"></param>
		/// <returns></returns>
		public bool Contains<TBehavior>( string behaviorId )
			where TBehavior : class
		{
			return Contains<TBehavior>( new InternString( behaviorId ) );
		}

		/*
		===============
		Contains
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <param name="behaviorId"></param>
		/// <returns></returns>
		public bool Contains<TBehavior>( InternString behaviorId )
			where TBehavior : class
		{
			return _factories.ContainsKey( new BehaviorKey( typeof( TBehavior ), behaviorId ) );
		}

		/*
		===============
		Create
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="behaviorId"></param>
		/// <param name="context"></param>
		/// <param name="definition"></param>
		/// <returns></returns>
		public TBehavior Create<TBehavior, TDefinition>(
			InternString behaviorId,
			TDefinition definition
		)
			where TBehavior : class
		{
			if ( TryCreate( behaviorId, definition, out TBehavior behavior ) ) {
				return behavior;
			}

			throw new KeyNotFoundException( $"Behavior '{(string)behaviorId}' was not registered for '{typeof( TBehavior ).Name}'." );
		}

		/*
		===============
		Create
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="behaviorId"></param>
		/// <param name="context"></param>
		/// <param name="definition"></param>
		/// <returns></returns>
		public TBehavior Create<TBehavior, TDefinition>(
			InternString behaviorId,
			IModuleContext context,
			TDefinition definition
		)
			where TBehavior : class
		{
			if ( TryCreate( behaviorId, context, definition, out TBehavior behavior ) ) {
				return behavior;
			}

			throw new KeyNotFoundException( $"Behavior '{(string)behaviorId}' was not registered for '{typeof( TBehavior ).Name}'." );
		}

		/*
		===============
		TryCreate
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="behaviorId"></param>
		/// <param name="definition"></param>
		/// <param name="behavior"></param>
		/// <returns></returns>
		public bool TryCreate<TBehavior, TDefinition>(
			InternString behaviorId,
			TDefinition definition,
			out TBehavior behavior
		)
			where TBehavior : class
		{
			return TryCreate( behaviorId, null, definition, out behavior );
		}

		/*
		===============
		TryCreate
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TBehavior"></typeparam>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="behaviorId"></param>
		/// <param name="context"></param>
		/// <param name="definition"></param>
		/// <param name="behavior"></param>
		/// <returns></returns>
		public bool TryCreate<TBehavior, TDefinition>(
			InternString behaviorId,
			IModuleContext? context,
			TDefinition definition,
			out TBehavior behavior
		)
			where TBehavior : class
		{
			var behaviorKey = new BehaviorKey( typeof( TBehavior ), behaviorId );

			if ( _factories.TryGetValue( behaviorKey, out var entry ) ) {
				if ( entry is not BehaviorFactoryEntry<TBehavior, TDefinition> typedEntry ) {
					throw new InvalidOperationException(
						$"Behavior '{(string)behaviorId}' was registered for definition type '{entry.DefinitionType.Name}', " +
						$"but was requested with '{typeof( TDefinition ).Name}'."
					);
				}

				behavior = typedEntry.Create( context, definition );
				return true;
			}

			behavior = null!;
			return false;
		}
	};
};
