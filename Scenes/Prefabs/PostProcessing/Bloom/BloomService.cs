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

using Game.Application.Configuration.Enums;
using NomadCore.Abstractions.Services;

namespace Game.Prefabs {
	/*
	===================================================================================
	
	BloomService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class BloomService {
		public BloomService( ICVarSystemService cvarSystem ) {
			var resolution = cvarSystem.GetCVar<WindowResolution>( "display.WindowResolution" ).Value;
		}

		/*
		private readonly RenderingDevice _device;
		private readonly Rid _computePipeline;
		private readonly byte[] _framebufferData;

		public BloomService( Vector2I currentResolution ) {
			_device = RenderingServer.CreateLocalRenderingDevice();

			var shaderFile = ResourceLoader.Load<RDShaderFile>( "res://Scenes/Prefabs/Bloom.glsl" );
			var spirvData = shaderFile.GetSpirV();
			var shader = _device.ShaderCreateFromSpirV( spirvData );

			_framebufferData = new byte[ currentResolution.X * currentResolution.Y / 2 ];
			var storageBuffer = _device.StorageBufferCreate( (uint)_framebufferData.Length, _framebufferData );

			var texture = new RDUniform {
				UniformType = RenderingDevice.UniformType.Image,
				Binding = 0
			};
			texture.AddId( storageBuffer );

			_computePipeline = _device.ComputePipelineCreate( shader );
			var computeList = _device.ComputeListBegin();
			_device.ComputeListBindComputePipeline( computeList, _computePipeline );
		}

		public void Update() {
		}

		private void ComputeBloom() {
		}
		*/
	};
};