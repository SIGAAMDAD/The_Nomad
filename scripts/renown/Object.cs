using System.Collections.Generic;
using Godot;
using Renown.World;

namespace Renown {
	public interface Object {
		public void Save();
		public void Load();

		public NodePath GetHash();

		public StringName GetObjectName();

		public float GetMoney();
		public void DecreaseMoney( float nAmount );
		public void IncreaseMoney( float nAmount );

		public HashSet<Trait> GetTraits();
		public bool HasTrait( Trait trait );
		public bool HasConflictingTrait( Trait other );
		public List<Trait> GetConflictingTraits( Trait other );
		public List<Trait> GetAgreeableTraits( Trait other );
		public void AddTrait( Trait trait );
		public void RemoveTrait( Trait trait );

		public void DetermineRelationStatus( Object other );
		public void Meet( Object other );
		public void RelationIncrease( Object other, float nAmount );
		public void RelationDecrease( Object other, float nAmount );
		public float GetRelationScore( Object other );
		public RelationStatus GetRelationStatus( Object other );

		public int GetRenownScore();
		public void AddRenown( int nAmount );
	};
};