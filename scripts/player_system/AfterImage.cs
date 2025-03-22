using Godot;

namespace PlayerSystem {
	public partial class AfterImage : Node2D {
		private AnimatedSprite2D Legs;
		private AnimatedSprite2D Torso;
		private AnimatedSprite2D LeftArm;
		private AnimatedSprite2D RightArm;

		public AfterImage() {
			LeftArm = new AnimatedSprite2D();
			CallDeferred( "add_child", LeftArm );

			RightArm = new AnimatedSprite2D();
			CallDeferred( "add_child", RightArm );

			Torso = new AnimatedSprite2D();
			CallDeferred( "add_child", Torso );

			Legs = new AnimatedSprite2D();
			CallDeferred( "add_child", Legs );
		}
		public void Update( Player player ) {
			GlobalPosition = player.GlobalPosition;
			LeftArm.SpriteFrames = player.GetLeftArmAnimation().SpriteFrames;
			RightArm.SpriteFrames = player.GetRightArmAnimation().SpriteFrames;
			Torso.SpriteFrames = player.GetTorsoAnimation().SpriteFrames;
			Legs.SpriteFrames = player.GetLegsAnimation().SpriteFrames;
		}
	};
};