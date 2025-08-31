using Godot;

public partial class DynamicResolutionScaling : SubViewportContainer {
	private int TargetFrameRate = 0;
	private DRSPreset Preset = DRSPreset.Balanced;

	//	private int UltraPerformance_MinFrames;
	//	private int Performance_MinFrames;
	//	private int Balanced_MinFrames;

	private int MinFrames;
	private System.Action ProcessCallback;

	private int[] PrevFrameData = new int[ 30 ];

	private void OnSettingsChanged() {
		int MaxFps = SettingsData.GetMaxFps();
		int RefreshRate = (int)DisplayServer.ScreenGetRefreshRate();

		int UltraPerformance_MinFrames = 0;
		int Performance_MinFrames = 0;
		int Balanced_MinFrames = 0;

		// if we have it set to "quality", we won't really care about a minimum frame rate
		if ( MaxFps > 0 ) {
			UltraPerformance_MinFrames = MaxFps - 10;
			Performance_MinFrames = MaxFps / 2;
			Balanced_MinFrames = MaxFps / 4;
		} else {
			UltraPerformance_MinFrames = RefreshRate - 10;
			Performance_MinFrames = RefreshRate / 2;
			Balanced_MinFrames = RefreshRate / 4;
		}

		TargetFrameRate = SettingsData.GetDRSTargetFrames();
		Preset = SettingsData.GetDRSPreset();

		switch ( Preset ) {
		case DRSPreset.UltraPerformance:
			MinFrames = UltraPerformance_MinFrames;
			break;
		case DRSPreset.Performance:
			MinFrames = Performance_MinFrames;
			break;
		case DRSPreset.Balanced:
			MinFrames = Balanced_MinFrames;
			break;
		case DRSPreset.Quality:
		case DRSPreset.UltraQuality:
			MinFrames = 0;
			break;
		};
	}

	public override void _Ready() {
		base._Ready();

		SettingsData.Instance.SettingsChanged += OnSettingsChanged;

		TargetFrameRate = SettingsData.GetDRSTargetFrames();
		Preset = SettingsData.GetDRSPreset();
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		PrevFrameData[ Engine.GetFramesDrawn() % PrevFrameData.Length ] = (int)Engine.GetFramesPerSecond();

		// average it out so that we aren't constantly going from high-low
		int average = 0;
		for ( int i = 0; i < PrevFrameData.Length; i++ ) {
			average += PrevFrameData[ i ];
		}
		average /= PrevFrameData.Length;

		if ( Preset == DRSPreset.Dynamic ) {
			if ( average < MinFrames ) {

			} else if ( average > TargetFrameRate ) {
				int amount = TargetFrameRate - average;

				if ( amount > TargetFrameRate / 2 ) {
					// we're hitting twice the target frame rate, we can upscale
				}
			}
		} else {

		}
	}
};