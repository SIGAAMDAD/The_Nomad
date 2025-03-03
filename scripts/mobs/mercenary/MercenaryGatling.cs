using System.Collections.Concurrent;
using Godot;
using MountainGoap;
using System.Collections.Generic;

public partial class MercenaryGatling : MobBase {
	public override void _Ready() {
		base._Ready();

		Init();

		/*
		Navigation.Connect( "target_reached", Callable.From( OnTargetReached ) );L
		
		List<BaseGoal> goals = new List<BaseGoal>();
		for ( int i = 0; i < Goals.Count; i++ ) {
			goals[i] = Goals[i];
		}

		Agent = new Agent(
			"MercenaryGatling",
			new ConcurrentDictionary<string, object>{

			},
			null,
			goals,
			Actions,
			null
		);
		*/
	}
};