using Godot;
using System.Collections;

public partial class ItemPickup : InteractionItem {
	[Export]
	public Resource Data;
	[Export]
	public int Amount = -1;

	protected Sprite2D Icon;

	private RichTextLabel Text;
	private Callable Callback;

	private void OnPickupItem( Player player ) {
		Godot.Collections.Array<Resource> Categories = (Godot.Collections.Array<Resource>)Data.Get( "categories" );

		bool done = false;
		for ( int i = 0; i < Categories.Count; i++ ) {
			string name = (string)Categories[ i ].Get( "name" );
			switch ( name ) {
			case "Weapon":
				WeaponEntity weapon = new WeaponEntity();
				weapon.Name = "Weapon" + weapon;
				weapon.Data = Data;
				player.AddChild( weapon );
				weapon.TriggerPickup( player );
				done = true;
				break;
			case "Ammo":
				AmmoEntity ammo = new AmmoEntity();
				ammo.Name = "Ammo" + ammo;
				ammo.Data = Data;
				player.AddChild( ammo );
				player.PickupAmmo( ammo, Amount );
				done = true;
				break;
			};
		}

		if ( done ) {
			Icon?.QueueFree();
			Icon = null;

			SetDeferred( PropertyName.Monitorable, false );
			GetChild<CollisionShape2D>( 0 ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );
		}

		Text.Hide();
		player.Disconnect( Player.SignalName.Interaction, Callback );
	}

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		// TODO: auto-pickup toggle?
		if ( body is Player player && player != null ) {
			Callback = Callable.From( () => OnPickupItem( player ) );
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

	private void Load() {
		using ( var reader = ArchiveSystem.GetSection( GetPath() ) ) {
			if ( reader == null ) {
				return;
			}
			if ( !reader.LoadBoolean( "Used" ) ) {
				CreateSprite();
			} else {
				RemoveFromGroup( "Archive" );
				QueueFree();
			}
		}
	}
	public void Save() {
		using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() ) ) {
			writer.SaveBool( "Used", Icon == null );
		}
	}

	protected void CreateSprite() {
		Icon = new Sprite2D();
		Icon.Name = "Icon";
		Icon.Texture = (Texture2D)Data.Get( "icon" );
		Icon.ZIndex = 8;
		AddChild( Icon );
	}

	public override void _Ready() {
		base._Ready();

		Text = GetNode<RichTextLabel>( "RichTextLabel" );
		LevelData.Instance.ThisPlayer.InputMappingContextChanged += () => Text.ParseBbcode( AccessibilityManager.GetBindString( LevelData.Instance.ThisPlayer.InteractAction ) );

		AddToGroup( "Archive" );

		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		} else {
			CreateSprite();
		}
	}
};