using Godot;
using System.Collections.Generic;
using Renown.World;
using Renown.Thinkers;
using System;

namespace Renown {
	public partial class Contract : Resource {
		/// <summary>
		/// The name of the contract as it appears on the Mercenary Master's ledger
		/// </summary>
		public readonly string Title;

		/// <summary>
		/// Extra requirements that impact the final paycheck of a contract. These are usually rare and narrative driven.
		/// see <see cref="Contracts.Flags"/> for more information.
		/// </summary>
		public readonly Contracts.Flags Flags;

		/// <summary>
		/// The type of the contract, see <see cref="ContractType"/>
		/// </summary>T
		public readonly Contracts.Type Type;

		/// <summary>
		/// The target of the contract, not always filled.
		/// </summary>
		public readonly Thinker Target;

		/// <summary>
		/// The faction or NPC paying for the contract and the person who posted the contract
		/// </summary>
		public readonly Object Client;

		/// <summary>
		/// The guild that is handling the processing of the contract, sort of the "middleman"
		/// </summary>
		public readonly Faction Guild;

		/// <summary>
		/// The deadline the contract must be completed before to get the paycheck. Deadlines can occur when the player has
		/// high relation status with the client, but this is extremely rare.
		/// </summary>
		public WorldTimestamp DueDate { get; private set; }

		/// <summary>
		/// The total amount of money the mercenary can get out of the deal.
		/// this value can change with the value of the target, like a bounty
		/// for instance.
		/// </summary>
		public float TotalPay { get; private set; } = 0.0f;

		/// <summary>
		/// The amount of money the contract type costs.
		/// </summary>
		public float BasePay { get; private set; } = 0.0f;

		/// <summary>
		/// The designated place where the merc can operate in.
		/// if they step outside here, its no longer in the protection
		/// of the guild.
		/// </summary>
		public WorldArea CollateralArea { get; private set; }

		/*
		===============
		Contract
		===============
		*/
		public Contract( string name, WorldTimestamp duedate, Contracts.Flags flags, Contracts.Type type,
			float basePay, float totalPay, WorldArea area, Object client, Faction guild, Thinker target )
		{
			// check that all the parameters are actually valid
			ArgumentException.ThrowIfNullOrEmpty( name );
			ArgumentNullException.ThrowIfNull( area );
			ArgumentNullException.ThrowIfNull( client );
			ArgumentNullException.ThrowIfNull( guild );
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual( basePay, 0.0f );

			Title = name;
			Flags = flags;
			DueDate = duedate;
			Client = client;
			Guild = guild;
			TotalPay = totalPay;
			BasePay = basePay;
			CollateralArea = area;
			Target = target;

			Console.PrintLine( $"Contract {name} created, [type:{type}] [flags:[{flags}]], [client:{client}], [guild:{guild}], [basePay:{basePay}]" );
		}
		public Contract() {
		}

		/*
		===============
		CalculateCost
		===============
		*/
		/// <summary>
		/// calculates the cost of a generic contract with the given values,
		/// doesn't however take into account how the pay might change over time
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="type"></param>
		/// <param name="duedate"></param>
		/// <param name="area"></param>
		/// <returns></returns>
		public static float CalculateCost( in Contract contract ) {
			float cost = contract.BasePay;

			switch ( contract.Type ) {
				case Contracts.Type.Assassination:
				case Contracts.Type.Bounty:
				case Contracts.Type.Extortion:
					cost *= contract.Target.RenownScore;
					break;
				default:
					Console.PrintError( $"Contract.CalculateCost: invalid contract type {contract.Type}" );
					return 0.0f;
			}

			//
			// calculate flags
			//
			if ( ( contract.Flags & Contracts.Flags.Clean ) != 0 ) {
				cost += 2000.0f;
			}
			if ( ( contract.Flags & Contracts.Flags.Massacre ) != 0 ) {
				// a lot of cleanup
				cost += 1000.0f;
			}
			if ( ( contract.Flags & Contracts.Flags.Ghost ) != 0 ) {
				cost += 2500.0f;
			} else if ( ( contract.Flags & Contracts.Flags.Silent ) != 0 ) {
				cost += 3500.0f;
			}

			return cost;
		}

		/*
		===============
		CalculateCost
		===============
		*/
		public static float CalculateCost( Contracts.Type type, Contracts.Flags flags, Thinker target ) {
			float cost = type switch {
				Contracts.Type.Assassination => Contracts.Assassination.BASE_COST,
				Contracts.Type.Extortion => Contracts.Extortion.BASE_COST,
				Contracts.Type.Extraction => Contracts.Extraction.BASE_COST,
				_ => throw new ArgumentOutOfRangeException( nameof( type ) )
			};

			return cost;
		}

		/*
		===============
		Load
		===============
		*/
		public virtual void Load( int hashCode ) {
			using var reader = ArchiveSystem.GetSection( $"Contract{hashCode}" );
		}

		/*
		===============
		Start
		===============
		*/
		public void Start() {
			Dictionary<string, bool> state = new Dictionary<string, bool>();
			switch ( Type ) {
				case Contracts.Type.Assassination:
					state.Add( "TargetAlive", true );
					break;
				case Contracts.Type.Extortion:
					break;
			}

			QuestState.StartContract( this, Flags, state );
		}
	};
};