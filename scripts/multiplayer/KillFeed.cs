using Godot;
using System;

public partial class KillFeed : Control {
	private VBoxContainer Feed;
	private HBoxContainer Cloner;

	public override void _Ready() {
		Feed = GetNode<VBoxContainer>( "MarginContainer/VBoxContainer" );
		Cloner = GetNode<HBoxContainer>( "MarginContainer/HBoxContainer" );
	}

	public void Push( Player source, Texture2D weaponIcon, Player target ) {
		HBoxContainer data = (HBoxContainer)Cloner.Duplicate();

		data.Show();
		( (Label)data.GetChild( 0 ) ).Text = target.MultiplayerUsername;
		if ( weaponIcon != null ) {
			( (TextureRect)data.GetChild( 1 ) ).Texture = weaponIcon;
		}
		if ( source == null ) {
			( (Label)data.GetChild( 2 ) ).Hide();
		} else {
			( (Label)data.GetChild( 2 ) ).Text = source.MultiplayerUsername;
		}
		( (Timer)data.GetChild( 3 ) ).Connect( "timeout", Callable.From<HBoxContainer>( ( data ) => { Feed.RemoveChild( data ); data.QueueFree(); } ) );
	}
};
