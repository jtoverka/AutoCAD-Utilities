using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace ACAD_Utilities
{
	/// <summary>
	/// A container of vanilla AutoCAD commands
	/// </summary>
	public static class Commands
	{
		/// <summary>
		/// Replicates the overkill command.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		[CommandMethod("AcOverkill")]
		public static ResultBuffer OverkillCommand(ResultBuffer args)
		{
			ResultBuffer result = new();

			Database database = Application.DocumentManager.MdiActiveDocument.Database;
			Transaction transaction = database.TransactionManager.StartTransaction();

			Overkill overkill = new(transaction, database);
			overkill.Overkill_All();

			return result;
		}
	}
}
