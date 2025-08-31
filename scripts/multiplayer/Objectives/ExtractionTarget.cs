using Godot;
using Renown.Thinkers;
using Steamworks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Renown.Thinkers.GoapCache;
using Steam;

namespace Multiplayer.Objectives {
	public partial class ExtractionTarget : Thinker {
		private string Personality;

		public override void _Ready() {
			base._Ready();

			Personality = SteamMatchmaking.GetLobbyData( SteamLobby.Instance.LobbyId, "target_personality" );

			Agent = new MountainGoap.Agent(
				name: "ExtractionTarget",
				state: new ConcurrentDictionary<string, object?> {
				},
				memory: new Dictionary<string, object?> {
				},
				goals: GoapAllocator.GetGoalList( Personality ),
				actions: GoapAllocator.GetActionList( Personality ),
				sensors: GoapAllocator.GetSensorList( Personality ),
				costMaximum: float.MaxValue,
				stepMaximum: 10
			);
		}
	};
};