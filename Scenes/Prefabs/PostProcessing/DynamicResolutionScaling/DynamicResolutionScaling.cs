/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;

namespace Game.Prefabs {
	/*
	===================================================================================
	
	DynamicResolutionScaling
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class DynamicResolutionScaling : SubViewport {
		private DynamicResolutionScalingService _service;
		private TextureRect _displayRect;

		public override void _Ready() {
			base._Ready();

			_service = new DynamicResolutionScalingService();

			Size = new Vector2I( _service.ScreenSize.width, _service.ScreenSize.height );
			RenderTargetUpdateMode = UpdateMode.Always;
			Msaa2D = Msaa.Msaa4X;

			_displayRect = GetNode<TextureRect>( "DisplayRect" );
			_displayRect.Texture = GetTexture();
			_displayRect.Scale = new Vector2( _service.ScaleFactor.X, _service.ScaleFactor.Y );
		}

		public override void _Process( double delta ) {
			base._Process( delta );

			_service.Update( Engine.GetFramesPerSecond() );
			_displayRect.Scale = new Vector2( _service.ScaleFactor.X, _service.ScaleFactor.Y );
		}
	};
};