using Godot;
using System;
using System.Collections.Generic;

public partial class KillFeed : Control {
	private VBoxContainer Feed;
	private HBoxContainer Cloner;

	private Queue<HBoxContainer> KillQueue;

	public override void _Ready() {
		Feed = GetNode<VBoxContainer>( "MarginContainer/VBoxContainer" );
		Cloner = GetNode<HBoxContainer>( "MarginContainer/HBoxContainer" );

		KillQueue = new Queue<HBoxContainer>( 4 );
	}

	public void Push( Player source, Texture2D weaponIcon, Player target ) {
		HBoxContainer data = (HBoxContainer)Cloner.Duplicate();

		data.Show();
		( (Label)data.GetChild( 0 ) ).Text = target.MultiplayerData.Username;
		if ( weaponIcon != null ) {
			( (TextureRect)data.GetChild( 1 ) ).Texture = weaponIcon;
		}
		if ( source == null ) {
			( (Label)data.GetChild( 2 ) ).Hide();
		} else {
			( (Label)data.GetChild( 2 ) ).Text = source.MultiplayerData.Username;
		}
		( (Timer)data.GetChild( 3 ) ).Connect( "timeout", Callable.From<HBoxContainer>( ( data ) => { Feed.RemoveChild( data ); data.QueueFree(); } ) );
	}
};
