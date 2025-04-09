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
	public partial class Contract : Resource {
		public readonly string Title;
		public readonly ContractFlags Flags;
		public readonly ContractType Type;
		public readonly Object Contractor;
		public readonly Faction Guild;

		// not readonly to allow extensions
		protected WorldTimestamp DueDate;
		
		protected uint Pay = 0;
		
		/// <summary>
		/// the designated place where the merc can operate in.
		/// if they step outside here, its no longer in the protection
		/// of the guild.
		/// </summary>
		private WorldArea CollateralArea;
		
		public Contract( string name, WorldTimestamp duedate, ContractFlags flags, ContractType type,
			uint basePay, WorldArea area, Object contractor, Faction guild )
		{
			Contractor = contractor;
			Guild = guild;
		}

		public virtual void Save() {

		}
		public virtual void Load() {
		}

		public void Start() {
			Dictionary<string, bool> state = new Dictionary<string, bool>();
			switch ( Type ) {
			case ContractType.Assassination:
				state.Add( "TargetAlive", true );
				break;
			};

			QuestState.StartContract( this, Flags, state );
		}

		public virtual ContractType GetContractType() => ContractType.Count;
		public WorldArea GetArea() => CollateralArea;
		
		/*
		public override string GetQuestName() => Title;
		public override QuestType GetQuestType() => QuestType.Contract;
		public override QuestStatus GetStatus() => Status;
		*/
	};
};