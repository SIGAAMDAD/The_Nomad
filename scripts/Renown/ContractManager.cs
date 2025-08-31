using Godot;
using Renown.World;
using System.Collections.Generic;

namespace Renown {
	public class ContractManager {
		private static HashSet<Contract> Cache = null;

		public static void Init() {
			Cache = new HashSet<Contract>();
		}

		public static void Save() {
			using ( var writer = new SaveSystem.SaveSectionWriter( "ContractCache", ArchiveSystem.SaveWriter ) ) {
				writer.SaveInt( "Count", Cache.Count );

				int index = 0;
				foreach ( var contract in Cache ) {
					writer.SaveString( string.Format( "ContractName{0}", index ), contract.Title );
					writer.SaveUInt( string.Format( "ContractDueDateYear{0}", index ), contract.DueDate.GetYear() );
					writer.SaveUInt( string.Format( "ContractDueDateMonth{0}", index ), contract.DueDate.GetMonth() );
					writer.SaveUInt( string.Format( "ContractDueDateDay{0}", index ), contract.DueDate.GetDay() );
					writer.SaveUInt( string.Format( "ContractType{0}", index ), (uint)contract.Type );
					writer.SaveUInt( string.Format( "ContractFlags{0}", index ), (uint)contract.Flags );
					writer.SaveFloat( string.Format( "ContractBasePay{0}", index ), contract.BasePay );
					writer.SaveFloat( string.Format( "ContractTotalPay{0}", index ), contract.TotalPay );
					writer.SaveString( string.Format( "ContractArea{0}", index ), contract.CollateralArea.GetPath() );
					writer.SaveString( string.Format( "ContractContractor{0}", index ), ( contract.Contractor as Node ).GetPath() );
					writer.SaveString( string.Format( "ContractGuild{0}", index ), contract.Guild.GetPath() );

					index++;
				}
			}
		}
		public static void Load() {
			using ( var reader = ArchiveSystem.GetSection( "ContractCache" ) ) {
				Cache.Clear();

				int count = reader.LoadInt( "Count" );
				Cache.EnsureCapacity( count );

				System.Threading.Tasks.Parallel.For( 0, count, ( index ) => {
					WorldTimestamp duedate = new WorldTimestamp(
						Year: reader.LoadUInt( string.Format( "ContractDueDateYear{0}", index ) ),
						Month: reader.LoadUInt( string.Format( "ContractDueDateMonth{0}", index ) ),
						Day: reader.LoadUInt( string.Format( "ContractDueDateDay{0}", index ) )
					);

					Cache.Add(
						new Contract(
							name: reader.LoadString( string.Format( "ContractName{0}", index ) ),
							duedate: duedate,
							flags: (ContractFlags)reader.LoadUInt( string.Format( "ContractFlags{0}", index ) ),
							type: (ContractType)reader.LoadUInt( string.Format( "ContractType{0}", index ) ),
							basePay: reader.LoadFloat( string.Format( "ContractBasePay{0}", index ) ),
							totalPay: reader.LoadFloat( string.Format( "ContractTotalPay{0}", index ) ),
							area: WorldArea.Cache.SearchCache( reader.LoadString( string.Format( "ContractArea{0}", index ) ) ),
							contractor: ( (Node)Engine.GetMainLoop().Get( "root" ) ).GetNode<Object>( reader.LoadString( string.Format( "ContractContractor{0}", index ) ) ),
							guild: Faction.Cache.SearchCache( reader.LoadString( string.Format( "ContractGuild{0}", index ) ) )
						)
					);
				} );
			}
		}
	};
};