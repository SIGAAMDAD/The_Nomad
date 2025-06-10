public class FreeFlow {
	private static int ComboCounter = 0;
	private static int MaxCombo = 0;

	public static void IncreaseCombo( int nAmount = 1 ) {
		ComboCounter += nAmount;
		if ( ComboCounter > MaxCombo ) {
			MaxCombo = ComboCounter;
		}
	}
	public static void EndCombo() {
		ComboCounter = 0;
	}

	public static int CurrentCombo() => ComboCounter;
	public static int HighestCombo() => MaxCombo;
};