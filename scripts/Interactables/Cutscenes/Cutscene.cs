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

using System.Collections.Generic;
using Godot;

namespace Interactables.Cutscenes {
	/*
	===================================================================================

	Cutscene

	===================================================================================
	*/

	public partial class Cutscene : Node2D {
		[Export]
		private CutsceneSequence[] Sequences;
		[Export]
		private CutsceneTrigger Trigger;

		private Queue<CutsceneSequence> Actions = new Queue<CutsceneSequence>();
		private CutsceneSequence Sequence;

		[Signal]
		public delegate void FinishedEventHandler();

		/*
		===============
		Start
		===============
		*/
		public void Start() {
			Actions.TryDequeue( out Sequence );
			Sequence.Start();
		}

		/*
		===============
		OnSequenceEnded
		===============
		*/
		private void OnSequenceEnded() {
			if ( Actions.TryDequeue( out Sequence ) ) {
				Sequence.Start();
			}
			EmitSignalFinished();
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			for ( int i = 0; i < Sequences.Length; i++ ) {
				Actions.Enqueue( Sequences[ i ] );
				Sequences[ i ].End += OnSequenceEnded;
			}
		}
	};
};