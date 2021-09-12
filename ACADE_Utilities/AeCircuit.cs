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
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ACAD_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using System.IO;
using System.Windows;

namespace ACADE_Utilities
{
    /// <summary>
    /// Represents a container for an AutoCAD Electrical circuit block.
    /// </summary>
    public class AeCircuit : IDisposable
    {
        #region Fields

        private static readonly Regex wireNo = new("^WD_WN");
        private static readonly Regex terminal = new("^X[0-9]TERM[0-9]{2}");
        private static readonly Regex blockTag = new("TAG");
        private static readonly Regex sigCode = new("^SIGCODE");

        private readonly Transaction transaction;
        
        // ObjectId of Line, ObjectId of WireNo Attribute
        private readonly Dictionary<ObjectId, ObjectId> wires = new();

        // ObjectId of WireNo Attribute, value of old TextString
        private readonly Dictionary<ObjectId, string> wireNoAttributes = new();

        // Collection of terminal attribute ObjectIds
        private readonly ObjectIdCollection terminalAttributes = new();

        // Collection of block reference ObjectIds containing the LINKTERM attribute
        private readonly ObjectIdCollection linkTermCollection = new();

        // ObjectId of WireNo Attribute, ObjectId Collection of Attributes tied to WireNo Attribute
        private readonly Dictionary<ObjectId, ObjectIdCollection> circuitNetwork = new();

        // ObjectId of Table, then List of row, column, value of old table cells
        private readonly List<Tuple<ObjectId, List<Tuple<int, int, string>>>> tables = new();

        // ObjectId of MText, value of old Content
        private readonly Dictionary<ObjectId, string> mText = new();

        // ObjectId of DBText, value of old Text
        private readonly Dictionary<ObjectId, string> dBText = new();

        // ObjectId of Block, AttributesWithId Dictionary
        private readonly Dictionary<ObjectId, Dictionary<string, Tuple<Attrib, ObjectId?>>> blockAttributes = new();

        // ObjectId of Block References
        private readonly ObjectIdCollection blockReferences = new();

        // ObjectId collection of inserted circuit entities
        private readonly ObjectIdCollection blockIds = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the deviation factor acceptable for a circuit drawing.
        /// </summary>
        public static double Tolerance { get; set; } = 0.025;

        /// <summary>
        /// Gets or sets the AutoCAD Electrical circuit block reference.
        /// </summary>
        public BlockReference BlockReference { get; }

        /// <summary>
        /// Gets or sets the replacement keys for the AutoCAD Electrical circuit.
        /// </summary>
        public Dictionary<string, string> AttributeKeys { get; set; } = null;

        /// <summary>
        /// Gets or sets the replacement keys for an AutoCAD Electrical block reference with a specific Tag key.
        /// </summary>
        public Dictionary<string, Dictionary<string, Attrib>> BlockKeys { get; set; } = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="transaction">The transaction to perform operations.</param>
        /// <param name="blockId">The block table record circuit.</param>
        public AeCircuit(Transaction transaction, ObjectId blockId)
        {
            this.transaction = transaction;

            blockId.Validate(Sort.rxBlockReference, true);
            using BlockTableRecord block = transaction.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
            BlockReference = new BlockReference(Point3d.Origin, blockId);

            Database database = blockId.Database;

            //Sort the circuit data
            foreach (ObjectId objectId in block)
            {
                if (objectId.ObjectClass == Sort.rxLine)
                {
                    using Line line = transaction.GetObject(objectId, OpenMode.ForRead) as Line;
                    Dictionary<string, Attrib> attributes = line.GetAttributes(transaction);

                    if (!attributes.ContainsKey("WNPTR"))
                        continue;

                    ObjectId wireNoId = new();
                    try { wireNoId = database.GetObjectId(false, new Handle(Convert.ToInt64(attributes["WNPTR"].Text, 16)), 0); }
                    catch { }

                    if (wireNoId == ObjectId.Null)
                        continue;

                    using BlockReference reference = transaction.GetObject(wireNoId, OpenMode.ForRead) as BlockReference;
                    var attributeIds = reference.GetAttributesWithIds(transaction);

                    if (!(attributeIds.ContainsKey("WIRENO") && attributeIds["WIRENO"].Item2.HasValue))
                        continue;

                    wires.Add(objectId, attributeIds["WIRENO"].Item2.Value);
                }
                else if (objectId.ObjectClass == Sort.rxBlockReference)
                {
                    using BlockReference blockReference = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;

                    Dictionary<string, Tuple<Attrib, ObjectId?>> attributeIds = blockReference.GetAttributesWithIds(transaction);

                    blockAttributes.Add(objectId, attributeIds);
                    blockReferences.Add(objectId);

                    if (attributeIds.ContainsKey("LINKTERM"))
                        linkTermCollection.Add(objectId);

                    if (wireNo.IsMatch(blockReference.Name) && attributeIds.ContainsKey("WIRENO") && attributeIds["WIRENO"].Item2.HasValue)
                        wireNoAttributes.Add(attributeIds["WIRENO"].Item2.Value, attributeIds["WIRENO"].Item1.Text);

                    foreach (var item in attributeIds)
                    {
                        string attributeTag = item.Key;
                        if ((terminal.IsMatch(attributeTag) || attributeTag.Equals("WIRENO")) && item.Value.Item2.HasValue)
                            terminalAttributes.Add(item.Value.Item2.Value);
                    }
                }
                else if (objectId.ObjectClass == Sort.rxDBText)
                {
                    using DBText text = transaction.GetObject(objectId, OpenMode.ForRead) as DBText;
                    dBText.Add(objectId, text.TextString);
                }
                else if (objectId.ObjectClass == Sort.rxMText)
                {
                    using MText text = transaction.GetObject(objectId, OpenMode.ForRead) as MText;
                    mText.Add(objectId, text.Contents);
                }
                else if(objectId.ObjectClass == Sort.rxTable)
                {
                    using Table table = transaction.GetObject(objectId, OpenMode.ForRead) as Table;
                    List<Tuple<int, int, string>> cells = new();

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            Cell cell = table.Cells[i, j];
                            
                            //Catch exception from merged cells
                            try
                            {
                                string value = cell.Contents[0].TextString;
                                cells.Add(new(i, j, value));
                            }
                            catch (System.ArgumentOutOfRangeException) { }
                        }
                    }

                    tables.Add(new(objectId, cells));
                }
            }

