using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace ACAD_Utilities
{
	/// <summary>
	/// A container of methods to check the validity of AutoCAD structs and objects.
	/// </summary>
	public static class Check
	{
		#region Static Methods

		/// <summary>
		/// Check Object Id for errors.
		/// </summary>
		/// <param name="id">ObjectId to check.</param>
		/// <param name="error">Flag to throw error.</param>
		/// <returns>True if no errors found; otherwise, false.</returns>
		public static bool Validate(this ObjectId id, bool error)
		{
			if (error)
			{
				if (id.IsNull)
					throw new NullReferenceException("Transaction is null");

				if (id.IsEffectivelyErased || id.IsErased)
					throw new AccessViolationException("Object Id is erased");

				if (!id.IsValid)
					throw new Exception("Object Id is not valid");
			}
			else
			{
				if (id.IsNull || id.IsEffectivelyErased || id.IsErased || !id.IsValid)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Check transaction for errors.
		/// </summary>
		/// <param name="transaction">Transaction to check.</param>
		/// <param name="argument">Flag whether it is an argument check.</param>
		/// <returns></returns>
		public static bool Validate(this Transaction transaction, bool argument)
		{
			if (argument)
			{
				if (transaction == null)
					throw new ArgumentNullException("Transaction is null");
			}
			else
			{
				if (transaction == null)
					throw new NullReferenceException("Transaction is null");
			}

			if (transaction.IsDisposed)
				throw new ObjectDisposedException("Transaction has been disposed");

			return true;
		}

		/// <summary>
		/// Check database for errors.
		/// </summary>
		/// <param name="database">Database to check.</param>
		/// <param name="argument">Flag whether it is an argument check.</param>
		/// <returns></returns>
		public static bool Validate(this Database database, bool argument)
		{
			if (argument)
			{
				if (database == null)
					throw new ArgumentNullException("Database is null");
			}
			else
			{
				if (database == null)
					throw new NullReferenceException("Database is null");
			}

			if (database.IsBeingDestroyed)
				throw new AccessViolationException("Database cannot be used, it is being destroyed");
			if (database.IsDisposed)
				throw new ObjectDisposedException("Database has been disposed");

			return true;
		}

		#endregion
	}
}
