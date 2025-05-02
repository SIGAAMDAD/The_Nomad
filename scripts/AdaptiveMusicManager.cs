using Godot;

public partial class AdaptiveMusicManager : Node {
	public partial class MusicStem : Node {
		[Export]
		private AudioStream FadeInStream;
		[Export]
		private AudioStream FadeOutStream;
		[Export]
		private AudioStream LoopStream;

		private AudioStreamPlayer StreamPlayer;
		private Tween AudioTweener;

		public override void _Ready() {
			base._Ready();

			StreamPlayer = new AudioStreamPlayer();
			StreamPlayer.VolumeDb = -20.0f;
			AddChild( StreamPlayer );
		}

		private void OnFadeInFinished() {
			StreamPlayer.Stream = LoopStream;
			StreamPlayer.Finished -= OnFadeInFinished;
			StreamPlayer.Play();
		}
		private void OnFadeOutFinished() {
			StreamPlayer.Finished -= OnFadeOutFinished;
			StreamPlayer.Stop();
		}

		public void FadeIn() {
			StreamPlayer.Stream = FadeInStream;
			StreamPlayer.Finished += OnFadeInFinished;
			StreamPlayer.Play();
		}
		public void FadeOut() {
			StreamPlayer.Stream = FadeInStream;
			StreamPlayer.Finished += OnFadeOutFinished;
			StreamPlayer.Play();
		}
	};

	[Export]
	private AudioStream CombatMusicStream;
	[Export]
	private AudioStream CalmMusicStream;
	[Export]
	private AudioStream AmbienceMusicStream;
	
	private AudioStreamPlayer CombatPlayer;
	private AudioStreamPlayer CalmPlayer;

	private AudioStream CurrentAudio;
	private Tween FadeOutTween;
	private Tween FadeInTween;

	private void OnTweenFinished( AudioStreamPlayer audioPlayer ) {
		audioPlayer.Stop();
	}

	private void TransitionTo( AudioStreamPlayer fromStream, AudioStreamPlayer toStream ) {
		FadeOutTween = CreateTween();
		FadeOutTween.Finished += () => { OnTweenFinished( fromStream ); };
		FadeOutTween.TweenProperty( fromStream, "volume_db", -20.0f, 2.5f );

		toStream.Play();
		FadeInTween = CreateTween();
		FadeInTween.TweenProperty( toStream, "volume_db", SettingsData.GetMusicVolumeLinear(), 2.5f );

		CurrentAudio = toStream.Stream;
	}

	public override void _Ready() {
		base._Ready();

		CalmPlayer = new AudioStreamPlayer();
		CalmPlayer.Name = "CalmPlayer";
		CalmPlayer.Stream = CalmMusicStream;
		CalmPlayer.VolumeDb = SettingsData.GetMusicVolumeLinear();
		CalmPlayer.Set( "parameters/looping", true );
		AddChild( CalmPlayer );

		CombatPlayer = new AudioStreamPlayer();
		CombatPlayer.Name = "CombatPlayer";
		CombatPlayer.Stream = CombatMusicStream;
		CombatPlayer.VolumeDb = SettingsData.GetMusicVolumeLinear();
		CombatPlayer.Set( "parameters/looping", true );
		AddChild( CombatPlayer );

		CalmPlayer.Play();
	}

	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 60 ) != 0 ) {
			return;
		}

		if ( Player.InCombat && CurrentAudio != CombatMusicStream ) {
			TransitionTo( CalmPlayer, CombatPlayer );
		} else if ( !Player.InCombat && CurrentAudio != CalmMusicStream ) {
			TransitionTo( CombatPlayer, CalmPlayer );
		}
	}
};