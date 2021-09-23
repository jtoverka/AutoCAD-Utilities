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

namespace ACAD_Utilities
{
    /// <summary>
    /// Provides extended methods to work with XData.
    /// </summary>
    public static class XDataLibrary
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Dictionary<string, Tuple<int, string>> GetXData(this Entity entity)
        {
            Dictionary<string, Tuple<int, string>> attributes = new();

            // Get xdata
            using ResultBuffer xdata = entity.XData;
            if (xdata != null)
            {
                string application = null;
                foreach (TypedValue typedValue in xdata)
                {
                    // Registered Application
                    if (typedValue.TypeCode == 1001)
                        application = typedValue.Value.ToString().Replace("VIA_WD_", "");

                    // Only accept strings or handles
                    if ((typedValue.TypeCode == 1000
                        || typedValue.TypeCode == 1005)
                        && application != null)
                    {
                        string value = typedValue.Value.ToString();
                        attributes.Add(application, new Tuple<int, string>(typedValue.TypeCode, value));
                        application = null;
                    }
                }
            }

            return attributes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="entity"></param>
        /// <param name="attributes"></param>
        public static void SetXData(this Transaction transaction, Entity entity, Dictionary<string, Attrib> attributes)
        {
            // Make sure the block reference is able to be written
            entity.UpgradeOpen();
            Database database = entity.Database;

            // The attributes that have not been output yet will be written to the block reference as XData
            using ResultBuffer buffer = new();
            using ResultBuffer xdata = entity.XData;
            string tag = null;

            // XData that has not been output yet
            Dictionary<string, Attrib> overflowXData = new(attributes);

            // Overwrite existing XData 'attributes'
            if (xdata != null)
            {
                foreach (TypedValue typedValue in xdata)
                {
                    if (typedValue.TypeCode == 1001)
                    {
                        tag = typedValue.Value.ToString().Replace("VIA_WD_", "");
                        buffer.Add(typedValue);
                    }
                    if ((typedValue.TypeCode == 1000
                        || typedValue.TypeCode == 1005)
                        && tag != null)
                    {
                        TypedValue value;

                        if (attributes.ContainsKey(tag))
                        {
                            overflowXData.Remove(tag);
                            value = new TypedValue(attributes[tag].Code, attributes[tag].Text);
                        }
                        else
                        {
                            value = typedValue;
                        }

                        tag = null;
                        buffer.Add(value);
                    }
                    else
                    {
                        buffer.Add(typedValue);
                    }
                }
            }

            using RegAppTable table = transaction.GetObject(database.RegAppTableId, OpenMode.ForWrite) as RegAppTable;

            // If attribute data does not have an attribute or existing XData to write to, create new XData
            foreach (KeyValuePair<string, Attrib> attribute in overflowXData)
            {
                AddRegAppTableRecord(transaction, database, "VIA_WD_" + attribute.Key);

                buffer.Add(new TypedValue(1001, "VIA_WD_" + attribute.Key));
                buffer.Add(new TypedValue(1002, "{"));
                buffer.Add(new TypedValue(attribute.Value.Code, attribute.Value.Text));
                buffer.Add(new TypedValue(1002, "}"));
            }

            entity.XData = buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="db"></param>
        /// <param name="name"></param>
        public static void AddRegAppTableRecord(Transaction transaction, Database db, string name)
        {
            using RegAppTable regAppTable = transaction.GetObject(db.RegAppTableId, OpenMode.ForRead) as RegAppTable;

            if (!regAppTable.Has(name))
            {
                regAppTable.UpgradeOpen();
                using RegAppTableRecord regAppTableRecord = new()
				{
					Name = name
				};
				regAppTable.Add(regAppTableRecord);
                transaction.AddNewlyCreatedDBObject(regAppTableRecord, true);
            }
        }
    }
}
