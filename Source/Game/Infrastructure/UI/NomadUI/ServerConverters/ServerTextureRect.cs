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
using System;

namespace Game.Infrastructure.UI.NomadUI.ServerConverters {
	/*
	===================================================================================
	
	ServerTextureRect
	
	===================================================================================
	*/
	/// <summary>
	/// A RenderingServer variant of Godot's <see cref="TextureRect"/>
	/// </summary>
	
	internal readonly struct ServerTextureRect : IDisposable {
		private readonly Rid _canvasRid;
		private readonly Rid _textureRid;
		private readonly Rect2 _textureRect;

		/*
		===============
		ServerTextureRect
		===============
		*/
		public ServerTextureRect( Control owner, TextureRect textureRect ) {
			_canvasRid = RenderingServer.CanvasItemCreate();
			RenderingServer.CanvasItemSetParent( _canvasRid, owner.GetCanvasItem() );
			RenderingServer.CanvasItemSetDefaultTextureFilter( _canvasRid, (RenderingServer.CanvasItemTextureFilter)textureRect.TextureFilter  );
			RenderingServer.CanvasItemSetDefaultTextureRepeat( _canvasRid, (RenderingServer.CanvasItemTextureRepeat)textureRect.TextureRepeat  );
			RenderingServer.CanvasItemSetTransform( _canvasRid, textureRect.GetTransform() );
			RenderingServer.CanvasItemSetLightMask( _canvasRid, textureRect.LightMask );
			RenderingServer.CanvasItemSetModulate( _canvasRid, textureRect.Modulate );
			RenderingServer.CanvasItemSetUseParentMaterial( _canvasRid, textureRect.UseParentMaterial );
			RenderingServer.CanvasItemSetVisibilityLayer( _canvasRid, textureRect.VisibilityLayer );
			RenderingServer.CanvasItemSetZIndex( _canvasRid, textureRect.ZIndex );
			RenderingServer.CanvasItemSetZAsRelativeToParent( _canvasRid, textureRect.ZAsRelative );
			RenderingServer.CanvasItemSetVisible( _canvasRid, textureRect.Visible );
			RenderingServer.CanvasItemSetDrawBehindParent( _canvasRid, textureRect.ShowBehindParent );
			if ( textureRect.Material != null ) {
				RenderingServer.CanvasItemSetMaterial( _canvasRid, textureRect.Material.GetRid() );
			}
			_textureRid = textureRect.Texture.GetRid();
			_textureRect = textureRect.GetRect();

			RenderingServer.CanvasItemClear( _canvasRid );
			RenderingServer.CanvasItemAddTextureRect( _canvasRid, _textureRect, _textureRid );

			owner.RemoveChild( textureRect );
			textureRect.QueueFree();
		}

		/*
		===============
		Dispose
		===============
		*/
		public void Dispose() {
			if ( _canvasRid.IsValid ) {
				RenderingServer.FreeRid( _canvasRid );
			}
		}
	};
};