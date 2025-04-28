using System;
using System.Collections.Generic;
using Godot;
using Renown.World;
using Renown.World.Buildings;

namespace Renown.Thinkers {
	public enum SocietyRank {
		Lower,
		Middle,
		Upper,

		Count
	};

	public partial class Family : Node {
		private class NameGenerator {
			private static HashSet<string> UsedLastNames = new HashSet<string>();
			private static Random random = new Random();

			private static readonly string[] LastNameScramble_Begin = [
				"Ono",
				"Cha",
				"Kal",
				"Rin",
				"Ossa",
				"Eoc",
				"Forl",
				"Anni",
				"Mi",
				"Rez"
			];
			private static readonly string[] LastNameScramble_Middle = [
				"il",
				"lo",
				"op",
				"ppa",
				"ser",
				"olla",
				"ava",
				"iki",
				"za",
				"oma",
				"oso",
				"ava",
				"assa",
				"isse"
			];
			private static readonly string[] LastNameScramble_End = [
				"kol",
				"go",
				"vil",
				"tro",
				"ini",
				"op",
				"si",
				"af",
				"of",
				"it",
				"in",
				"ik",
				"ir",
				"io",
				"at",
				"ap",
				"an",
				"ak",
			];

			private static string NameScramble( string[] begin, string[] middle, string[] end ) {
				return string.Format( "{0}{1}{2}", begin[ random.Next( 0, begin.Length - 1 ) ], middle[ random.Next( 0, middle.Length - 1 ) ], end[ random.Next( 0, end.Length - 1 ) ] );
			}
			public static string GenerateLastName() {
				string name = NameScramble( LastNameScramble_Begin, LastNameScramble_Middle, LastNameScramble_End );
				while ( UsedLastNames.Contains( name ) ) {
					name = NameScramble( LastNameScramble_Begin, LastNameScramble_Middle, LastNameScramble_End );
				}
				UsedLastNames.Add( name );
				return name;
			}
		};

		private int MaxMembers = 0;
		private int MemberCount = 0;
		private StringName FamilyName;

		[Export]
		private Godot.Collections.Array<Thinker> CachedMembers = null;

		private Thinker[] Members = null;

		[Export]
		private BuildingHouse Home;

		[Export]
		private SocietyRank SocioEconomicStatus = SocietyRank.Lower;

		[Export]
		public int MaxStrength {
			get;
			private set;
		}
		[Export]
		public int MaxDexterity {
			get;
			private set;
		}
		[Export]
		public int MaxIntelligence {
			get;
			private set;
		}
		[Export]
		public int MaxWisdom {
			get;
			private set;
		}
		[Export]
		public int MaxConstitution {
			get;
			private set;
		}
		[Export]
		public int MaxCharisma {
			get;
			private set;
		}

		[Export]
		public int StrengthBonus {
			get;
			private set;
		}
		[Export]
		public int DexterityBonus {
			get;
			private set;
		}
		[Export]
		public int IntelligenceBonus {
			get;
			private set;
		}
		[Export]
		public int WisdomBonus {
			get;
			private set;
		}
		[Export]
		public int ConstitutionBonus {
			get;
			private set;
		}
		[Export]
		public int CharismaBonus {
			get;
			private set;
		}

		[Export]
		private bool IsPremade = false;

		public Family() {
		}
		public Family( Random random, SocietyRank rank ) {
			MaxStrength = random.Next( 12, 18 );
			MaxDexterity = random.Next( 12, 18 );
			MaxIntelligence = random.Next( 12, 18 );
			MaxWisdom = random.Next( 12, 18 );
			MaxConstitution = random.Next( 12, 18 );
			MaxCharisma = random.Next( 12, 18 );

			StrengthBonus = random.Next( 0, 6 );
			DexterityBonus = random.Next( 0, 6 );
			IntelligenceBonus = random.Next( 0, 6 );
			WisdomBonus = random.Next( 0, 6 );
			ConstitutionBonus = random.Next( 0, 6 );
			CharismaBonus = random.Next( 0, 6 );

			SocioEconomicStatus = rank;
			switch ( SocioEconomicStatus ) {
			case SocietyRank.Lower:
				MaxMembers = random.Next( 18, 30 );
				break;
			case SocietyRank.Middle:
				MaxMembers = random.Next( 4, 12 );
				break;
			case SocietyRank.Upper:
				MaxMembers = random.Next( 1, 3 );
				break;
			};

			Members = new Thinker[ MaxMembers ];
		}

		public SocietyRank GetSocietyRank() => SocioEconomicStatus;
		public int GetMemberCount() => MemberCount;
		public int GetMaxMembers() => MaxMembers;
		public bool CanAddMember() => MemberCount < MaxMembers;

		public StringName GetFamilyName() => FamilyName;

		public void SetHome( BuildingHouse house ) => Home = house;
		public BuildingHouse GetHome() => Home;

		private void OnMemberDeath( Entity source, Entity target ) {
		}
		public bool AddMember( Thinker member ) {
			if ( MemberCount >= MaxMembers ) {
				return false;
			}
			Members[ MemberCount ] = member;
			MemberCount++;

			member.Die += OnMemberDeath;

			GD.Print( "Added member " + member + " to family (MemberCount: " + MemberCount + " )" );

			return true;
		}

