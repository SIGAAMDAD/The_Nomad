using Godot;
using System.Collections.Generic;
using Renown.World;

public enum ContractType : uint {
	/// <summary>
	/// fixed-price bounty, most usual target is a politician
	/// </summary>
	Assassination,

	Kidnapping,
	Extortion,

	/// <summary>
	/// an assassination but more for less important entities, this
	/// can include the player.
	/// 
	/// the price of a bounty can and most likely will increase over time
	/// </summary>
	Bounty,

	Count
};

public enum ContractFlags : uint {
	Silent = 0x0001,
	Ghost = 0x0002,
	Massacre = 0x0004,
	Clean = 0x0008,

	None = 0x0000
};

namespace Renown {
	public partial class Contract : Resource {
		public readonly string Title;
		public readonly ContractFlags Flags;
		public readonly ContractType Type;
		public readonly Object Contractor;
		public readonly Faction Guild;

		// not readonly to allow extensions
		public WorldTimestamp DueDate {
			get;
			private set;
		}

		/// <summary>
		/// the total amount of money the mercenary can get out of the deal.
		/// this value can change with the value of the target, like a bounty
		/// for instance.
		/// </summary>
		public float TotalPay {
			get;
			private set;
		} = 0.0f;
		public float BasePay {
			get;
			private set;
		} = 0.0f;

		/// <summary>
		/// the designated place where the merc can operate in.
		/// if they step outside here, its no longer in the protection
		/// of the guild.
		/// </summary>
		public WorldArea CollateralArea {
			get;
			private set;
		}

		public Contract( string name, WorldTimestamp duedate, ContractFlags flags, ContractType type,
			float basePay, float? totalPay, WorldArea area, Object contractor, Faction guild ) {
			Title = name;
			Flags = flags;
			DueDate = duedate;
			Contractor = contractor;
			Guild = guild;
			TotalPay = totalPay != null ? (float)totalPay : basePay;
			BasePay = basePay;
			CollateralArea = area;

			GD.Print( string.Format( "Contract {0} created, [type:{1}] [flags:[2]], [contractor:{3}], [guild:{4}], [basePay:{5}]", name, type, flags, contractor, guild, basePay ) );
		}
		public Contract() {
		}

		/// <summary>
		/// calculates the cost of a generic contract with the given values,
		/// doesn't however take into account how the pay might change over time
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="type"></param>
		/// <param name="duedate"></param>
		/// <param name="area"></param>
		/// <returns></returns>
		public static float CalculateCost( ContractFlags flags, ContractType type, WorldTimestamp duedate, WorldArea area, Object target = null ) {
			float cost;

			switch ( type ) {
				case ContractType.Assassination: {
						cost = 160.0f;
						if ( target is Entity entity && entity != null ) {
							cost *= entity.RenownScore;
						} else {
							Console.PrintError( string.Format( "Contract.CalculateCost: ContractType is Assassination, but target {0} isn't an entity", target ) );
						}
						break;
					}
				case ContractType.Bounty: {
						if ( target is Entity entity && entity != null ) {
							cost = entity.RenownScore;
						} else {
							Console.PrintError( string.Format( "Contract.CalculateCost: ContractType is Bounty, but target {0} isn't an entity", target ) );
							return 0.0f;
						}
						break;
					}
				default:
					Console.PrintError( string.Format( "Contract.CalculateCost: invalid contract type {0}", type ) );
					return 0.0f;
			}

			//
			// calculate flags
			//
			if ( ( flags & ContractFlags.Clean ) != 0 ) {
				cost += 2000.0f;
			}
			if ( ( flags & ContractFlags.Massacre ) != 0 ) {
				// a lot of cleanup
				cost += 1000.0f;
			}
			if ( ( flags & ContractFlags.Ghost ) != 0 ) {
				cost += 2500.0f;
			} else if ( ( flags & ContractFlags.Silent ) != 0 ) {
				cost += 3500.0f;
			}

			return cost;
		}

		public virtual void Load( int nHashCode ) {
			using ( var reader = ArchiveSystem.GetSection( nHashCode.ToString() ) ) {
			}
		}

		public void Start() {
			Dictionary<string, bool> state = new Dictionary<string, bool>();
			switch ( Type ) {
				case ContractType.Assassination:
					state.Add( "TargetAlive", true );
					break;
				case ContractType.Extortion:
					break;
			}
			;

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