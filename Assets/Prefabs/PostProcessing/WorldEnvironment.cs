using Game.Infrastructure;
using Godot;
using Nomad.Core;
using Nomad.CVars;

namespace Game.Prefabs {
	/*
	===================================================================================
	
	WorldEnvironment
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class WorldEnvironment : Godot.WorldEnvironment {
		public override void _Ready() {
			base._Ready();

			var cvarSystem = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper").ServiceLocator.GetService<ICVarSystemService>();
			
			cvarSystem.GetCVar<float>( Constants.CVars.Display.BRIGHTNESS ).ValueChanged.Subscribe( this, OnBrightnessChanged );
		}

		/*
		===============
		OnBrightnessChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnBrightnessChanged( in CVarValueChangedEventArgs<float> args ) {
			Environment.AdjustmentBrightness = args.NewValue * 0.01f;
		}
	};
};