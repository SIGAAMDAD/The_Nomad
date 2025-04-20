namespace Renown.World.Settlements {
	public struct Income {
		public float Taxes;
		public float Exports;
		public uint GoldExtracted;
		public float Donated;
		public float Total;
	};
	public struct Expenses {
		public float Imports;
		public float Wages;
		public int Construction;
		public float Interest;
		public int Salary;
		public float Stolen;
		public float Debt;
		public float Total;
	};
	public struct FinanceOverview {
		public Income Income;
		public Expenses Expenses;
		public float NetInOut;
		public float Balance;
	};
};