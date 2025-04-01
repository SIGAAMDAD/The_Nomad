using System;
using System.Collections.Generic;
using Godot;

namespace Renown.World {
	public enum AIAlignment {
		NeutralEvil,
		ChaoticEvil,
		LawfulEvil,

		Nuetral,
		ChaoticNeutral,
		LawfulNeutral,

		NeutralGood,
		ChaoticGood,
		LawfulGood,

		Count
	};

	public partial class Faction : Node {
		public static DataCache<Faction> Cache = null;

		[Export]
		private StringName FactionName;
		[Export]
		private string Description;
		[Export]
		private AIAlignment PrimaryAlignment;
		[Export]
		private Thinker Leader;
		[Export]
		private Thinker[] MemberList;
		[Export]
		private NavigationRegion2D TerritoryBounds;

		private Dictionary<NodePath, Thinker> MemberCache = new Dictionary<NodePath, Thinker>();

		public StringName GetFactionName() => FactionName;
		public AIAlignment GetAlignment() => PrimaryAlignment;
		public Thinker GetLeader() => Leader;

		public void Save() {
			SaveSystem.SaveSectionWriter writer = new SaveSystem.SaveSectionWriter( GetPath() );

			writer.SaveUInt( "alignment", (uint)PrimaryAlignment );
			writer.SaveString( "leader", Leader != null ? Leader.GetPath() : "nil" );
			writer.SaveInt( "member_count", MemberCache.Count );
			foreach ( var member in MemberCache ) {
				writer.SaveString( "hash", member.Key );
			}
			writer.Flush();
		}
		public void Load() {
			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( GetPath() );

			// save file compatibility
			if ( reader == null ) {
				return;
			}

			PrimaryAlignment = (AIAlignment)reader.LoadUInt( "alignment" );
			Leader = Thinker.Cache.SearchCache( reader.LoadString( "leader" ) );
			
			int numMembers = reader.LoadInt( "member_count" );
			MemberCache = new Dictionary<NodePath, Thinker>( numMembers );
			for ( int i = 0; i < numMembers; i++ ) {
				NodePath key = reader.LoadString( "hash" );
				MemberCache.Add( key, Thinker.Cache.SearchCache( key ) );
			}
		}

		public override void _Ready() {
			base._Ready();

			if ( SettingsData.GetNetworkingEnabled() ) {
			}
			if ( !IsInGroup( "Factions" ) ) {
				AddToGroup( "Factions" );
			}
		}
	};
};