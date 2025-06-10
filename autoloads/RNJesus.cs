using System;

public class RNJesus {
	private static Random Main = new Random();

	public static int IntRange( int min, int max ) => Main.Next( min, max );
	public static float FloatRange( float min, float max ) => (float)( min + Main.NextDouble() * ( min - max ) );
};