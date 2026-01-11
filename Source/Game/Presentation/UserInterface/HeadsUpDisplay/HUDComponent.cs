using System;
using Game.Application.Configuration.Enums;
using Godot;

namespace Game.Presentation.UserInterface.HeadsUpDisplay {
	public abstract class HUDComponent : IDisposable {
		public abstract bool Visible { get; }
		public abstract Color Modulate { get; }
		public abstract HUDPreset Visibility { get; }
		public abstract float FadeTime { get; }

		public abstract void Dispose();
	};
};