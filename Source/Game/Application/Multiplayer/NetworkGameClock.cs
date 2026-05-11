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
using System.Diagnostics;

namespace Nomad.Game.Application.Multiplayer
{
	internal sealed class NetworkGameClock
	{
		public const int DEFAULT_TICK_RATE = 60;
		public const int DEFAULT_MAX_TICKS_PER_FRAME = 8;

		public int TickRate => _tickRate;
		private readonly int _tickRate = 0;

		public double TickDeltaSeconds => _tickDeltaSeconds;
		private readonly double _tickDeltaSeconds = 0.0f;

		public long Tick => _tick;
		private long _tick = 0;

		public double TimeSeconds => _tick * _tickDeltaSeconds;

		public bool IsRunning => _isRunning;
		private bool _isRunning = false;

		public int PendingTicks => _pendingTicks;
		private int _pendingTicks = 0;

		public int MaxTicksPerFrame {
			get => _maxTicksPerFrame;
			set {
				if ( value < 1 ) {
					throw new ArgumentOutOfRangeException( nameof( value ) );
				}
				_maxTicksPerFrame = value;
			}
		}
		private int _maxTicksPerFrame = 0;

		private readonly Stopwatch _watch = new Stopwatch();

		private long _lastTimestamp = 0;
		private double _accumulatorSeconds = 0.0f;

		public NetworkGameClock()
			: this( DEFAULT_TICK_RATE, DEFAULT_MAX_TICKS_PER_FRAME )
		{
		}

		public NetworkGameClock( int tickRate, int maxTicksPerFrame = DEFAULT_MAX_TICKS_PER_FRAME )
		{
			if ( tickRate < 1 ) {
				throw new ArgumentOutOfRangeException( nameof( tickRate ) );
			}
			if ( maxTicksPerFrame < 1 ) {
				throw new ArgumentOutOfRangeException( nameof( maxTicksPerFrame ) );
			}

			_tickRate = tickRate;
			_tickDeltaSeconds = 1.0f / tickRate;
			_maxTicksPerFrame = maxTicksPerFrame;
		}

		public void Start()
		{
			if ( _isRunning ) {
				return;
			}

			_watch.Start();
			_lastTimestamp = Stopwatch.GetTimestamp();
			_isRunning = true;
		}

		public void Stop()
		{
			if ( !_isRunning ) {
				return;
			}

			_watch.Stop();
			_isRunning = false;
		}

		public void Reset()
		{
			_watch.Reset();

			_tick = 0;
			_pendingTicks = 0;
			_accumulatorSeconds = 0.0f;
			_lastTimestamp = Stopwatch.GetTimestamp();
			_isRunning = false;
		}

		public void Restart()
		{
			Reset();
			Start();
		}

		public int Update()
		{
			if ( !_isRunning ) {
				return 0;
			}

			long now = Stopwatch.GetTimestamp();
			long elapsedTicks = now - _lastTimestamp;
			_lastTimestamp = now;

			double elapsedSeconds = elapsedTicks / (double)Stopwatch.Frequency;
			if ( elapsedSeconds <= 0.0f ) {
				return 0;
			}

			_accumulatorSeconds += elapsedSeconds;

			int ticksDue = 0;
			while ( _accumulatorSeconds >= _tickDeltaSeconds && ticksDue < _maxTicksPerFrame ) {
				_accumulatorSeconds -= _tickDeltaSeconds;
				ticksDue++;
			}

			//
			// Spiral-of-death guard.
			//
			// If the game stalls badly, do not allow the network clock to spend
			// many frames trying to catch up forever. Drop excess accumulated time.
			//
			if ( ticksDue == _maxTicksPerFrame && _accumulatorSeconds >= _tickDeltaSeconds ) {
				_accumulatorSeconds = 0.0f;
			}

			_pendingTicks += ticksDue;
			return ticksDue;
		}
	};
};
