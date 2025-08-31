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

/*
using Godot;

namespace Interactables {
	public partial class RespawningHealthPack : InteractionItem {
		private Timer RespawnTimer;

		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null && player.GetHealth() < 60.0f ) {
				player.SetHealth( player.GetHealth() + 40.0f );
				player.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/StimPack_Activate1.ogg" ) );

				CallDeferred( MethodName.Hide );
				SetDeferred( PropertyName.Monitoring, false );
				InteractArea.SetDeferred( CollisionShape2D.PropertyName.Disabled, true );

				RespawnTimer.Start();
			}
		}
		private void OnRespawn() {
			Show();
			Monitoring = true;
			InteractArea.Disabled = false;
		}

		public override void _Ready() {
			base._Ready();

			RespawnTimer = GetNode<Timer>( "Timer" );
			RespawnTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnRespawn ) );

			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		}
	};
};
*/