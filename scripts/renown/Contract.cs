using Godot;
using System.Collections.Generic;
using Renown.World;

public enum ContractType {
	Assassination,
	Kidnapping,
	Extortion,
	Bounty,
	
	Count
};

public enum ContractFlags {
	Silent		= 0x0001,
	Ghost		= 0x0002,
	Massacre	= 0x0004,
	Clean		= 0x0008,
	
	None		= 0x0000
};

namespace Renown {
	public partial class Contract : Quest {
		public readonly string Title;
		public readonly ContractFlags Flags;
		public readonly ContractType Type;
		public readonly Object Contractor;

		// not readonly to allow extensions
		protected WorldTimestamp DueDate;
		
		protected uint Pay = 0;
		
		/// <summary>
		/// the designated place where the merc can operate in.
		/// if they step outside here, its no longer in the protection
		/// of the guild.
		/// </summary>
		private WorldArea CollateralArea;
		
		public Contract( string name, List<QuestObjective> objectives, WorldTimestamp duedate, ContractFlags flags,
			ContractType type, uint basePay, WorldArea area, Object contractor )
		{
			Title = name;
			Objectives = objectives;
			Status = QuestStatus.Active;
			DueDate = duedate;
			CollateralArea = area;
			Type = type;
			Flags = flags;
			Pay = basePay;
			Contractor = contractor;
			
			SetProcess( false );
		}

		public virtual void Save() {
		}
		public virtual void Load() {
		}

		public void CheckStatus() {
			CompletedObjectives = 0;
			FailedObjectives = 0;
			int objectivesActive = 0;
			for ( int i = 0; i < Objectives.Count; i++ ) {
				switch ( Objectives[i].Update() ) {
				case QuestStatus.Completed:
					CompletedObjectives++;
					break;
				case QuestStatus.Failed:
					FailedObjectives++;
					break;
				case QuestStatus.Active:
					objectivesActive++;
					break;
				default:
					Console.PrintError( string.Format(
						"Contract._Process: unknown objective status ({0}), setting to active.", Objectives[i].GetStatus()
					) );
					Objectives[i].SetStatus( QuestStatus.Active );
					break;
				};
			}
			
			// if the merc has failed over half the objectives, cancel the contract
			if ( FailedObjectives > ( Objectives.Count * 0.5f ) ) {
				Status = QuestStatus.Failed;
				SetProcess( false );
			}
			else if ( objectivesActive == 0 ) {
				// finished
				Status = QuestStatus.Completed;
			}
		}
		
		public override void StartBaseQuest() {
			if ( WorldTimeManager.Now().LaterThan( DueDate ) ) {
				Status = QuestStatus.Failed;
				return;
			}
			SetProcess( true );
		}

		public virtual ContractType GetContractType() => ContractType.Count;
		public WorldArea GetArea() => CollateralArea;
		
		public override string GetQuestName() => Title;
		public override QuestType GetQuestType() => QuestType.Contract;
		public override QuestStatus GetStatus() => Status;
	};
};