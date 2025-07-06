using Godot;
using System.Collections.Generic;

namespace Renown.Thinkers.Formations {
	public partial class Formation : Node2D {
		protected Node2D Leader;
		protected List<Node2D> Positions;
		protected List<int> Members;

		public override void _Ready() {
			base._Ready();

			Leader = GetNode<Node2D>( "Leader" );

			Godot.Collections.Array<Node> children = GetNode( "Positions" ).GetChildren();
			Positions = new List<Node2D>( children.Count );
			for ( int i = 0; i < children.Count; i++ ) {
				Positions.Add( children[ i ] as Node2D );
			}
		}

		public virtual void AssignLeader( Entity entity ) {
			CallDeferred( MethodName.Reparent, entity );
			Leader.SetMeta( "Entity", entity );
		}
		public virtual void AddMember( Entity entity ) {
			entity.GlobalPosition = Positions[ Members.Count ].GlobalPosition;
			entity.SetMeta( "ParentNode", entity.GetParent().GetPath() );
			Members.Add( entity.GetHashCode() );
			entity.CallDeferred( MethodName.Reparent, this );
		}
		public virtual void RemoveMember( Entity entity ) {
			for ( int i = 0; i < Members.Count; i++ ) {
				if ( Members[ i ] == entity.GetHashCode() ) {
					Members.RemoveAt( i );
					return;
				}
			}
			Console.PrintError( string.Format( "Formation.RemoveMember: invalid entity {0}", entity.GetHashCode() ) );
		}

		public virtual bool IsPartOfFormation( Entity entity ) {
			for ( int i = 0; i < Members.Count; i++ ) {
				if ( Members[ i ] == entity.GetHashCode() ) {
					return true;
				}
			}
			return false;
		}
	};
};