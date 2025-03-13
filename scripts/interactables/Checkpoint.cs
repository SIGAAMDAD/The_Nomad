using Godot;

public partial class Checkpoint : InteractionItem {
	[Export]
	private string Title;
	
	private AudioStreamPlayer2D PassedCheckpoint;
	private AudioStreamPlayer2D Ambience;

	private PointLight2D Light;
	private AnimatedSprite2D Bonfire;
	private Sprite2D Unlit;

	private Texture2D Icon;

	private bool Activated = false;

	public void Save() {
		/*
		DirAccess.MakeDirRecursiveAbsolute( ArchiveSystem.Instance.GetSaveDirectory() + "Checkpoints/" );

		string path = ProjectSettings.GlobalizePath( ArchiveSystem.Instance.GetSaveDirectory() + "Checkpoints/Checkpoint_" + Title + ".ngd" );
		System.IO.FileStream stream;
		try {
			stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
		} catch ( System.IO.FileNotFoundException exception ) {
			GD.PushError( exception.Source + ":" + exception.Message + "\n" + exception.StackTrace );
			return;
		}
		System.IO.BinaryWriter writer = new System.IO.BinaryWriter( stream );
		*/

		Player.SaveWriter.Write( Activated );
	}
	public void Load() {
		/*
		string path = ProjectSettings.GlobalizePath( ArchiveSystem.Instance.GetSaveDirectory() + "Checkpoints/Checkpoint_" + Title + ".ngd" );
		System.IO.FileStream stream;
		try {
			stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
		} catch ( System.IO.FileNotFoundException exception ) {
			GD.PushError( exception.Source + ":" + exception.Message + "\n" + exception.StackTrace );
			return;
		}
		System.IO.BinaryReader reader = new System.IO.BinaryReader( stream );
		*/

		Activated = Player.SaveReader.ReadBoolean();
	}

	public string GetTitle() {
		return Title;
	}

	public override void _ExitTree() {
		base._ExitTree();

		if ( Bonfire != null ) {
			Bonfire.QueueFree();
		}
		if ( Unlit != null ) {
			Unlit.QueueFree();
		}
		if ( Light != null ) {
			Light.QueueFree();
		}
		if ( Ambience != null ) {
			Ambience.QueueFree();
		}
	}

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		Player player = (Player)body;
		player.BeginInteraction( this );
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		Player player = (Player)body;
		player.EndInteraction();
	}

	private void OnPassedCheckpointFinished() {
		PassedCheckpoint.QueueFree();
	}

	public override InteractionType GetInteractionType() {
		return InteractionType.Checkpoint;
	}

    public override void _Ready() {
		base._Ready();
		
		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

		PassedCheckpoint = GetNode<AudioStreamPlayer2D>( "PassCheckpoint" );
		PassedCheckpoint.Connect( "finished", Callable.From( OnPassedCheckpointFinished ) );

		Ambience = GetNode<AudioStreamPlayer2D>( "Ambience" );

		Light = GetNode<PointLight2D>( "PointLight2D" );

		Bonfire = GetNode<AnimatedSprite2D>( "Bonfire" );
		Unlit = GetNode<Sprite2D>( "Unlit" );
	}
};
