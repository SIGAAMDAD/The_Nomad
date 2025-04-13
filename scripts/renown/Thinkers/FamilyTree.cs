using System.Collections.Generic;
using Godot;

namespace Renown.Thinkers {
	public partial class FamilyTree : Node {
		[Export]
		private Godot.Collections.Array<Thinker> Members = null;

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