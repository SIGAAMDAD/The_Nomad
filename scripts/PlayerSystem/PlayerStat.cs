namespace PlayerSystem {
	public struct PlayerStat<T> {
		public readonly T BaseValue;
		public readonly T MaxValue;
		public readonly T MinValue;
		public T Value;

		public PlayerStat( T baseValue, T min, T max ) {
			BaseValue = baseValue;
			MaxValue = max;
			MinValue = min;

			Value = BaseValue;
		}
	};
};