using Godot;
using Renown;
using Renown.Thinkers;

namespace PlayerSystem.Upgrades {
	public partial class Blacksmith : Entity {
		private DialogueInteractor Interactor;
		private AnimatedSprite2D Animations;

		private AudioStreamPlayer2D ForgeAmbience;
		private AnimatedSprite2D ForgeSparks;

		public 

		public override void _Ready() {
			base._Ready();

			Interactor = GetNode<DialogueInteractor>( "DialogueInteractor" );
			Interactor.Connect( DialogueInteractor.SignalName.BeginDialogue, Callable.From( () => Animations.Play( "interact" ) ) );
			Interactor.Connect( DialogueInteractor.SignalName.EndDialogue, Callable.From( () => Animations.Play( "idle" ) ) );

			ForgeSparks = GetNode<AnimatedSprite2D>( "Animations/ForgeSparks" );
			ForgeSparks.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( ForgeSparks.Hide ) );

			ForgeAmbience = GetNode<AudioStreamPlayer2D>( "Animations/ForgeAmbience" );

			Animations = GetNode<AnimatedSprite2D>( "Animations" );
			Animations.Connect( AnimatedSprite2D.SignalName.AnimationLooped, Callable.From( () => {
				ForgeAmbience.Play();
				ForgeSparks.Show();
				ForgeSparks.Play();
			} ) );

			VisibleOnScreenEnabler2D enabler = GetNode<VisibleOnScreenEnabler2D>( "VisibleOnScreenEnabler2D" );
			enabler.Connect( VisibleOnScreenEnabler2D.SignalName.ScreenEntered, Callable.From( () => Animations.ProcessMode = ProcessModeEnum.Pausable ) );
			enabler.Connect( VisibleOnScreenEnabler2D.SignalName.ScreenExited, Callable.From( () => Animations.ProcessMode = ProcessModeEnum.Disabled ) );

			Node CraftStation = GetNode( "CraftStation" );
			CraftStation.Set( "database", ResourceCache.ItemDatabase );

			Node InputInventory = GetNode( "InputInventory" );
			InputInventory.Set( "database", ResourceCache.ItemDatabase );

			Node OutputInventory = GetNode( "OutputInventory" );
			OutputInventory.Set( "database", ResourceCache.ItemDatabase );
		}
	};
};