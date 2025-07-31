using Godot;
using System.Collections.Generic;

namespace Renown.Thinkers.Groups {
	public enum SquadTactic {
		None,
		CirclePlayer,
		FlankPlayer,
		Cover,
		Retreat
	};
	public partial class Squad {
		public List<Thinker> Members { get; private set; } = new List<Thinker>();
		public Vector2 LastKnownTargetPosition { get; private set; }
		public SquadTactic CurrentTactic { get; private set; } = SquadTactic.None;
		public Dictionary<Thinker, Vector2> AssignedPositions { get; private set; } = new Dictionary<Thinker, Vector2>();

		public void AddMember( Thinker member ) {
			if ( !Members.Contains( member ) ) {
				Members.Add( member );
			}
		}
		public void RemoveMember( Thinker member ) {
			Members.Remove( member );
			AssignedPositions.Remove( member );
		}

		public void UpdateFormation() {
			if ( Members.Count == 0 ) {
				return; // sanity check
			}

			switch ( CurrentTactic ) {
			case SquadTactic.CirclePlayer:
				AssignCirclePositions();
				break;
			case SquadTactic.FlankPlayer:
				break;
			case SquadTactic.Cover:
				break;
			}
			;
		}
		public void AssignCirclePositions() {
			const float radius = 150.0f;
			float angleStep = Mathf.Tau / Members.Count;

			for ( int i = 0; i < Members.Count; i++ ) {
				Vector2 offset = new Vector2(
					Mathf.Cos( angleStep * i ) * radius,
					Mathf.Sin( angleStep * i ) * radius
				);
				AssignedPositions[ Members[ i ] ] = LastKnownTargetPosition + offset;
			}
		}
	};
};