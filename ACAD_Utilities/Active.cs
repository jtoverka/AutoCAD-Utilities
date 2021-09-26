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

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace ACAD_Utilities
{
	/// <summary>
	/// Provides easy access to several "active" objects in the AutoCAD runtime environment.
	/// </summary>
	public static class Active
	{
		/// <summary>
		/// Returns the active Editor object.
		/// </summary>
		public static Editor Editor
		{
			get { return Document.Editor; }
		}

		/// <summary>
		/// Returns the active Document object.
		/// </summary>
		public static Document Document
		{
			get { return Application.DocumentManager.MdiActiveDocument; }
		}

		/// <summary>
		/// Returns the active Database object.
		/// </summary>
		public static Database Database
		{
			get { return Document.Database; }
		}

		/// <summary>
		/// Returns the working Database object.
		/// </summary>
		public static Database WorkingDatabase
		{
			get { return HostApplicationServices.WorkingDatabase; }
			set { HostApplicationServices.WorkingDatabase = value; }
		}

		/// <summary>
		/// Sends a string to the command line in the active Editor.
		/// </summary>
		/// <param name="message">The message to send.</param>
		public static void WriteMessage(string message)
		{
			Editor.WriteMessage(message);
		}

		/// <summary>
		/// Sends a string to the command line in the active Editor using String.Format.
		/// </summary>
		/// <param name="message">The message containing format specifications.</param>
		/// <param name="parameter">The variables to substitute into the format string.</param>
		public static void WriteMessage(string message, params object[] parameter)
		{
			Editor.WriteMessage(message, parameter);
		}
	}
}
