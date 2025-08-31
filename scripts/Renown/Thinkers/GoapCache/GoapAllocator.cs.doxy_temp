using System.Collections.Generic;

namespace Renown.Thinkers.GoapCache {
	public class GoapAllocator {
		public static List<MountainGoap.Action> GetActionList( string thinkerId ) {
			EntityGoapData data = (EntityGoapData)ResourceCache.GetResource( "res://resources/goap_cache/" + thinkerId + "_goap_list.tres" );
			List<MountainGoap.Action> actions = new List<MountainGoap.Action>( data.AllowedActions.Count );
			for ( int i = 0; i < data.AllowedActions.Count; i++ ) {
				actions.Add( GoapActionCache.Cache[ data.AllowedActions[ i ] ] );
			}
			return actions;
		}
		public static List<MountainGoap.BaseGoal> GetGoalList( string thinkerId ) {
			EntityGoapData data = (EntityGoapData)ResourceCache.GetResource( "res://resources/goap_cache/" + thinkerId + "_goap_list.tres" );
			List<MountainGoap.BaseGoal> goals = new List<MountainGoap.BaseGoal>( data.AllowedGoals.Count );
			for ( int i = 0; i < data.AllowedGoals.Count; i++ ) {
				goals.Add( GoapGoalCache.Cache[ data.AllowedGoals[ i ] ] );
			}
			return goals;
		}
		public static List<MountainGoap.Sensor> GetSensorList( string thinkerId ) {
			EntityGoapData data = (EntityGoapData)ResourceCache.GetResource( "res://resources/goap_cache/" + thinkerId + "_goap_list.tres" );
			List<MountainGoap.Sensor> sensors = new List<MountainGoap.Sensor>( data.AllowedSensors.Count );
			for ( int i = 0; i < data.AllowedSensors.Count; i++ ) {
				sensors.Add( GoapSensorCache.Cache[ data.AllowedSensors[ i ] ] );
			}
			return sensors;
		}
	};
};