namespace Multiplayer {
	public enum Badge : uint {
		/// <summary>
		/// left a duel early
		/// </summary>
		Dishonered,

		/// <summary>
		/// most deaths in a single KotH game
		/// </summary>
		CannonFodder,

		/// <summary>
		/// get the highest objective score in a single objective focused game
		/// </summary>
		ObjectiveFocused,

		/// <summary>
		/// get the highest kill count in any mode that isn't Bloodbath
		/// </summary>
		Sweaty,

		Count
	};
};