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
using static AcDbWrapper.LayerExtension;

namespace AcDbWrapper
{
    /// <summary>
    /// Methods to process attributes and xdata as a dictionary.
    /// </summary>
    public static partial class AttributeExtension
    {
		/// <summary>
		/// Gets data in the form of attributes from a <see cref="BlockReference"/> and XData from a <see cref="DBObject"/>.
		/// </summary>
		/// <param name="objectId">The object ID to read.</param>
		/// <param name="attributes">The attributes object to fill.</param>
		public static void GetAttributes(this Attributes attributes, ObjectId objectId)
        {
			if (objectId.IsMatch<DBObject>())
            {
                if (objectId.IsMatch<BlockReference>())
                {
                    // Get transaction
                    bool started = objectId.Database.GetOrStartTransaction(out Transaction transaction);
                    using Disposable disposable = new(transaction, started);

                    // Get drawing object
                    using BlockReference blockReference = (BlockReference)transaction.GetObject(objectId, OpenMode.ForRead);

                    // Get Attributes
                    AttributeCollection collection = blockReference.AttributeCollection;

                    // Read attributes
                    foreach (ObjectId id in collection)
                    {
                        using AttributeReference attribute = transaction.GetObject(id, OpenMode.ForWrite) as AttributeReference;
                        string tag = attribute.Tag;
                        string textstring = attribute.TextString;

                        attributes[tag] = new DataPoint(XDataType.ASCII, textstring, id);
                    }
                }

                Attributes xdata = new()
                {
                    Prefix = attributes.Prefix,
                    Suffix = attributes.Suffix,
                };

                // Read XData
                objectId.GetXData(xdata);
                foreach (KeyValuePair<string, DataPoint> item in xdata)
                {
                    if (attributes.ContainsKey(item.Key))
                        continue;

                    if (item.Value.Code == XDataType.ASCII
                     || item.Value.Code == XDataType.Handle)
                    {
                        // Get XData
                        attributes[item.Key] = item.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets data in the form of attributes from a <see cref="BlockReference"/> and XData from a <see cref="DBObject"/>.
        /// </summary>
		/// <param name="objectId">The object ID to read.</param>
        /// <returns>Attributes object that holds both attributes and XData.</returns>
        public static Attributes GetAttributes(ObjectId objectId)
		{
			Attributes attributes = new();
			GetAttributes(attributes, objectId);

			return attributes;
        }

        /// <summary>
        /// Set Attributes of a <see cref="BlockReference"/>. If data in the dictionary does not have an <see cref="AttributeReference"/> to write to, it converts the data to XData and writes to the Block Reference.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributes"></param>
		/// <param name="xdata">Used for </param>
        /// <remarks>
        /// <see cref="Dictionary{TKey, TValue}"/> uses the attribute tag as a key. The value of <see cref="Tuple{T, T}"/> is a typecode, string pair. The typecode can either be 1001 (for standard attribute textstring) or 1005 (for handle references).
        /// </remarks>
        public static void SetAttributes(this Attributes attributes, ObjectId objectId)
        {
            // Return if no data to write
            if (attributes.Count == 0)
                return;

            if (objectId.IsMatch<DBObject>())
            {
                Database database = objectId.Database;

                bool started = database.GetOrStartTransaction(out Transaction transaction);
                using Disposable disposable = new(transaction, started);

                using DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
                

                // Attributes that have not been output yet
                Attributes overflowAttributes = new(attributes)
                {
                    Prefix = attributes.Prefix,
                    Suffix = attributes.Suffix
                };

                // Make sure the block reference is able to be written
                dbObject.UpgradeOpen();

                if (objectId.IsMatch<BlockReference>())
                {
                    // Switch database to allow attribute text alignment adjustments
                    using WorkingDatabaseSwitcher switcher = new(database);

                    // Layer states to reset
                    List<Disposable> layerStates = new();
                    
                    // Reset layer states when leaving the code block
                    using Disposable resetLayerStates = new(() => {
                        foreach (Disposable layerState in layerStates)
                        {
                            layerState.Dispose();
                        }
                    });

                    foreach (ObjectId id in ((BlockReference)dbObject).AttributeCollection)
                    {
                        if (id.IsMatch<AttributeReference>())
                        {
                            using AttributeReference attributeReference = transaction.GetObject(id, OpenMode.ForRead) as AttributeReference;
                            
                            if (attributes.TryGetValue(attributeReference.Tag, out DataPoint value))
                            {
                                // Enable writing
                                attributeReference.UpgradeOpen();

                                // Unlock layer temporarily
                                Disposable attributeLayerState = TempLayerState(attributeReference.LayerId, LayerState.Unlock);
                                
                                // Reset states later
                                layerStates.Add(attributeLayerState);

                                // Write to attribute
                                attributeReference.TextString = value.Text;
                                
                                // Adjust text to alignment after writing changes
                                attributeReference.AdjustAlignment(database);

                                // Remove from unused attributes
                                overflowAttributes.Remove(attributeReference.Tag);
                            }
                        }
                    }
                }

                // Write remaining attribute data to XData
                objectId.SetXData(overflowAttributes);
            }
        }
	}

    public static partial class AttributeExtension
    {
        /// <summary>
        /// Gets Attributes of a <see cref="BlockReference"/>. This takes both data in the attribute collection and XData.
        /// </summary>
        /// <param name="dbObject"></param>
        /// <returns><see cref="Dictionary{TKey, TValue}"/></returns>
        /// <remarks>
        /// The <see cref="Dictionary{TKey, TValue}"/> uses the attribute tag as a key. The value of <see cref="Tuple{T, T}"/> is a typecode, string pair. The typecode can either be 1001 (for standard attribute textstring) or 1005 (for handle references).
        /// </remarks>
        [Obsolete]
        public static Dictionary<string, Attrib> GetAttributes(this DBObject dbObject)
        {
            Dictionary<string, Attrib> attributes = new();

            bool started = dbObject.Database.GetOrStartTransaction(out Transaction transaction);
            using Disposable disposable = new(transaction, started);

            // Get Attributes
            if (dbObject.GetType() == typeof(BlockReference))
            {
                BlockReference blockReference = dbObject as BlockReference;
                AttributeCollection collection = blockReference.AttributeCollection;
                foreach (ObjectId id in collection)
                {
                    using AttributeReference attribute = transaction.GetObject(id, OpenMode.ForWrite) as AttributeReference;
                    string tag = attribute.Tag;
                    string textstring = attribute.TextString;
                    attributes[tag] = new Attrib(1000, textstring);
                }
            }

            Dictionary<string, Attrib> xdata = dbObject.GetXData();
            foreach (KeyValuePair<string, Attrib> item in xdata)
            {
                if (attributes.ContainsKey(item.Key))
                    continue;

                attributes[item.Key] = item.Value;
            }

            return attributes;
        }

        /// <summary>
        /// Gets Attributes of a <see cref="BlockReference"/>. This takes both data in the attribute collection and XData.
        /// </summary>
        /// <param name="dbObject"></param>
        /// <returns><see cref="Dictionary{TKey, TValue}"/></returns> 
        /// <remarks>
        /// The <see cref="Dictionary{TKey, TValue}"/> uses the attribute tag as a key. The value of <see cref="Tuple{T, T}"/> is a typecode, string pair. The typecode can either be 1001 (for standard attribute textstring) or 1005 (for handle references).
        /// </remarks>

        [Obsolete]
        public static Dictionary<string, Tuple<Attrib, ObjectId?>> GetAttributesWithIds(this DBObject dbObject)
        {
            Dictionary<string, Tuple<Attrib, ObjectId?>> attributes = new();

            bool started = dbObject.Database.GetOrStartTransaction(out Transaction transaction);
            using Disposable disposable = new(transaction, started);

            // Get Attributes
            if (dbObject.GetType() == typeof(BlockReference))
            {
                BlockReference blockReference = dbObject as BlockReference;
                AttributeCollection collection = blockReference.AttributeCollection;
                foreach (ObjectId id in collection)
                {
                    using AttributeReference attribute = transaction.GetObject(id, OpenMode.ForWrite) as AttributeReference;
                    string tag = attribute.Tag;
                    string textstring = attribute.TextString;
                    attributes[tag] = new(new(1000, textstring), id);
                }
            }

            Dictionary<string, Attrib> xdata = dbObject.GetXData();
            foreach (KeyValuePair<string, Attrib> item in xdata)
            {
                if (attributes.ContainsKey(item.Key))
                    continue;

                attributes[item.Key] = new(item.Value, null);
            }

            return attributes;
        }

        /// <summary>
        /// Set Attributes of a <see cref="BlockReference"/>. If data in the dictionary does not have an <see cref="AttributeReference"/> to write to, it converts the data to XData and writes to the Block Reference.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributes"></param>
        /// <remarks>
        /// <see cref="Dictionary{TKey, TValue}"/> uses the attribute tag as a key. The value of <see cref="Tuple{T, T}"/> is a typecode, string pair. The typecode can either be 1001 (for standard attribute textstring) or 1005 (for handle references).
        /// </remarks>
        [Obsolete]
        public static void SetAttributes(this Entity entity, Dictionary<string, Attrib> attributes)
        {
            // Return if no data to write
            if (attributes.Count == 0)
                return;

            bool started = entity.Database.GetOrStartTransaction(out Transaction transaction);
            using Disposable disposable = new(transaction, started);

            // Unlock layer prior to editing entity
            using LayerTableRecord layer = transaction.GetObject(entity.LayerId, OpenMode.ForRead) as LayerTableRecord;
            bool locked = layer.IsLocked;
            if (locked)
            {
                layer.UpgradeOpen();
                layer.IsLocked = false;
            }

            using Disposable exitMethod = new(() =>
            {
                if (locked)
                    layer.IsLocked = true;
            });

            // Make sure the block reference is able to be written
            entity.UpgradeOpen();
            Database database = entity.Database;

            // Switch database to allow attribute text alignment adjustments
            using WorkingDatabaseSwitcher switcher = new(database);

            // Attributes that have not been output yet
            Dictionary<string, Attrib> overflowAttributes = new(attributes);

            if (entity.GetType() == typeof(BlockReference))
            {
                // Get attribute collection
                AttributeCollection collection = ((BlockReference)entity).AttributeCollection;

                // No attributes, just adjust current attribute alignment for good measure
                if (attributes == null)
                {
                    foreach (ObjectId id in collection)
                    {
                        using AttributeReference attribute = transaction.GetObject(id, OpenMode.ForWrite) as AttributeReference;

                        attribute.AdjustAlignment(database);
                    }

                    return;
                }

                // input attribute value, adjust alignment
                foreach (ObjectId id in collection)
                {
                    using AttributeReference attribute = transaction.GetObject(id, OpenMode.ForWrite) as AttributeReference;

                    if (attributes.ContainsKey(attribute.Tag))
                    {
                        overflowAttributes.Remove(attribute.Tag);

                        attribute.TextString = attributes[attribute.Tag].Text;
                    }

                    attribute.AdjustAlignment(database);
                }
            }

            // Write remaining attribute data to XData
            transaction.SetXData(entity, overflowAttributes);
        }

        /// <summary>
        /// Set Attributes of a <see cref="BlockReference"/>. If data in the dictionary does not have an <see cref="AttributeReference"/> to write to, it converts the data to XData and writes to the Block Reference.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributes"></param>
        /// <remarks>
        /// <see cref="Dictionary{TKey, TValue}"/> uses the attribute tag as a key. The value of <see cref="Tuple{T, T}"/> is a typecode, string pair. The typecode can either be 1001 (for standard attribute textstring) or 1005 (for handle references).
        /// </remarks>
        [Obsolete]
        public static void SetAttributes(this Entity entity, Dictionary<string, Tuple<Attrib, ObjectId?>> attributes)
        {
            Dictionary<string, Attrib> dictionary = new();

            foreach (var item in attributes)
                dictionary[item.Key] = item.Value.Item1;

            entity.SetAttributes(dictionary);
        }
    }
}