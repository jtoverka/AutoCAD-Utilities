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
using Autodesk.AutoCAD.Runtime;
using System.Windows;

namespace ACADE_Utilities
{
	/// <summary>
	/// A container of AutoCAD Electrical commands
	/// </summary>
	public static class AeCommands
	{
		/// <summary>
		/// Represents the Audit Command
		/// </summary>
		[CommandMethod("AeAudit")]
		public static void Audit()
		{
			try
			{
				Database database = Active.Database;
				bool started = database.GetOrStartTransaction(out Transaction transaction);
				using Disposable disposable = new(transaction, started);

				AeAudit aeAudit = new(database);
				aeAudit.BogusWireNumber = true;
				aeAudit.WireGap = true;
				aeAudit.WireNumberFloater = true;
				aeAudit.ZeroLengthWires = true;

				aeAudit.Audit();

				if (started)
					transaction.Commit();
			}
			catch (System.Exception e)
			{
				MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
