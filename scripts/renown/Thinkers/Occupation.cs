using System.Collections.Generic;
using Godot;
using Renown.World;
using Renown.World.Buildings;

namespace Renown.Thinkers {
	public enum OccupationType : uint {
		None,

		Industry,

		Bandit,
		Mercenary,
		Count,
		MercenaryMaster,
		Doctor,

		Merchant,
		
		Blacksmith,
		Gunsmith,
			
		// DNA splicer
		Splicer,
		
		Politician,
	};
	public partial class Occupation : Node {
		protected OccupationType Type;
		protected float Wage;
		protected Building WorkPlace;
		protected Faction Company;
		protected Thinker Worker;

		protected static readonly Dictionary<OccupationType, float> DefaultWages = new Dictionary<OccupationType, float>{
			// per-commission
			{ OccupationType.Blacksmith, 0.0f },
			{ OccupationType.Bandit, 0.0f },
			{ OccupationType.Splicer, 0.0f },
			{ OccupationType.Merchant, 0.0f },
			
			// per-hour
			{ OccupationType.Industry, 10.0f },
			{ OccupationType.Doctor, 2000.0f },
			{ OccupationType.Mercenary, 1600.0f },
			{ OccupationType.Politician, 30000.0f },
		};

		[Signal]
		public delegate void RequestedRaiseEventHandler( float nAmount );

		public Occupation( Thinker worker, Faction company ) {
			Worker = worker;
			Company = company;
		}

		public Building GetWorkPlace() => WorkPlace;
		public void SetWorkPlace( Building building ) => WorkPlace = building;

		public float GetWage() => Wage;
		public OccupationType GetOccupationType() => Type;

		public Faction GetCompany() => Company;
		public void SetCompany( Faction faction ) => Company = faction;

		public void RequestRaise( float nAmount ) {
			EmitSignalRequestedRaise( nAmount );
		}

		public virtual void Damage( Entity source, float nAmount ) {
		}
		public virtual void Load( SaveSystem.SaveSectionReader reader, string key ) {
			Type = (OccupationType)reader.LoadUInt( key + "JobType" );
			Wage = reader.LoadFloat( key + "JobWage" );
			CallDeferred( "SetCompany", GetTree().Root.GetNode( reader.LoadString( key + "JobCompany" ) ) );
		}
		public virtual void Save( SaveSystem.SaveSectionWriter writer, string key ) {
			writer.SaveUInt( key + "JobType", (uint)Type );
			writer.SaveFloat( key + "JobWage", Wage );
			writer.SaveString( key + "JobCompany", Company.GetPath() );
		}

		public override void _Ready() {
			base._Ready();

			ProcessMode = ProcessModeEnum.Disabled;

			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				Wage = DefaultWages[ Type ];
			}
		}

		public virtual void OnPlayerEnteredArea() {
		}
		public virtual void OnPlayerExitedArea() {
		}
		public virtual void Process() {
		}
		public virtual void ProcessAnimations() {
		}
		public virtual void Notify( GroupEvent nEventType, Thinker source ) {
		}
		public virtual void SetNavigationTarget( Godot.Vector2 target ) {
		}
		public virtual void OnTargetReached() {
		}

		public static Occupation Create( OccupationType nType, Thinker worker, Faction faction ) {
			switch ( nType ) {
			case OccupationType.MercenaryMaster:
				return new Thinker.MercenaryMaster( worker, faction );
			case OccupationType.Industry:
				return new Thinker.Industry( worker, faction );
			case OccupationType.Bandit:
				return new Thinker.Bandit( worker, faction );
			case OccupationType.Mercenary:
				return new Thinker.Mercenary( worker, faction );
			case OccupationType.None:
				return new Thinker.None( worker, faction );
			default:
				Console.PrintError( string.Format( "Invalid OccupationType: {0}", nType ) );
				break;
			};
			return null;
		}
	};
};