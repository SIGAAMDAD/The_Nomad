using Godot;
using System;

namespace PlayerSystem.Perks {
	public abstract partial class Perk : Resource {
		[Export]
		protected Texture2D? Icon = null;
		[Export]
		public StringName? Name { get; protected set; } = null;
		[Export]
		public StringName? Description { get; protected set; } = null;
		
		protected Player? User;

		public Perk( Player? user ) {
			ArgumentNullException.ThrowIfNull( user );
			ArgumentNullException.ThrowIfNull( Icon );
			ArgumentException.ThrowIfNullOrEmpty( Name );
			ArgumentException.ThrowIfNullOrEmpty( Description );

			User = user;
		}

		public abstract void Connect();
		public abstract void Disconnect();
	};
};