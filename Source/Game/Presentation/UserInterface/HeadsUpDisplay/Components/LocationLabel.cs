using Game.Application.Configuration.Enums;
using Godot;

namespace Game.Presentation.UserInterface.HeadsUpDisplay {
	public sealed class LocationLabel : HUDComponent {
		public override bool Visible => _visible;
		private bool _visible = true;

		public override Color Modulate => throw new System.NotImplementedException();

		public override HUDPreset Visibility => throw new System.NotImplementedException();

		public override float FadeTime => throw new System.NotImplementedException();

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void Dispose() {
		}
	};
};