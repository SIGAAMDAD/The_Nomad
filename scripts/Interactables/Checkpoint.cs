using Godot;
using Renown.World;

public partial class Checkpoint : InteractionItem {
	[Export]
	private string Title;
	[Export]
	private WorldArea Location;

	private AnimatedSprite2D Animations;
	private AnimatedSprite2D ActivateAnimation;
	private AudioStreamPlayer2D AudioChannel;

	private Callable Callback;
	private RichTextLabel Text;

	private bool Activated = false;

	public WorldArea GetLocation() => Location;

	public bool GetActivated() => Activated;
	public void Activate( Player player ) {
		Activated = true;
		Animations.Hide();
		ActivateAnimation.Show();
		ActivateAnimation.Play( "default" );

		player.BeginInteraction( this );

		player.Disconnect( Player.SignalName.Interaction, Callback );
	}
	public string GetTitle() => Title;

	private void OnScreenEnter() => ProcessMode = ProcessModeEnum.Pausable;
	private void OnScreenExit() => ProcessMode = ProcessModeEnum.Disabled;

	public void Save() {
		using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() ) ) {
			writer.SaveBool( nameof( Activated ), Activated );
		}
	}
	public void Load() {
		SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( GetPath() );

		// save file compatibility
		if ( reader == null ) {
			return;
		}

		Activated = reader.LoadBoolean( nameof( Activated ) );

		if ( Activated ) {
			Animations.Play( "activated" );
		}
	}
	
	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Callback = Callable.From( () => Activate( player ) );
			Text.Show();
			player.Connect( Player.SignalName.Interaction, Callback );
			player.EmitSignal( Player.SignalName.ShowInteraction, this );
		}
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Text.Hide();
			if ( player.IsConnected( Player.SignalName.Interaction, Callback ) ) {
				player.Disconnect( Player.SignalName.Interaction, Callback );
			}
		}
	}
	public override InteractionType GetInteractionType() {
		return InteractionType.Checkpoint;
	}

    public override void _Ready() {
		base._Ready();

		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
		
		Animations = GetNode<AnimatedSprite2D>( "AnimatedSprite2D" );

		ActivateAnimation = GetNode<AnimatedSprite2D>( "ActivateAnimation" );
		ActivateAnimation.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( () => {
			ActivateAnimation.Hide();
			RemoveChild( ActivateAnimation );
			ActivateAnimation.QueueFree();
			Animations.Play( "activated" );
			Animations.Show();
		} ) );

		Text = GetNode<RichTextLabel>( "RichTextLabel" );
		LevelData.Instance.ThisPlayer.InputMappingContextChanged += () => Text.ParseBbcode( AccessibilityManager.GetBindString( LevelData.Instance.ThisPlayer.InteractAction ) );

		AddToGroup( "Archive" );

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		}
	}
};
