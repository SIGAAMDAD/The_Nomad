using Godot;

public partial class WeaponSfxCache : Node {
	public static System.Collections.Generic.List<AudioStream> BulletShellCasings;
	public static System.Collections.Generic.List<AudioStream> ShotgunShellCasings;
	public static System.Collections.Generic.List<AudioStream> Ricochet;

	public override void _Ready() {
		BulletShellCasings = new System.Collections.Generic.List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/env/bullet_shell_0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/bullet_shell_1.ogg" )
		};

		ShotgunShellCasings = new System.Collections.Generic.List<AudioStream>(){
			ResourceLoader.Load<AudioStream>( "res://sounds/env/shotgun_shell_0.ogg" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/shotgun_shell_1.ogg" )
		};

		Ricochet = new System.Collections.Generic.List<AudioStream>(){
			ResourceLoader.Load<AudioStream>( "res://sounds/env/RICOCHE2.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/RICOCHE3.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/RICOCHE5.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/RICOCHE6.wav" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/env/RICOCHE7.wav" )
		};
	}
};
