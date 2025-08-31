/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using Renown;
using Menus;
using System.Runtime.CompilerServices;

/*
===================================================================================

StatusEffect

===================================================================================
*/
/// <summary>
/// The base class from which all status effects must inherit from
/// </summary>

public partial class StatusEffect : Node2D {
	[Export]
	protected float Duration = 0.0f;

	public Timer EffectTimer { get; protected set; }
	public Entity Victim { get; protected set; }
	public Texture2D Icon { get; protected set; }

	protected AudioStreamPlayer2D AudioChannel;

	[Signal]
	public delegate void TimeoutEventHandler();

	/*
	===============
	SetVictim
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public virtual void SetVictim( Entity owner ) {
		Victim = owner;
	}

	/*
	===============
	ResetTimer
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public virtual void ResetTimer() {
		EffectTimer.Start();
	}

	/*
	===============
	Stop
	===============
	*/
	public virtual void Stop() {
		SetProcess( false );
		QueueFree();
	}

	/*
	===============
	OnEffectTimerTimeout
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	protected virtual void OnEffectTimerTimeout() {
		EmitSignalTimeout();
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

		AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioChannel" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		EffectTimer = GetNode<Timer>( "EffectTimer" );
		EffectTimer.Autostart = true;
		EffectTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnEffectTimerTimeout ) );
	}
};