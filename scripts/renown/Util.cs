namespace Renown {
	public static class Util {
		public static int CalcAdjustWithPercentage( int value, int percentage ) => percentage * value / 100;
		public static float CalcAdjustWithPercentage( float value, float percentage ) => percentage * value / 100.0f;
		public static int CalcPercentage( int value, int total ) => total > 0 ? ( 100 * value ) / total : 0;
		public static float CalcPercentage( float value, float total ) => total > 0.0f ? ( 100.0f * value ) / total : 0.0f;
	};
};