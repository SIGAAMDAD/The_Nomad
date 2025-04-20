using System;
using Godot;

namespace Renown.Thinkers {
	public partial class FamilyTree : Node {
		[Export]
		private Godot.Collections.Array<Thinker> Members = null;
		[Export]
		private int MaxStrength = new Random().Next( 12, 18 );
		[Export]
		private int MaxDexterity = new Random().Next( 12, 18 );
		[Export]
		private int MaxIntelligence = new Random().Next( 12, 18 );
		[Export]
		private int MaxWisdom = new Random().Next( 12, 18 );
		[Export]
		private int MaxConsitution = new Random().Next( 12, 18 );
		[Export]
		private int StrengthBonus = new Random().Next( 0, 4 );
		[Export]
		private int DexterityBonus = new Random().Next( 0, 4 );
		[Export]
		private int IntelligenceBonus = new Random().Next( 0, 4 );
		[Export]
		private int WisdomBonus = new Random().Next( 0, 4 );
		[Export]
		private int ConsitutionBonus = new Random().Next( 0, 4 );

		public int GetStrengthBonus() => StrengthBonus;
		public int GetDexterityBonus() => DexterityBonus;
		public int GetIntelligenceBonus() => IntelligenceBonus;
		public int GetWisdomBonus() => WisdomBonus;
		public int GetConstitutionBonus() => ConsitutionBonus;

		public int GetStrengthMax() => MaxStrength;
		public int GetDexterityMax() => MaxDexterity;
		public int GetIntelligenceMax() => MaxIntelligence;
		public int GetWisdomMax() => MaxWisdom;
		public int GetConstitutionMax() => MaxConsitution;

		public bool HasMemberName( StringName name ) {
			for ( int i = 0; i < Members.Count; i++ ) {
				if ( Members[i].GetFirstName() == name ) {
					return true;
				}
			}
			return false;
		}

		private void OnMemberDeath( Entity attacker, Entity member ) {
		}

		public void AddMember( Thinker thinker ) {
			if ( Members.Contains( thinker ) ) {
				return;
			}
			Members.Add( thinker );
			thinker.Die += OnMemberDeath;
		}

		public static FamilyTree CreateTree() {
			FamilyTree tree = new FamilyTree();
			( (Node)Engine.GetMainLoop().Get( "root" ) ).GetNode<Node2D>( "World/FamilyTrees" ).AddChild( tree );
			return tree;
		}
	};
};