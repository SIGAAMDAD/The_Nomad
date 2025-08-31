using Godot;
using Renown.Thinkers;

namespace Renown {
	public partial class Relationship : Resource {
		public enum Type {
			Acquaintance,
			Friend,
			
			Sibling,
			Parent,
			Guardian,
			
			Lover,
			Engaged,
			Spouse,
			
			Dislikes,
			Enemy,
			RapBeef
		};
		
		public static int REQUIRED_SCORE_DISLIKES = 50;
		public static int REQUIRED_SCORE_ENEMY = 35;
		public static int REQUIRED_SCORE_RAPBEEFs = 10;
		
		private Thinker Thinker1;
		private Thinker Thinker2;
		private Type Status;
		private int Score = 50;
//		[Export]
//		private EventHistory History;
		
		public Relationship( Thinker thinker1, Thinker thinker2, Type type ) {
			Thinker1 = thinker1;
			Thinker2 = thinker2;
			Status = type;
			Score = 50;
		}
		public Relationship() {
		}

		private void EvaluateRelationStatus() {
			if ( Score > 50 ) {
				Status = Type.Acquaintance;
			} else if ( Score <= 50 ) {
				Status = Type.Dislikes;
				if ( Score < 35 ) {
					Status = Type.Enemy;
				}
				if ( Score < 10 ) {
					Status = Type.RapBeef;
				}
			}
		}
		
		public Thinker GetPerson1() {
			return Thinker1;
		}
		public Thinker GetPerson2() {
			return Thinker2;
		}
		public void DecreaseScore( int nAmount ) {
			Score -= nAmount;
			EvaluateRelationStatus();
		}
		public void IncreaseScore( int nAmount ) {
			Score += nAmount;
			EvaluateRelationStatus();
		}
		public Type GetStatus() {
			return Status;
		}
		public int GetScore() {
			return Score;
		}
//		public EventHistory GetHistory() {
//			return History;
//		}
	};
};