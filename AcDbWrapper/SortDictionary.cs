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
using System;
using System.Collections.Generic;

namespace AcDbWrapper
{
    /// <summary>
    /// Represents a sorted collection of <see cref="ObjectIdSet"/> with any key type.
    /// </summary>
    /// <typeparam name="TKey">The type of key</typeparam>
    public class SortDictionary<TKey> : Dictionary<TKey, ObjectIdSet>, IUniqueDatabase
    {
        /// <summary>
        /// The function to cancel adding an <see cref="ObjectIdSet"/> to the dictionary.
        /// </summary>
        private readonly Func<TKey, ObjectIdSet,bool> m_indexerCancelFunction;

        /// <summary>
        /// Gets the database this SortDictionary is attached to.
        /// </summary>
        public Database Database { get; }

        /// <summary>
        /// Gets or sets the 
        /// </summary>
        /// <param name="key">The key used to search dictionary.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public new ObjectIdSet this[TKey key]
        {
            get { return base[key]; }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                if (m_indexerCancelFunction(key, value))
                    return;

                base[key] = value;
            }
        }

        /// <summary>
        /// Initialize a new instance of this class.
        /// </summary>
        /// <param name="database">The database this sort dictionary belongs to.</param>
        /// <param name="indexerCancelFunction">The function to cancel adding an <see cref="ObjectIdSet"/> to this dictionary.</param>
        public SortDictionary(Database database, Func<TKey, ObjectIdSet, bool> indexerCancelFunction)
        {
            Database = database;
            m_indexerCancelFunction = indexerCancelFunction;
        }
    }
}