            // Go through the terminal attributes
            // Try and find connections to wireno blocks
            foreach (ObjectId id in terminalAttributes)
            {
                ObjectId WireNoId = FindWireNoAttribute(id);

                if (WireNoId.IsNull)
                    continue;

                if (!circuitNetwork.ContainsKey(WireNoId))
                    circuitNetwork[WireNoId] = new ObjectIdCollection();

                circuitNetwork[WireNoId].Add(id);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prompt the user to insert the circuit.
        /// </summary>
        /// <returns>A collection of ObjectIds of the exploded inserted circuit.</returns>
        public ObjectIdCollection ManualInsertCircuit(IList<AeLadder> aeLadders)
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            Editor editor = document.Editor;

            BlockReference.Position = Point3d.Origin;
            CircuitJig jig = new(this);
            
            UpdateCircuitKeys();

            PromptResult result = editor.Drag(jig);

            if (result.Status != PromptStatus.OK)
                return new ObjectIdCollection();

            UpdateWireNo(document.Database.CurrentSpaceId, aeLadders);
            UpdateLinkTerms();

            using BlockTableRecord space = transaction.GetObject(document.Database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
            
            space.AppendEntity(BlockReference);
            transaction.AddNewlyCreatedDBObject(BlockReference, true);

            blockIds.Clear();
            document.Database.ObjectAppended += Database_ObjectAppended;

            try { BlockReference.ExplodeToOwnerSpace(); }
            catch { }

            document.Database.ObjectAppended -= Database_ObjectAppended;

            BlockReference.UpgradeOpen();
            BlockReference.Erase();
            BlockReference.Dispose();

            return blockIds;
        }

        /// <summary>
        /// Insert the circuit at the specified point.
        /// </summary>
        /// <param name="spaceId">The space to insert the circuit.</param>
        /// <param name="point">The point to insert the circuit.</param>
        /// <param name="aeLadders">The list of wire number ladders in the drawing.</param>
        /// <returns></returns>
        public ObjectIdCollection AutoInsertCircuit(ObjectId spaceId, Point3d point, IList<AeLadder> aeLadders)
        {
            if (!spaceId.Validate(false) || !transaction.Validate(false,false))
                return new ObjectIdCollection();

            using BlockTableRecord space = transaction.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;

            if (space == null)
                return new ObjectIdCollection();

            Database database = spaceId.Database;

            BlockReference.Position = point;

            UpdateCircuitKeys();
            UpdateLinkTerms();
            UpdateWireNo(spaceId, aeLadders);

            space.AppendEntity(BlockReference);
            transaction.AddNewlyCreatedDBObject(BlockReference, true);

            blockIds.Clear();
            database.ObjectAppended += Database_ObjectAppended;
            
            try { BlockReference.ExplodeToOwnerSpace(); }
            catch { }

            database.ObjectAppended -= Database_ObjectAppended;

            BlockReference.UpgradeOpen();
            BlockReference.Erase();
            BlockReference.Dispose();

            return blockIds;
        }

        /// <summary>
        /// Update all of the wire numbers in the circuit.
        /// </summary>
        /// <param name="space">The space to get wire number ladders from.</param>
        /// <param name="aeLadders">The list of wire number ladders.</param>
        public void UpdateWireNo(ObjectId space, IList<AeLadder> aeLadders)
        {
            if (!space.Validate(false))
                return;

            Point3d point = BlockReference.Position;

            foreach (KeyValuePair<ObjectId,string> item in wireNoAttributes)
            {
                using AttributeReference attribute = transaction.GetObject(item.Key, OpenMode.ForRead) as AttributeReference;
                
                if (!item.Value.Contains("?"))
                    continue;

                AeLadder aeLadder = null;
                double? priority = null;
                double? maxPriority = null;
				foreach (AeLadder ladder in aeLadders)
				{
                    ladder.Refresh();
                    priority = ladder.GetPriority(point.Add(attribute.Position.GetAsVector()));
                    if (priority.HasValue)
					{
                        if (maxPriority == null
                         || maxPriority > priority)
                        {
                            maxPriority = priority;
                            aeLadder = ladder;
                        }
                    }
				}

                if (aeLadder == null)
                    continue;

                string wireno = aeLadder.GetWireNumber(point.Add(attribute.Position.GetAsVector())) + item.Value.Replace("?", "");
                attribute.TextString = wireno;
                AlignText(attribute);

                if (!circuitNetwork.ContainsKey(item.Key))
                    continue;

                foreach (ObjectId id in circuitNetwork[item.Key])
                {
                    using AttributeReference circuitAttribute = transaction.GetObject(id, OpenMode.ForWrite) as AttributeReference;
                    circuitAttribute.TextString = wireno;
                    AlignText(circuitAttribute);
                }
            }
        }

        /// <summary>
        /// Update the circuit values.
        /// </summary>
        public void UpdateCircuitKeys()
        {
            if (AttributeKeys == null || BlockKeys == null)
                return;

            foreach (var item in tables)
            {
                using Table table = transaction.GetObject(item.Item1, OpenMode.ForWrite) as Table;

                foreach (var tuple in item.Item2)
                {
                    Cell cell = table.Cells[tuple.Item1,tuple.Item2];

                    //If the cell is locked, unlock it.
                    if (cell.IsContentEditable.Value == false)
                        cell.State = CellStates.ContentModifiedAfterUpdate;

                    cell.Value = FindAndReplace(tuple.Item3, AttributeKeys);
                }
            }

            foreach (KeyValuePair<ObjectId, string> item in mText)
            {
                using MText mtext = transaction.GetObject(item.Key, OpenMode.ForRead) as MText;
                string value = FindAndReplace(item.Value, AttributeKeys);

                if (value == mtext.Contents)
                    continue;

                mtext.UpgradeOpen();
                mtext.Contents = value;
            }

            foreach (KeyValuePair<ObjectId,string> item in dBText)
            {
                using DBText text = transaction.GetObject(item.Key, OpenMode.ForRead) as DBText;
                string value = FindAndReplace(item.Value, AttributeKeys);

                if (value == text.TextString)
                    continue;

                text.UpgradeOpen();
                text.TextString = value;
                AlignText(text);
            }

            foreach (var item in blockAttributes)
            {
                using BlockReference blockReference = transaction.GetObject(item.Key, OpenMode.ForRead) as BlockReference;
                Dictionary<string, Tuple<Attrib, ObjectId?>> attributes = item.Value;
                Dictionary<string, Attrib> write = new();

                foreach (var attribute in attributes)
                {
                    write[attribute.Key] = attribute.Value.Item1;
                }

                string[] tags = write.Keys.ToArray();

                foreach (string tag in tags)
                {
                    string value = write[tag].Text;

                    value = FindAndReplace(value, AttributeKeys);

                    write[tag].Text = value;
                }

                foreach (string tag in tags)
                {
                    if (!(blockTag.IsMatch(tag) || sigCode.IsMatch(tag)))
                        continue;

                    string value = write[tag].Text;

                    if (!BlockKeys.ContainsKey(value))
                        continue;

                    foreach (var kvp in BlockKeys[value])
                    {
                        write[kvp.Key] = kvp.Value;
                    }

                    if (BlockKeys[value].ContainsKey("BASETAG"))
                        write[tag] = BlockKeys[value]["BASETAG"];
                }

                blockReference.SetAttributes(transaction, write);
            }
        }

        /// <summary>
        /// Update all of the terminals with fresh linkterm values.
        /// </summary>
        public void UpdateLinkTerms()
        {
            Dictionary<string, Attrib> attributes = new();
            foreach (ObjectId id in linkTermCollection)
            {
                using BlockReference blockReference = transaction.GetObject(id, OpenMode.ForWrite) as BlockReference;
                attributes["LINKTERM"] = new(1000, Guid.NewGuid().ToString().ToUpper());
                blockReference.SetAttributes(transaction, attributes);
            }
        }

        private string FindAndReplace(string value, Dictionary<string, string> keys)
        {
            foreach (KeyValuePair<string, string> item in keys)
            {
                value = value.Replace(item.Key, item.Value);
            }
            return value;
        }

        private void AlignText(DBText text)
        {
            Database database = text.Database;
            using WorkingDatabaseSwitcher switcher = new(database);
            text.AdjustAlignment(database);
        }

        private ObjectId FindWireNoAttribute(ObjectId attributeId)
        {
            using AttributeReference attribute = transaction.GetObject(attributeId, OpenMode.ForRead) as AttributeReference;
            
            if (attribute.Tag.Equals("WIRENO"))
            {
                using BlockReference blockReference = transaction.GetObject(attribute.OwnerId, OpenMode.ForRead) as BlockReference;

                foreach (var item in blockReference.GetAttributesWithIds(transaction))
                {
                    if (!(terminal.IsMatch(item.Key) && item.Value.Item2.HasValue))
                        continue;

                    ObjectId id = FindWireNoAttribute(item.Value.Item2.Value);
                    
                    if (!id.IsNull)
                        return id;
                }
            }
            else
            {
                foreach (var wire in wires)
                {
                    using Line line = transaction.GetObject(wire.Key, OpenMode.ForRead) as Line;

                    if (line.StartPoint.DistanceTo(attribute.Position) < Tolerance
                     || line.EndPoint.DistanceTo(attribute.Position) < Tolerance)
                    {
                        return wire.Value;
                    }
                }
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Release all of the resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (!BlockReference.IsDisposed && !BlockReference.IsErased)
                BlockReference.Dispose();
        }

		#endregion

		#region Static Methods

		/// <summary>
		/// This method reads in a .dwg file, inserts it into the drawing, and automatically fills in all attributes.
		/// </summary>
		/// <param name="database">The drawing database to insert circuit.</param>
		/// <param name="transaction">The database transaction to perform operations</param>
		/// <param name="fileName">The filename of the block to insert.</param>
		/// <param name="blockSpaceId">The space to insert the block</param>
		/// <param name="insertionPoint">The insertion point to use for insertion.</param>
		/// <param name="ladders">The wire diagram ladders.</param>
		/// <param name="attributeKeys">The replacement keys.</param>
		/// <param name="blockKeys">The replacement block attributes.</param>
		public static void Insert(Database database,
            Transaction transaction,
            string fileName,
            ObjectId blockSpaceId,
            Point3d? insertionPoint,
            IList<AeLadder> ladders,
            Dictionary<string, string> attributeKeys,
            Dictionary<string, Dictionary<string, Attrib>> blockKeys)
        {
            if ((database == null) || !insertionPoint.HasValue)
                database = Application.DocumentManager.MdiActiveDocument.Database;

            using BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

            ObjectIdCollection newObjectsIds = new();

            using LayerTable layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);
            database.Clayer = layerTable["0"];

            //If fileName does exist...
            //If fileName does not exist, report the error and return.
            if (File.Exists(fileName))
            {
                //Create a database for the new block from other drawing.
                using Database dbInsert = new(false, true);

                // Read in the DWG, store it into the main database
                dbInsert.ReadDwgFile(fileName, System.IO.FileShare.ReadWrite, true, "");
                string blockName = Path.GetFileNameWithoutExtension(fileName);
                ObjectId blockId = database.Insert(blockName, dbInsert, false);
                using AeCircuit circuit = new(transaction, blockId)
                {
                    AttributeKeys = attributeKeys,
                    BlockKeys = blockKeys,
                };

                if (insertionPoint.HasValue)
                    newObjectsIds = circuit.AutoInsertCircuit(blockSpaceId, insertionPoint.Value, ladders);
                else
                    newObjectsIds = circuit.ManualInsertCircuit(ladders);

                using BlockTableRecord block = transaction.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
                block.Erase();
            }
            else
            {
                if (fileName == null)
                    MessageBox.Show($"File not found:\nnull");
                else
                    MessageBox.Show($"File not found:\n{ fileName }");

                return;
            }
        }
        #endregion

        #region Delegates, Events, Handlers

        private void Database_ObjectAppended(object sender, ObjectEventArgs e)
        {
            blockIds.Add(e.DBObject.ObjectId);
        }

        #endregion
    }
}
