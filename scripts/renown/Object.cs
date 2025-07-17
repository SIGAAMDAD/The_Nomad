using System.Collections.Generic;
using Godot;
using Renown.World;

namespace Renown {
	public interface Object {
		public void Save();
		public void Load();

		public NodePath GetHash();

		public StringName GetObjectName();

		public void DetermineRelationStatus( Object other );
		public float GetRelationScore( Object other );
		public RelationStatus GetRelationStatus( Object other );
	};
};