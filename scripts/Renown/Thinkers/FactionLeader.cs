using Godot;
using System.Collections.Generic;
using Renown;
using Renown.World;

namespace Renown.Thinkers {
	public partial class FactionLeader : Thinker {
		[Export]
		protected float Money = 0.0f;
		[Export]
		protected int WarCrimeCount = 0;
		[Export]
		protected Godot.Collections.Array<TraitType> Traits = new Godot.Collections.Array<TraitType>();
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Relations = null;
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Debts = null;

		protected HashSet<Trait> TraitCache = null;
		protected HashSet<RenownValue> RelationCache = null;
		protected HashSet<RenownValue> DebtCache = null;

		public override void Save() {
			using var writer = new SaveSystem.SaveSectionWriter( Name );
			int count;

			writer.SaveInt( "RelationCount", RelationCache.Count );
			count = 0;
			foreach ( var relation in RelationCache ) {
				if ( relation.Object is Entity entity && entity != null ) {
					writer.SaveBool( string.Format( "RelationIsEntity{0}", count ), true );
				} else {
					writer.SaveBool( string.Format( "RelationIsEntity{0}", count ), false );
				}
				writer.SaveString( string.Format( "RelationNode{0}", count ), relation.Object.GetObjectName() );
				writer.SaveFloat( string.Format( "RelationValue{0}", count ), relation.Value );
				count++;
			}

			writer.SaveInt( "DebtCount", DebtCache.Count );
			count = 0;
			foreach ( var debt in DebtCache ) {
				writer.SaveString( string.Format( "DebtNode{0}", count ), debt.Object.GetObjectName() );
				writer.SaveFloat( string.Format( "DebtValue{0}", count ), debt.Value );
				count++;
			}

			writer.SaveInt( "TraitCount", TraitCache.Count );
			count = 0;
			foreach ( var trait in TraitCache ) {
				writer.SaveUInt( string.Format( "TraitType{0}", count ), (uint)trait.GetTraitType() );
				count++;
			}
		}
		public override void Load() {
			using var reader = ArchiveSystem.GetSection( Name );

			// save file compatibility
			if ( reader == null ) {
				return;
			}

			int relationCount = reader.LoadInt( "RelationCount" );
			RelationCache = new HashSet<RenownValue>( relationCount );
			for ( int i = 0; i < relationCount; i++ ) {
				RelationCache.Add( new RenownValue(
					(Object)GetTree().Root.GetNode( reader.LoadString( string.Format( "RelationNode{0}", i ) ) ),
					reader.LoadFloat( string.Format( "RelationValue{0}", i ) )
				) );
			}

			int debtCount = reader.LoadInt( "DebtCount" );
			DebtCache = new HashSet<RenownValue>( debtCount );
			for ( int i = 0; i < debtCount; i++ ) {
				DebtCache.Add( new RenownValue(
					(Object)GetTree().Root.GetNode( reader.LoadString( string.Format( "DebtNode{0}", i ) ) ),
					reader.LoadFloat( string.Format( "DebtValue{0}", i ) )
				) );
			}

			int traitCount = reader.LoadInt( "TraitCount" );
			TraitCache = new HashSet<Trait>( traitCount );
			for ( int i = 0; i < traitCount; i++ ) {
				TraitCache.Add( Trait.Create( (TraitType)reader.LoadUInt( string.Format( "TraitType{0}", i ) ) ) );
			}
		}

		public override void DetermineRelationStatus( Object other ) {
			if ( !RelationCache.TryGetValue( new RenownValue( other ), out RenownValue value ) ) {
				return;
			}
			float score = value.Value;
			int renownScore = other.GetRenownScore();

			// TODO: write some way of using renown to determine if the entity knows all this stuff about the other one

			if ( Faction.GetRelationStatus( other ) >= RelationStatus.Hates ) {
				score -= Faction.GetRelationScore( other );
			}

			/*
			HashSet<Trait> traitList = other.GetTraits();
			foreach ( var trait in traitList ) {
				List<Trait> conflicting = GetConflictingTraits( trait );
				for ( int i = 0; i < conflicting.Count; i++ ) {
					score -= conflicting[i].GetNegativeRelationScore( trait );
				}

				List<Trait> agreeables = GetAgreeableTraits( trait );
				for ( int i = 0; i < agreeables.Count; i++ ) {
					score += conflicting[i].GetPositiveRelationScore( trait );
				}
			}
			*/

			value.Value = score;
		}
		public override bool HasRelation( Object other ) => RelationCache.Contains( new RenownValue( other ) );
		public override float GetRelationScore( Object other ) => RelationCache.TryGetValue( new RenownValue( other ), out RenownValue score ) ? score.Value : 0.0f;
		public override RelationStatus GetRelationStatus( Object other ) {
			float score = GetRelationScore( other );

			if ( score < -100.0f ) {
				return RelationStatus.KendrickAndDrake;
			}
			if ( score < -50.0f ) {
				return RelationStatus.Hates;
			}
			if ( score < 0.0f ) {
				return RelationStatus.Dislikes;
			}
			if ( score > 25.0f ) {
				return RelationStatus.Friends;
			}
			if ( score > 100.0f ) {
				return RelationStatus.GoodFriends;
			}
			return RelationStatus.Neutral;
		}

		public override void _Ready() {
			base._Ready();

			TraitCache = new HashSet<Trait>( Traits.Count );
			for ( int i = 0; i < Traits.Count; i++ ) {
				TraitCache.Add( Trait.Create( Traits[ i ] ) );
			}
			Traits.Clear();

			if ( Relations != null ) {
				RelationCache = new HashSet<RenownValue>( Relations.Count );
				foreach ( var relation in Relations ) {
					if ( relation.Key is Faction faction && faction != null ) {
						RelationCache.Add( new RenownValue( faction, relation.Value ) );
					} else if ( relation.Key is Entity entity && entity != null ) {
						RelationCache.Add( new RenownValue( entity, relation.Value ) );
					} else {
						Console.PrintError( string.Format( "Entity._Ready: relation key {0} isn't a renown object!", relation.Key != null ? relation.Key.GetPath() : "nil" ) );
					}
				}
				Relations.Clear();
			} else {
				RelationCache = new HashSet<RenownValue>();
			}

			if ( Debts != null ) {
				DebtCache = new HashSet<RenownValue>( Debts.Count );
				foreach ( var debt in Debts ) {
					if ( debt.Key is Faction faction && faction != null ) {
						DebtCache.Add( new RenownValue( faction, debt.Value ) );
					} else if ( debt.Key is Entity entity && entity != null ) {
						DebtCache.Add( new RenownValue( entity, debt.Value ) );
					} else {
						Console.PrintError( string.Format( "Entity._Ready: debt key {0} isn't a renown object!", debt.Key != null ? debt.Key.GetPath() : "nil" ) );
					}
				}
				Debts.Clear();
			} else {
				DebtCache = new HashSet<RenownValue>();
			}
		}
	};
};