using Godot;

namespace Renown {
	public partial class DynamicQuest : Node2D {
		private string QuestName;

		private System.Collections.Generic.List<QuestObjective> Objectives;

		public DynamicQuest( string name ) {
			QuestName = name;
		}

		public void AddObjective() {
		}
	};
};