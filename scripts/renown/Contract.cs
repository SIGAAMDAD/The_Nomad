using Godot;
using DialogueManagerRuntime;

namespace Renown {
	public partial class Contract : Quest {
		public enum ContractType {
			Count
		};

		[Export]
		private ContractType Type;
		[Export]
		private Thinker Contractor;

		private System.Collections.Generic.List<Resource> OptionalObjectives;

		public Contract( Thinker contractor, Resource quest, ContractType type ) {
			Contractor = contractor;
			QuestResource = quest;
			Type = type;
		}

		public void AddOptionalObjective( Resource objective ) {
			GDScript script = ResourceLoader.Load<GDScript>( "res://addons/questify/scripts/model/quest_objective.gd" );
			OptionalObjectives.Add( (Resource)script.New() );
		}

		public Resource GetQuest() {
			return QuestResource;
		}
		public ContractType GetContractType() {
			return Type;
		}

		private void OnContractStarted( Resource quest ) {
			
		}
		private void OnContractObjectiveAdded( Resource quest, Resource objective ) {

		}
		public override void _Ready() {
			base._Ready();

			Resource instance = (Resource)QuestResource.Call( "instantiate" );
			GetNode( "/root/Questify" ).Connect( "quest_started", Callable.From<Resource>( OnContractStarted ) );
			GetNode( "/root/Questify" ).Connect( "quest_objective_added", Callable.From<Resource, Resource>( OnContractObjectiveAdded ) );

			GetNode( "/root/Questify" ).Call( "start_quest", instance );
		}
    };
};