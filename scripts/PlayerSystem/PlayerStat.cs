namespace PlayerSystem {
	public struct PlayerStat<T> {
		public T BaseValue;
		public T MaxValue;
		public T MinValue;
		public T Value;

		public PlayerStat( T baseValue, T min, T max ) {
			BaseValue = baseValue;
			MaxValue = max;
			MinValue = min;
			
			Value = BaseValue;
		}
	};
};