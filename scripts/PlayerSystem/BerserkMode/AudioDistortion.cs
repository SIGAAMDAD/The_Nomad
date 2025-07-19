using Godot;
using System;

namespace PlayerSystem.BerserkMode {
	public partial class AudioDistortion : AudioEffectAmplify {
		private float BaseCutoff = 5000.0f;
		private float TranceCutoff = 1500.0f;

		public void UpdateAudio( float focusAmount ) {
			int busIndex = AudioServer.GetBusIndex( "Master" );
			AudioEffect effect = AudioServer.GetBusEffect( busIndex, 0 );

			effect.Set( "cutoff_hz", Mathf.Lerp( BaseCutoff, TranceCutoff, focusAmount ) );

			AudioServer.SetBusEffectEnabled( busIndex, 1, true );
			AudioServer.SetBusVolumeDb( busIndex, Mathf.Lerp( 0.0f, -10.0f, focusAmount ) );
		}
		
	};
};