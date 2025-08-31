using System.Collections.Generic;
using Godot;

public enum QuestType {
	Contract,
	Mission,
	
	Count
};
public enum QuestStatus {
	Completed,
	Active,
	Failed,
	
	Count
};

namespace Renown {
	public abstract partial class Quest : Node {
		protected QuestStatus Status;
		protected List<QuestObjective> Objectives;

		protected int CompletedObjectives = 0;
		protected int FailedObjectives = 0;

		public abstract string GetQuestName();
		public abstract QuestType GetQuestType();
		public abstract QuestStatus GetStatus();

		public abstract void StartBaseQuest();
	};
};