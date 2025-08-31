/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using DialogueManagerRuntime;
using Godot;
using PlayerSystem.UserInterface;
using Interactables;

namespace PlayerSystem.Upgrades {
	/*
	===================================================================================
	
	Blacksmith
	
	handles player-to-blacksmith interactions
	
	===================================================================================
	*/
	
	public partial class Blacksmith : Renown.Thinkers.Thinker {
		private enum DialogueOption : int {
			Upgrades,
			Repairs,
			Talk,
			Leave
		};

		[Export]
		private Resource[]? Quests;

		private DialogueInteractor? Interactor;

		private MarginContainer? UpgradeContainer;

		private AudioStreamPlayer2D? ForgeAmbience;
		private AnimatedSprite2D? ForgeSparks;

		/*
		===============
		UpgradeWeapon
		===============
		*/
		public void UpgradeWeapon( in WeaponEntity weapon ) {
		}

		/*
		===============
		OnDialogueOptionSelected
		===============
		*/
		private void OnDialogueOptionSelected( int selectedOption ) {
			switch ( (DialogueOption)selectedOption ) {
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
		}

		/*
		===============
		OnBodyAnimationsAnimationLooped
		===============
		*/
		private void OnBodyAnimationsAnimationLooped() {
			ForgeAmbience.Play();
			ForgeSparks.Show();
			ForgeSparks.Play();
		}

		/*
		===============
		_Ready

		godot initialization override
		===============
		*/
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

			BodyAnimations.Connect( AnimatedSprite2D.SignalName.AnimationLooped, Callable.From( OnBodyAnimationsAnimationLooped ) );
		}
	};
};