using Godot;

namespace Renown {
	public partial class StaticQuest : Node {
		[Export]
		protected Resource QuestResource;

		protected System.Collections.Generic.Dictionary<string, object> State =
			new System.Collections.Generic.Dictionary<string, object>();
		
		protected void OnConditionQueryRequest( string type, string key, Godot.Variant value, Resource requester ) {
		}

		public void Start() {
			Resource quest = (Resource)QuestResource.Call( "instantiate" );
			GetNode( "/root/Questify" ).Call( "start_quest", quest );
		}
	};
};