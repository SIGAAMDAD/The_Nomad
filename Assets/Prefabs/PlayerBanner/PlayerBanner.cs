using Godot;
using Nomad.Core.OnlineServices;
using System;
using System.Threading.Tasks;

public partial class PlayerBanner : HBoxContainer {
	private readonly IUserAvatarService _avatarService;
	private readonly PeerId _peerId;
	private readonly string _displayName;

	private TextureRect _profilePicture;

	public PlayerBanner( IUserAvatarService avatarService, PeerId peerId, string displayName ) {
		_avatarService = avatarService ?? throw new ArgumentNullException( nameof( avatarService ) );
		_displayName = displayName ?? string.Empty;
		_peerId = peerId;
	}
	
	private async Task GetAvatarImage( PeerId peerId ) {
		var avatarImage = await _avatarService.QueryAvatarAsync( peerId, AvatarSize.Large );
		
		_profilePicture.SetDeferred(
			TextureRect.PropertyName.Texture,
			ImageTexture.CreateFromImage( avatarImage.Image as Image )
		);
	}

	public override void _Ready() {
		base._Ready();

		AddThemeConstantOverride( "separation", 24 );

		_profilePicture = new TextureRect() {
			CustomMinimumSize = new Vector2( 64.0f, 64.0f ),
			ExpandMode = TextureRect.ExpandModeEnum.FitWidth
		};
		AddChild( _profilePicture );
		GetAvatarImage( _peerId );

		AddChild( new Label {
			Text = _displayName
		} );
	}
};
