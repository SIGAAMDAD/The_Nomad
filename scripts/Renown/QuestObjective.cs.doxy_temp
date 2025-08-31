using Godot;
using System.Collections.Generic;

public partial class QuestObjective : Node {
	private Dictionary<string, QuestStatus> Conditions;
	private string Description;
	private QuestStatus Status = QuestStatus.Active;
	private bool Required = false;
	
	public bool IsRequired() => Required;
	public void SetCondition( string key, QuestStatus status ) => Conditions[ key ] = status;
	public QuestStatus GetStatus() => Status;
	public void SetStatus( QuestStatus status ) => Status = status;
	
	public QuestObjective( string description, Dictionary<string, QuestStatus> conditions, bool required ) {
		Description = description;
		Conditions = conditions;
		Required = required;
		Status = QuestStatus.Active;
	}
	public QuestObjective() {
	}
	
	public static QuestObjective FromResource( QuestObjectiveResource resource ) {
		QuestObjective objective = new QuestObjective();
		
		objective.Conditions = new Dictionary<string, QuestStatus>( resource.Conditions.Count );
		foreach ( var condition in resource.Conditions ) {
			objective.Conditions.Add( condition.Key, condition.Value );
		}
		
		objective.Required = resource.Required;
		objective.Description = resource.Description;
		objective.Status = QuestStatus.Active;
		
		return objective;
	}
	
	public QuestStatus Update() {
		int activeConditions = 0;
		foreach ( var condition in Conditions ) {
			switch ( condition.Value ) {
			case QuestStatus.Failed:
			case QuestStatus.Completed:
				break;
			case QuestStatus.Active:
				activeConditions++;
				break;
			default:
				break;
			};
		}
		
		if ( activeConditions == 0 ) {
			Status = QuestStatus.Completed;
		}
		
		return Status;
	}
};