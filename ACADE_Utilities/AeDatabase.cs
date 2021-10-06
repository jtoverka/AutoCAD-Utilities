/*This is free and unencumbered software released into the public domain.
*
*Anyone is free to copy, modify, publish, use, compile, sell, or
*distribute this software, either in source code form or as a compiled
*binary, for any purpose, commercial or non-commercial, and by any
*means.
*
*In jurisdictions that recognize copyright laws, the author or authors
*of this software dedicate any and all copyright interest in the
*software to the public domain. We make this dedication for the benefit
*of the public at large and to the detriment of our heirs and
*successors. We intend this dedication to be an overt act of
*relinquishment in perpetuity of all present and future rights to this
*software under copyright law.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
*EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
*MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
*IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
*OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
*ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
*OTHER DEALINGS IN THE SOFTWARE.
*
*For more information, please refer to <https://unlicense.org>
*/

using ACAD_Utilities;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace ACADE_Utilities
{
	/// <summary>
	/// A wrapper and factory for <see cref="Autodesk.AutoCAD.DatabaseServices.Database"/> object.
	/// </summary>
	public abstract class AeDatabase : IDisposable
	{
		#region Fields

		/// <summary>
		/// The database this class wraps
		/// </summary>
		protected readonly Database databaseField = new(true, true);

		/// <summary>
		/// The transaction used to perform operations on the database.
		/// </summary>
		protected readonly Transaction transactionField;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the AutoCAD Electrical Drawing.
		/// </summary>
		public AeDrawing Drawing { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		public AeDatabase()
		{
			transactionField = databaseField.TransactionManager.StartTransaction();
			Drawing = AeDrawing.GetOrCreate(databaseField);
			Initialize();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Releases the resources held by <see cref="Autodesk.AutoCAD.DatabaseServices.Database"/> and <see cref="Autodesk.AutoCAD.DatabaseServices.Transaction"/>.
		/// </summary>
		public void Dispose()
		{
			if (transactionField.Validate(false, false))
				transactionField.Dispose();

			if (databaseField.Validate(false, false))
				databaseField.Dispose();
		}

		/// <summary>
		/// Commits the transaction changes to the database.
		/// </summary>
		public void Commit()
		{
			if (transactionField.Validate(false, false))
				transactionField.Commit();
		}

		/// <summary>
		/// Initialize an AutoCAD Electrical drawing.
		/// </summary>
		/// <Remarks>
		/// Set plot configuration, layouts, textstyles. etc.
		/// </Remarks>
		protected abstract void Initialize();

		#endregion
	}
}
