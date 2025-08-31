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

using System.Collections.Generic;
using Godot;
using Renown;
using ResourceCache;

namespace PlayerSystem {
	/*
	===================================================================================

	FootSteps

	I swear this isn't a foot thing.

	===================================================================================
	*/
	/// <summary>
	/// Handles footsteps, queueing them up, and releasing them
	/// </summary>

	public partial class FootSteps : Node {
		private static readonly int MAX_STEPS = 24;

		private MultiMeshInstance2D MeshManager;
		private Queue<Transform2D> Steps = new Queue<Transform2D>( MAX_STEPS );

		private bool Wet = false;
		private bool IsPlayer = false;

		/*
		===============
		CheckCapacity
		===============
		*/
		private void CheckCapacity() {
			if ( Steps.Count < MAX_STEPS ) {
				return;
			}

			for ( int i = 0; i < 3; i++ ) {
				Steps.Dequeue();

				int instance = 0;
				foreach ( var step in Steps ) {
					MeshManager.Multimesh.SetInstanceTransform2D( instance, step );
					instance++;
				}
			}

			MeshManager.Multimesh.VisibleInstanceCount -= 3;
		}

		/*
		===============
		AddStep
		===============
		*/
		public void AddStep( Vector2 velocity, Vector2 position, GroundMaterialType GroundType, AudioStreamPlayer2D customChannel = null ) {
			Transform2D transform = new Transform2D( 0.0f, new Vector2( position.X, position.Y + 24.0f ) );
			CheckCapacity();
			MeshManager.Multimesh.VisibleInstanceCount++;
			MeshManager.Multimesh.SetInstanceTransform2D( Steps.Count, transform );
			Steps.Enqueue( transform );

			AudioStream? stream = null;
			if ( IsPlayer ) {
				stream = GroundType switch {
					GroundMaterialType.Stone => SoundCache.GetEffectRange( SoundEffect.PlayerMoveStone0, 4 ),
					GroundMaterialType.Sand => SoundCache.GetEffectRange( SoundEffect.PlayerMoveSand0, 4 ),
					GroundMaterialType.Water => SoundCache.GetEffectRange( SoundEffect.PlayerMoveWater0, 2 ),
					GroundMaterialType.Wood => SoundCache.GetEffectRange( SoundEffect.PlayerMoveWood0, 4 ),
					_ => SoundCache.GetEffectRange( SoundEffect.PlayerMoveGravel0, 4 ),
				};
			} else {
			}
			GetParent<Entity>().PlaySound( customChannel, stream );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			MeshManager = new MultiMeshInstance2D();
			MeshManager.Multimesh = new MultiMesh();
			MeshManager.Texture = TextureCache.GetTexture( "res://textures/env/footstep.png" );
			MeshManager.Multimesh.Mesh = new QuadMesh();
			MeshManager.Multimesh.VisibleInstanceCount = 0;
			MeshManager.Multimesh.InstanceCount = MAX_STEPS;
			MeshManager.Modulate = new Color( 1.0f, 1.0f, 1.0f, 0.75f );
			( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 16.0f, -8.0f );
			AddChild( MeshManager );

			IsPlayer = GetParent() is Player;
		}
	};
};