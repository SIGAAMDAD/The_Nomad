using Renown.Thinkers;
using Renown.World;

namespace Renown.Contracts {

	public readonly struct AssassinationData {
		public readonly Thinker Target;
		public readonly string Name;
		public readonly WorldTimestamp DueDate;
		public readonly ContractFlags Flags;
		public readonly ContractType Type;
		public readonly float BasePay;
		public readonly WorldArea Area;
		public readonly Object Contractor;
		public readonly Faction Guild;

		public AssassinationData( string name, WorldTimestamp duedate, ContractFlags flags, ContractType type,
			float basePay, WorldArea area, Object contractor, Faction guild, Thinker Target )
		{
			Name = name;
			DueDate = duedate;
			Flags = flags;
			Type = type;
			BasePay = basePay;
			Area = area;
			Contractor = contractor;
			Guild = guild;
			this.Target = Target;
		}
	};

	public partial class Assassination : Contract {
		private readonly AssassinationData Data;

		public Assassination( in AssassinationData data )
			: base(
				name: data.Name,
				duedate: data.DueDate,
				flags: data.Flags,
				type: data.Type,
				basePay: data.BasePay,
				area: data.Area,
				contractor: data.Contractor,
				guild: data.Guild,
				totalPay: null
			)
		{
			Data.Target.Die += OnTargetDie;
		}

		private void OnTargetDie( Entity source, Entity target ) {
		}
	};
};