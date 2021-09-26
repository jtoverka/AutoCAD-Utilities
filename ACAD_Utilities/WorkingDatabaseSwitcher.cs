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

using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ACAD_Utilities
{
    /// <summary>
    /// Switches the current <c>HostApplicationServices.WorkingDatabase</c> to the supplied database.
    /// </summary>
    public sealed class WorkingDatabaseSwitcher : IDisposable
    {
        private readonly Database previousDatabaseField;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="database">The database to switch to.</param>
        public WorkingDatabaseSwitcher(Database database)
        {
            previousDatabaseField = Active.WorkingDatabase;
            Active.WorkingDatabase = database;
        }

        /// <summary>
        /// Return the working database to the original working database.
        /// </summary>
        public void Dispose()
        {
            Active.WorkingDatabase = previousDatabaseField;
        }
    }
}
