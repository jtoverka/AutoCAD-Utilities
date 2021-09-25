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
			_ = args;

			Database database = Application.DocumentManager.MdiActiveDocument.Database;
			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			Overkill overkill = new(transaction, database);
			overkill.Overkill_All();

			return result;
		}
	}
}
