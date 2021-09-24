using Autodesk.AutoCAD.DatabaseServices;

namespace ACAD_Utilities
{
	/// <summary>
	/// Provides the extension methods to handle database transactions.
	/// </summary>
	public static class TransactionExtensions
	{
		/// <summary>
		/// Creates a new transaction if one does not exist.
		/// </summary>
		/// <param name="database">The database to perform transactional operations on.</param>
		/// <param name="transaction">The variable to pass the transaction to.</param>
		/// <returns><c>true</c> if a new transaction was started and needs to be disposed by the caller; otherwise, false.</returns>
		public static bool GetOrStartTransaction(this Database database, out Transaction transaction)
		{
			database.Validate(true, true);

			TransactionManager manager = database.TransactionManager;

			if (manager.TopTransaction == null)
			{
				transaction = manager.StartTransaction();
				return true;
			}
			else
			{
				transaction = manager.TopTransaction;
				return false;
			}
		}
	}
}