		public void Load( SaveSystem.SaveSectionReader reader, int nIndex ) {
			string key = string.Format( "Family{0}", nIndex );

			if ( !IsPremade ) {
				FamilyName = reader.LoadString( key + nameof( FamilyName ) );
				SocioEconomicStatus = (SocietyRank)reader.LoadUInt( key + nameof( SocioEconomicStatus ) );
				Home = GetTree().Root.GetNode<BuildingHouse>( reader.LoadString( key + nameof( Home ) ) );
			}

			MaxStrength = reader.LoadInt( key + nameof( MaxStrength ) );
			MaxDexterity = reader.LoadInt( key + nameof( MaxDexterity ) );
			MaxIntelligence = reader.LoadInt( key + nameof( MaxIntelligence ) );
			MaxWisdom = reader.LoadInt( key + nameof( MaxWisdom ) );
			MaxConstitution = reader.LoadInt( key + nameof( MaxConstitution ) );
			MaxCharisma = reader.LoadInt( key + nameof( MaxCharisma ) );

			StrengthBonus = reader.LoadInt( key + nameof( StrengthBonus ) );
			DexterityBonus = reader.LoadInt( key + nameof( DexterityBonus ) );
			IntelligenceBonus = reader.LoadInt( key + nameof( IntelligenceBonus ) );
			WisdomBonus = reader.LoadInt( key + nameof( WisdomBonus ) );
			ConstitutionBonus = reader.LoadInt( key + nameof( ConstitutionBonus )  );
			CharismaBonus = reader.LoadInt( key + nameof( CharismaBonus ) );
		}
		public void Save( SaveSystem.SaveSectionWriter writer, int nIndex ) {
			string key = string.Format( "Family{0}", nIndex );

			writer.SaveString( key + nameof( FamilyName ), FamilyName );
			writer.SaveUInt( key + nameof( SocioEconomicStatus ), (uint)SocioEconomicStatus );
			writer.SaveString( key + nameof( Home ), Home.GetPath() );

			writer.SaveInt( key + nameof( MaxStrength ), MaxStrength );
			writer.SaveInt( key + nameof( MaxDexterity ), MaxDexterity );
			writer.SaveInt( key + nameof( MaxIntelligence ), MaxIntelligence );
			writer.SaveInt( key + nameof( MaxWisdom ), MaxWisdom );
			writer.SaveInt( key + nameof( MaxConstitution ), MaxConstitution );
			writer.SaveInt( key + nameof( MaxCharisma ), MaxCharisma );

			writer.SaveInt( key + nameof( StrengthBonus ), StrengthBonus );
			writer.SaveInt( key + nameof( DexterityBonus ), DexterityBonus );
			writer.SaveInt( key + nameof( IntelligenceBonus ), IntelligenceBonus );
			writer.SaveInt( key + nameof( WisdomBonus ), WisdomBonus );
			writer.SaveInt( key + nameof( ConstitutionBonus ), ConstitutionBonus );
			writer.SaveInt( key + nameof( CharismaBonus ), CharismaBonus );
		}

		public override void _Ready() {
			base._Ready();

			ProcessMode = ProcessModeEnum.Disabled;

			if ( !IsInGroup( "Families" ) ) {
				AddToGroup( "Families" );
			}

			if ( IsPremade ) {
				FamilyName = Name;

				Random random = new Random();
				MaxStrength = random.Next( 12, 18 );
				MaxDexterity = random.Next( 12, 18 );
				MaxIntelligence = random.Next( 12, 18 );
				MaxWisdom = random.Next( 12, 18 );
				MaxConstitution = random.Next( 12, 18 );
				MaxCharisma = random.Next( 12, 18 );

				StrengthBonus = random.Next( 0, 6 );
				DexterityBonus = random.Next( 0, 6 );
				IntelligenceBonus = random.Next( 0, 6 );
				WisdomBonus = random.Next( 0, 6 );
				ConstitutionBonus = random.Next( 0, 6 );
				CharismaBonus = random.Next( 0, 6 );

				switch ( SocioEconomicStatus ) {
				case SocietyRank.Lower:
					MaxMembers = random.Next( CachedMembers.Count, 30 );
					break;
				case SocietyRank.Middle:
					MaxMembers = random.Next( CachedMembers.Count, 12 );
					break;
				case SocietyRank.Upper:
					MaxMembers = random.Next( CachedMembers.Count, 3 );
					break;
				};

				Members = new Thinker[ MaxMembers ];
				for ( int i = 0; i < CachedMembers.Count; i++ ) {
					Members[i] = CachedMembers[i];
				}
				MemberCount = CachedMembers.Count;
				CachedMembers.Clear();
			}
		}

		public static Family Create( Settlement location ) {
			Random random = new Random();
			SocietyRank rank;
			for ( rank = SocietyRank.Lower; rank < SocietyRank.Upper; rank++ ) {
				if ( location.GetPercentageOfSocietyRank( rank ) < location.GetSocietyRankMaxPercentage( rank ) ) {
					break;
				}
			}

			Family family = new Family( random, rank );

			family.FamilyName = NameGenerator.GenerateLastName();
			family.Name = string.Format( "{0}{1}", family.FamilyName, family.GetHashCode() );

			location.AssignHouse( family );
			location.AddFamily( family );

			GD.Print( "Family " + family.Name + " Generated:" );
			GD.Print( "\tSocioEconomicStatus: " + family.SocioEconomicStatus );
			GD.Print( "\tMaxMembers: " + family.MaxMembers );
			GD.Print( "\tHome: " + family.Home.GetPath() );

			return family;
		}
	};
};