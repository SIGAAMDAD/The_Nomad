using Godot;

public partial class Checkpoint : InteractionItem {
	[Export]
	private StringName Title;

	private PointLight2D Light;
	private AnimatedSprite2D Bonfire;
	private Sprite2D Unlit;

	private Texture2D Icon;

	private bool Activated = false;

	public bool GetActivated() => Activated;
	public void Activate() {
		Activated = true;

		Bonfire.Show();
		Light.Show();

		Unlit.Hide();
		Unlit.QueueFree();

		Bonfire.Play( "default" );

		Tween BrightnessTween = CreateTween();
		BrightnessTween.TweenProperty( Light, "energy", 1.75f, 2.5f );

		AudioPlayer.PlaySound( this, ResourceCache.GetSound( "res://sounds/env/bonfire_create.ogg" ), false, 1.0f, new System.Action( () => {
			AudioPlayer.PlaySound( this, ResourceCache.GetSound( "res://sounds/env/campfire.ogg" ), true );
		} ) );
	}
	public StringName GetTitle() => Title;

	private void OnScreenEnter() {
		ProcessMode = ProcessModeEnum.Pausable;
		if ( Activated ) {
			AudioPlayer.PlaySound( this, ResourceCache.GetSound( "res://sounds/env/campfire.ogg" ), true );
		}
	}
	private void OnScreenExit() {
		ProcessMode = ProcessModeEnum.Disabled;
		AudioPlayer.StopSound( this );
	}

	public void Save() {
		SaveSystem.SaveSectionWriter writer = new SaveSystem.SaveSectionWriter( Name );
		writer.SaveBool( "activated", Activated );
		writer.Flush();
	}
	public void Load() {
		SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( Name  );

		// save file compatibility
		if ( reader == null ) {
			return;
		}

		Activated = reader.LoadBoolean( "activated" );

		if ( Activated ) {
			Light.Show();
			Bonfire.Show();
			Bonfire.Play( "default" );

			Light.Energy = 1.75f;
			
			Unlit.Hide();
			Unlit.QueueFree();
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
	public override InteractionType GetInteractionType() {
		return InteractionType.Checkpoint;
	}

    public override void _Ready() {
		base._Ready();
		
		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

		VisibleOnScreenNotifier2D notifier = GetNode<VisibleOnScreenNotifier2D>( "VisibleOnScreenNotifier2D" );
		notifier.Connect( "screen_entered", Callable.From( OnScreenEnter ) );
		notifier.Connect( "screen_exited", Callable.From( OnScreenExit ) );

		Light = GetNode<PointLight2D>( "PointLight2D" );
		Light.SetProcess( false );
		Light.SetProcessInternal( false );

		Bonfire = GetNode<AnimatedSprite2D>( "Bonfire" );
		Bonfire.SetProcess( false );
		Bonfire.SetProcessInternal( false );

		Unlit = GetNode<Sprite2D>( "Unlit" );
		Unlit.SetProcess( false );
		Unlit.SetProcessInternal( false );

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		}
	}
};
