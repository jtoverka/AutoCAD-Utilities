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
		/// <returns><see langword="true"/> if a new transaction was started and needs to be disposed by the caller; Otherwise, <see langword="false"/>.</returns>
		public static bool GetOrStartTransaction(this Database database, out Transaction transaction)
		{
			TransactionManager manager = database.TransactionManager;
			bool start = manager.TopTransaction == null;

			if (start)
				transaction = manager.StartTransaction();
			else
				transaction = manager.TopTransaction;

			return start;
		}

		/// <summary>
		/// Commit and dispose of the transaction.
		/// </summary>
		/// <param name="transaction">The transaction to perform database operations.</param>
		public static void Finish(this Transaction transaction)
		{
			transaction.Commit();
			transaction.Dispose();
		}
	}
}
