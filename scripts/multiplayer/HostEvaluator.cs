using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;
using Steamworks;

public static class HostEvaluator {
	// Configuration
	private const float CPU_WEIGHT = 0.35f;
	private const float ALLOC_WEIGHT = 0.25f;
	private const float NETWORK_WEIGHT = 0.30f;
	private const float BANDWIDTH_WEIGHT = 0.10f;
	private const int TEST_TIMEOUT_MS = 5000;

	private static bool TestsCompleted;
	private static float FinalScore;
	private static CSteamID[] LobbyMembers;

	public static async Task<float> EvaluateHostScore( CSteamID[] lobbyMembers ) {
		TestsCompleted = false;
		FinalScore = 0f;
		LobbyMembers = lobbyMembers;

		var testTask = RunAllTests();
		var timeoutTask = Task.Delay( TEST_TIMEOUT_MS );

		// Run tests with timeout
		if ( await Task.WhenAny( testTask, timeoutTask ) == timeoutTask ) {
			Console.PrintError( "Host evaluation timed out! Using fallback scores" );
			return CalculateFallbackScore();
		}

		return FinalScore;
	}

	private static async Task RunAllTests() {
		try {
			// Run tests in parallel
			var cpuTest = Task.Run( MeasureCpuPerformance );
			var allocTest = Task.Run( MeasureAllocationPerformance );

			await Task.WhenAll( cpuTest, allocTest );

			// Calculate composite score
			FinalScore = ( cpuTest.Result * CPU_WEIGHT ) +
						  ( allocTest.Result * ALLOC_WEIGHT );
		} catch ( Exception e ) {
			Console.PrintError( $"Host evaluation failed: {e.Message}" );
			FinalScore = CalculateFallbackScore();
		}
		finally {
			TestsCompleted = true;
		}
	}

	#region CPU Performance Test
	private static float MeasureCpuPerformance() {
		try {
			const int TEST_DURATION_MS = 1000;
			int primeCount = 0;
			long number = 2;
			Stopwatch sw = Stopwatch.StartNew();

			while ( sw.ElapsedMilliseconds < TEST_DURATION_MS ) {
				if ( IsPrime( number ) ) {
					primeCount++;
				}
				number++;
			}

			// Normalize score: 5000 primes/second = 100%
			return Mathf.Clamp( primeCount / 5000f, 0, 1 );
		} catch {
			return 0.5f;
		}
	}

	private static bool IsPrime( long n ) {
		if ( n <= 1 ) {
			return false;
		}
		if ( n == 2 ) {
			return true;
		}
		if ( n % 2 == 0 ) {
			return false;
		}

		var boundary = (long)Math.Sqrt( n );
		for ( long i = 3; i <= boundary; i += 2 ) {
			if ( n % i == 0 ) {
				return false;
			}
		}
		return true;
	}
	#endregion

	#region Memory Performance Test
	private static float MeasureAllocationPerformance() {
		try {
			// small allocations (16B)
			float smallScore = RunAllocationTest( 1000, 16, 10000 );
			// medium allocations (1KB)
			float mediumScore = RunAllocationTest( 500, 1024, 5000 );
			// Large allocations (1MB)
			float largeScore = RunAllocationTest( 100, 1048576, 1000 );
			// GC Impact
			float gcScore = MeasureGcImpact();

			return ( smallScore * 0.3f ) +
				   ( mediumScore * 0.4f ) +
				   ( largeScore * 0.2f ) +
				   ( gcScore * 0.1f );
		} catch {
			return 0.5f;
		}
	}

	private static float RunAllocationTest( int iterations, int size, int count ) {
		List<long> times = new List<long>();
		List<byte[]> dummyHolder = new List<byte[]>();
		Random rng = new Random();

		for ( int i = 0; i < iterations; i++ ) {
			Stopwatch sw = Stopwatch.StartNew();

			for ( int j = 0; j < count; j++ ) {
				// vary size slightly to prevent optimization
				int actualSize = size + rng.Next( 0, 16 );
				byte[] buffer = new byte[ actualSize ];
				buffer[ 0 ] = (byte)( j % 256 );
				if ( j % 10 == 0 ) dummyHolder.Add( buffer );
			}

			sw.Stop();
			times.Add( sw.ElapsedTicks );
			dummyHolder.Clear();
		}

		times.Sort();
		long medianTicks = times[ times.Count / 2 ];
		double medianMs = ( medianTicks / (double)Stopwatch.Frequency ) * 1000;

		// normalize: 1ms per 10k allocations = 100%
		return (float)( Math.Clamp( 1000.0 / ( medianMs * 10 ), 0, 100 ) / 100.0 );
	}

	private static float MeasureGcImpact() {
		try {
			int gc0 = GC.CollectionCount( 0 );
			int gc1 = GC.CollectionCount( 1 );
			int gc2 = GC.CollectionCount( 2 );

			// Create GC pressure
			List<byte[]> garbage = new List<byte[]>();
			for ( int i = 0; i < 100000; i++ ) {
				garbage.Add( new byte[ 1024 ] );
			}

			Stopwatch sw = Stopwatch.StartNew();
			garbage.Clear();
			GC.Collect( 2, GCCollectionMode.Forced, true, true );
			sw.Stop();

			// Calculate penalty score
			int gen0Collections = GC.CollectionCount( 0 ) - gc0;
			float gcPauseScore = Mathf.Clamp( 1.0f - ( sw.ElapsedMilliseconds / 100f ), 0, 1 );
			float gcFreqScore = 1.0f / ( 1 + gen0Collections );

			return ( gcPauseScore * 0.7f ) + ( gcFreqScore * 0.3f );
		} catch {
			return 0.7f;
		}
	}
	#endregion

	#region Fallback & Utilities
	private static float CalculateFallbackScore() {
		// Simple fallback based on system specs
		int coreCount = System.Environment.ProcessorCount;
		long memSize = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1000000000;
		return Mathf.Clamp( coreCount * memSize * 0.1f, 0.3f, 0.9f );
	}
	#endregion
};