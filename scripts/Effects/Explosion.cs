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
using Renown;
using Menus;

/*
===================================================================================

Explosion

EXPLOOOOOOOOSION!

===================================================================================
*/

public partial class Explosion : Node2D {
	public float Radius = 80.0f;
	public float Damage = 30.0f;
	public Curve DamageCurve = null;
	public ExtraAmmoEffects Effects = 0;

	private Area2D BlowupArea;

	/*
	===============
	OnFinished
	===============
	*/
	private void OnFinished() {
		GetParent().CallDeferred( MethodName.RemoveChild, this );
		CallDeferred( MethodName.QueueFree );
	}

	/*
	===============
	CalcDamage
	===============
	*/
	private void CalcDamage() {
		Godot.Collections.Array<Node2D> entities = BlowupArea.GetOverlappingBodies();
		int length = (int)Scale.Length();
		int i;

		for ( i = 0; i < length; i++ ) {
			DebrisFactory.Create( GlobalPosition );
		}

		for ( i = 0; i < entities.Count; i++ ) {
			if ( entities[ i ] is Entity entity && entity != null ) {
				float damage = Damage * ( DamageCurve != null ? DamageCurve.SampleBaked( entity.GlobalPosition.DistanceTo( GlobalPosition ) ) : 1.0f );
				entity.Damage( GetParent<Entity>(), damage );
				if ( ( Effects & ExtraAmmoEffects.Incendiary ) != 0 ) {
					entity.AddStatusEffect( "status_burning" );
				}
			}
		}
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

		ZIndex = 8;

		AudioStreamPlayer2D AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioStreamPlayer2D" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AudioChannel.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( OnFinished ) );

		BlowupArea = GetNode<Area2D>( "Area2D" );
		( BlowupArea.GetChild<CollisionShape2D>( 0 ).Shape as CircleShape2D ).Radius = Radius;

		CallDeferred( MethodName.CalcDamage );

		float distance = GlobalPosition.DistanceTo( GetViewport().GetCamera2D().GlobalPosition );
		if ( distance < 128.0f ) {
		}
	}
};