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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Save.Services;

namespace Nomad.Game.Infrastructure.Gameplay
{
	/*
	===================================================================================

	DataInstanceRegistry

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class DataInstanceRegistry<TInstanceId, TInstance> : IDisposable
		where TInstanceId : struct
		where TInstance : class, IDisposable
	{
		protected abstract string LoggerCategoryName { get; }

		protected readonly ILoggerCategory category;
		protected readonly Dictionary<TInstanceId, TInstance> dataCache = new();

		private readonly IDisposable _saveBegin;
		private readonly IDisposable _loadBegin;

		private bool _isDisposed = false;

		/*
		===============
		DataInstanceRegistry
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fileSystem"></param>
		/// <param name="logger"></param>
		/// <exception cref="ArgumentNullException"></exception>
		protected DataInstanceRegistry( ILoggerService logger, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( logger, nameof( logger ) );

			category = logger.CreateCategory( LoggerCategoryName, LogLevel.Info, true );

			_saveBegin = eventFactory
				.GetEvent<SaveBeginEventArgs>(
					SaveBeginEventArgs.Name,
					SaveBeginEventArgs.NameSpace
				)
				.Subscribe( OnSaveBegin );

			_loadBegin = eventFactory
				.GetEvent<LoadBeginEventArgs>(
					LoadBeginEventArgs.Name,
					LoadBeginEventArgs.NameSpace
				)
				.Subscribe( OnLoadBegin );
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

			_saveBegin.Dispose();
			_loadBegin.Dispose();
			category.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		Clear
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Clear()
		{
			dataCache.Clear();
		}

		/*
		===============
		Get
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public TInstance? Get( TInstanceId itemId )
		{
			return dataCache.TryGetValue( itemId, out var item ) ? item : null;
		}

		/*
		===============
		Get
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public bool TryGet( TInstanceId itemId, out TInstance? instance )
		{
			return dataCache.TryGetValue( itemId, out instance );
		}

		/*
		===============
		TryAdd
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public bool TryAdd( TInstanceId itemId, TInstance instance )
		{
			return dataCache.TryAdd( itemId, instance );
		}

		protected abstract void OnSaveBegin( in SaveBeginEventArgs args );
		protected abstract void OnLoadBegin( in LoadBeginEventArgs args );
	};
};
