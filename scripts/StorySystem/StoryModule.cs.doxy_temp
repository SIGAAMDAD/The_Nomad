using Godot;
using System;

namespace StorySystem {
	[AttributeUsage( AttributeTargets.Class )]
	public sealed class ModInfo : Attribute {
		public string ModuleId;
		public string Version;
		public string MinimumGameVersion;

		public ModInfo( string ModuleId, string Version, string MinimumGameVersion ) {
			this.ModuleId = ModuleId;
			this.Version = Version;
			this.MinimumGameVersion = MinimumGameVersion;
		}
	};

	public abstract partial class StoryModule : Resource {
		public virtual void OnRegistered() {
			RegisterQuests();
			RegisterBots();
			RegisterEndings();
		}

		protected abstract void RegisterQuests();
		protected abstract void RegisterBots();
		protected abstract void RegisterEndings();

		public virtual void HandleEvent( StringName eventName, object[] args ) {
		}
	};
};