using Godot;

namespace Nomad.Game.Prefabs
{
	internal sealed partial class ShadowOverlay : ColorRect
	{
		[Export]
		private DirectionalLight2D _sunLight;
		[Export]
		private SubViewport _heightViewport;
		[Export]
		private Camera2D _heightCamera;

		private ShaderMaterial _material;
		private Camera2D _mainCamera;

		public override void _Ready()
		{
			base._Ready();

			_material = Material as ShaderMaterial;

			_heightCamera.PositionSmoothingEnabled = false;
			_heightCamera.RotationSmoothingEnabled = false;
			_heightCamera.AnchorMode = Camera2D.AnchorModeEnum.DragCenter;
			_heightCamera.Offset = Vector2.Zero;

			_heightViewport.Size = (Vector2I)GetViewportRect().Size;
			_heightViewport.Disable3D = true;
			_heightViewport.TransparentBg = true;

			_material.SetShaderParameter( "height_texture", _heightViewport.GetTexture() );
		}

		public override void _Process( double delta )
		{
			base._Process( delta );

			_mainCamera ??= GetViewport().GetCamera2D();

			if ( _mainCamera != null ) {
				_heightCamera.GlobalPosition = _mainCamera.GetScreenCenterPosition();
				_heightCamera.GlobalRotation = _mainCamera.GetScreenRotation();
				_heightCamera.Zoom = _mainCamera.Zoom;
				_heightCamera.Offset = _mainCamera.Offset;
				_heightCamera.Rotation = _mainCamera.Rotation;

				var viewportSize = GetViewportRect().Size;
				var wantedSize = new Vector2I( (int)viewportSize.X, (int)viewportSize.Y );
				if ( _heightViewport.Size != wantedSize ) {
					_heightViewport.Size = wantedSize;
				}
			}

			Vector2 lightTravelDir = Vector2.Down.Rotated( _sunLight.GlobalRotation );
			Vector2 shadowDir = -lightTravelDir;

			_material.SetShaderParameter( "shadow_direction_screen", shadowDir );
			_material.SetShaderParameter( "sun_height", _sunLight.Height );

			_material.SetShaderParameter( "shadow_length_multiplier", 1.0f );
			_material.SetShaderParameter( "shadow_quality", 48 );
			_material.SetShaderParameter( "height_max_px", 96.0f );
		}
	};
};
