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
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AcDbWrapper
{
    /// <summary>
    /// Represents a database table.
    /// </summary>
    public class ObjectIdSet : HashSet<ObjectId>, IUniqueDatabase
    {
        /// <summary>
        /// The <see cref="ObjectId.Database"/> this table accepts.
        /// </summary>
        /// <remarks>
        /// All other types will throw an <see cref="ArgumentException"/> when attempting to add.
        /// </remarks>
        public Database Database { get; }

        /// <summary>
        /// The <see cref="ObjectId.ObjectClass"/> this table accepts.
        /// </summary>
        /// <remarks>
        /// All other types will throw an <see cref="ArgumentException"/> when attempting to add.
        /// </remarks>
        public RXClass ObjectClass { get; }

        /// <summary>
        /// Adds the specified element in a set.
        /// </summary>
        /// <remarks>
        /// Can only accept an <see cref="ObjectId"/> with an <see cref="RXClass">ObjectClass</see> that matches this object's <see cref="ObjectIdSet.ObjectClass"/>.
        /// </remarks>
        /// <param name="objectId">The ObjectId with an <see cref="ObjectId.ObjectClass"/> that matches the <see cref="ObjectIdSet.ObjectClass"/>.</param>
        /// <returns><see langword="true"/> if the element is added to the <see cref="HashSet{ObjectId}"/>; <see langword="false"/> if the element is already present.</returns>
        /// <exception cref="ArgumentException"></exception>
        public new bool Add(ObjectId objectId)
        {
            if (!(objectId.ObjectClass == ObjectClass || objectId.ObjectClass.IsDerivedFrom(ObjectClass)))
            {
                throw new ArgumentException(nameof(ObjectIdSet) + " of type '" + this.ObjectClass.Name + "' cannot hold an object of type '" + objectId.ObjectClass.Name + "'");
            }
            if (objectId.Database != Database)
            {
                throw new ArgumentException(nameof(ObjectIdSet) + " of database '" + Database.OriginalFileName + "' does not hold objects from '" + objectId.Database.OriginalFileName + "'");
            }
            return base.Add(objectId);
        }

        /// <summary>
        /// Checks if the <paramref name="objectId"/> can be added to this table.
        /// </summary>
        /// <param name="objectId">The <see cref="ObjectId"/> to check.</param>
        /// <returns><see langword="true"/> if the object can be added to this table; otherwise, <see langword="false"/>.</returns>
        public bool CanAdd(ObjectId objectId)
        {
            return objectId.IsMatch(ObjectClass)
                && (objectId.Database == Database);
        }

        /// <summary>
        /// Initialize a new instance of this class.
        /// </summary>
        /// <param name="database">The drawing database.</param>
        /// <param name="rXClass">All added <see cref="ObjectId"/> objects are limited to this class.</param>
        public ObjectIdSet(Database database, RXClass rXClass)
        {
            Database = database;
            ObjectClass = rXClass;
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <remarks>
        /// Does not add <paramref name="objectId"/> to table.
        /// </remarks>
        /// <param name="objectId">The <see cref="ObjectId"/> to base this table on.</param>
        public ObjectIdSet(ObjectId objectId) : this(objectId.Database, objectId.ObjectClass) { }
    }
}
