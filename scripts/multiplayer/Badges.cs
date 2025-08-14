namespace Multiplayer {
	public enum Badge : uint {
		Dishonered, // left a duel early
		CannonFodder, // most deaths in a single KotH game
		ObjectiveFocused, // get the highest objective score in a single objective focused game
		Sweaty, // get the highest kill count in any mode that isn't Bloodbath

		Count
	};
};