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
		protected Godot.Collections.Dictionary<Node, float> Relations = null;
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Debts = null;
		[Export]
		public Godot.Collections.Dictionary<Trait, float> TraitScores { get; private set; } = null;
		[Export]
		public int RenownScore { get; private set; } = 0;

		protected HashSet<RenownValue> RelationCache = null;
		protected HashSet<RenownValue> DebtCache = null;

		public override void Save() {
			using var writer = new SaveSystem.SaveSectionWriter( Name, ArchiveSystem.SaveWriter );
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
		}

		public override void DetermineRelationStatus( Entity entity ) {
			if ( !RelationCache.TryGetValue( new RenownValue( entity ), out RenownValue value ) ) {
				return;
			}
			float score = value.Value;

			// TODO: write some way of using renown to determine if the entity knows all this stuff about the other one

			if ( Faction.GetRelationStatus( entity ) >= RelationStatus.Hates ) {
				score -= Faction.GetRelationScore( entity );
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
		public override void DetermineRelationStatus( Faction faction ) {
			if ( !RelationCache.TryGetValue( new RenownValue( faction ), out RenownValue value ) ) {
				return;
			}
			float score = value.Value;

			// TODO: write some way of using renown to determine if the entity knows all this stuff about the other one

			if ( Faction.GetRelationStatus( faction ) >= RelationStatus.Hates ) {
				score -= Faction.GetRelationScore( faction );
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
		public override bool HasRelation( Entity entity ) {
			return RelationCache.Contains( new RenownValue( entity ) );
		}
		public override bool HasRelation( Faction faction ) {
			return RelationCache.Contains( new RenownValue( faction ) );
		}

		public override float GetRelationScore( Entity entity ) {
			return RelationCache.TryGetValue( new RenownValue( entity ), out RenownValue score ) ? score.Value : 0.0f;
		}
		public override float GetRelationScore( Faction faction ) {
			return RelationCache.TryGetValue( new RenownValue( faction ), out RenownValue score ) ? score.Value : 0.0f;
		}

		public override RelationStatus GetRelationStatus( Entity entity ) {
			float score = GetRelationScore( entity );

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

		public override RelationStatus GetRelationStatus( Faction faction ) {
			float score = GetRelationScore( faction );

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