using System.Collections.Generic;
using Godot;

namespace Renown.Thinkers {
	public partial class FamilyTree : Node {
		[Export]
		private Godot.Collections.Array<Thinker> Members = null;
		[Export]
		private int MaxStrength = 0;
		[Export]
		private int MaxDexterity = 0;
		[Export]
		private int MaxIntelligence = 0;
		[Export]
		private int MaxWisdom = 0;
		[Export]
		private int MaxConsitution = 0;
		[Export]
		private int StrengthBonus = 0;
		[Export]
		private int DexterityBonus = 0;
		[Export]
		private int IntelligenceBonus = 0;
		[Export]
		private int WisdomBonus = 0;
		[Export]
		private int ConsitutionBonus = 0;

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

		private void OnMemberDeath( Entity attacker, Entity member ) {
		}

		public void AddMember( Thinker thinker ) {
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