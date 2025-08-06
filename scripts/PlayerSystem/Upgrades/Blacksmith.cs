using DialogueManagerRuntime;
using Godot;
using PlayerSystem.UserInterface;
using Renown;

namespace PlayerSystem.Upgrades {
	public partial class Blacksmith : Renown.Thinkers.Thinker {
		private enum DialogueOption : int {
			Upgrades,
			Repairs,
			Talk,
			Leave
		};

		[Export]
		private Resource[] Quests;

		private DialogueInteractor Interactor;

		private MarginContainer UpgradeContainer;

		private AudioStreamPlayer2D ForgeAmbience;
		private AnimatedSprite2D ForgeSparks;

		public void UpgradeWeapon( in WeaponEntity weapon ) {
		}

		private void OnDialogueOptionSelected( int nSelectedOption ) {
			switch ( (DialogueOption)nSelectedOption ) {
			case DialogueOption.Upgrades:
				DialogueManager.ShowDialogueBalloon( Interactor.DialogueResource, "upgrades" );
				break;
			case DialogueOption.Repairs:
				break;
			case DialogueOption.Talk:
				break;
			case DialogueOption.Leave:
				DialogueContainer.EndInteraction();
				DialogueManager.ShowDialogueBalloon( Interactor.DialogueResource, "exit" );
				break;
			}
			;
		}

		public override void _Ready() {
			base._Ready();

			Interactor = GetNode<DialogueInteractor>( "DialogueInteractor" );
			Interactor.Connect( DialogueInteractor.SignalName.BeginDialogue, Callable.From( () => BodyAnimations.Play( "interact" ) ) );
			Interactor.Connect( DialogueInteractor.SignalName.EndDialogue, Callable.From( () => BodyAnimations.Play( "idle" ) ) );
			Interactor.Connect( DialogueInteractor.SignalName.DialogueOptionSelected, Callable.From<int>( OnDialogueOptionSelected ) );

			UpgradeContainer = GetNode<MarginContainer>( "UIInteractor/BlacksmithUpgradeContainer" );

			ForgeSparks = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations/ForgeSparks" );
			ForgeSparks.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( ForgeSparks.Hide ) );

			ForgeAmbience = GetNode<AudioStreamPlayer2D>( "Animations/BodyAnimations/ForgeAmbience" );

			BodyAnimations.Connect( AnimatedSprite2D.SignalName.AnimationLooped, Callable.From( () => {
				ForgeAmbience.Play();
				ForgeSparks.Show();
				ForgeSparks.Play();
			} ) );

			VisibleOnScreenEnabler2D enabler = GetNode<VisibleOnScreenEnabler2D>( "VisibleOnScreenEnabler2D" );
			enabler.Connect( "screen_entered", Callable.From( Show ) );
			enabler.Connect( "screen_exited", Callable.From( Hide ) );

			Node CraftStation = GetNode( "CraftStation" );
			CraftStation.Set( "database", ResourceCache.ItemDatabase );

			Node InputInventory = GetNode( "InputInventory" );
			InputInventory.Set( "database", ResourceCache.ItemDatabase );

			Node OutputInventory = GetNode( "OutputInventory" );
			OutputInventory.Set( "database", ResourceCache.ItemDatabase );
		}
	};
};