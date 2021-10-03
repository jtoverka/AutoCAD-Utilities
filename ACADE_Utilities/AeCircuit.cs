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
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

		// Find and replace sub blocks if key $BLOCKREPLACE$ is found
		private readonly HashSet<ObjectId> blocks = new();

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
		/// <param name="blockId">The block table record circuit.</param>
		public AeCircuit(ObjectId blockId)
		{
			using AeXData aeXData = new();
			blockId.Validate(RXObject.GetClass(typeof(BlockTableRecord)), true);

			Database database = blockId.Database;

			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			using BlockTableRecord block = transaction.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
			BlockReference = new BlockReference(Point3d.Origin, blockId);

			//Sort the circuit data
			foreach (ObjectId objectId in block)
			{
				if (objectId.ObjectClass == Sort.rxLine)
				{
					using Line line = transaction.GetObject(objectId, OpenMode.ForRead) as Line;
					Dictionary<string, Attrib> attributes = line.GetAttributes();

					if (!attributes.ContainsKey("WNPTR"))
						continue;

					ObjectId wireNoId = new();
					try { wireNoId = database.GetObjectId(false, new Handle(Convert.ToInt64(attributes["WNPTR"].Text, 16)), 0); }
					catch { }

					if (wireNoId == ObjectId.Null)
						continue;

					using BlockReference reference = transaction.GetObject(wireNoId, OpenMode.ForRead) as BlockReference;
					var attributeIds = reference.GetAttributesWithIds();

					if (!(attributeIds.ContainsKey("WIRENO") && attributeIds["WIRENO"].Item2.HasValue))
						continue;

					wires.Add(objectId, attributeIds["WIRENO"].Item2.Value);
				}
				else if (objectId.ObjectClass == Sort.rxBlockReference)
				{
					using BlockReference blockReference = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;

					// Add sub blocks for later use.
					if (!blocks.Contains(blockReference.BlockTableRecord))
						blocks.Add(blockReference.BlockTableRecord);

					Dictionary<string, Tuple<Attrib, ObjectId?>> attributeIds = blockReference.GetAttributesWithIds();

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
		/// <param name="aeLadders">The ladder blocks.</param>
		/// <returns>A collection of ObjectIds of the exploded inserted circuit.</returns>
		public ObjectIdCollection ManualInsertCircuit(IList<AeLadder> aeLadders)
		{
			Document document = Active.Document;
			Editor editor = Active.Editor;

			bool started = document.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			BlockReference.Position = Point3d.Origin;
			CircuitJig jig = new(this);
			
			UpdateCircuitKeys(transaction);

			PromptResult result = editor.Drag(jig);

			if (result.Status != PromptStatus.OK)
				return new ObjectIdCollection();

			UpdateWireNo(transaction, document.Database.CurrentSpaceId, aeLadders);
			UpdateLinkTerms(transaction);

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
			blockIds.Clear();

			if (!spaceId.Validate(false))
				return blockIds;

			Database database = spaceId.Database;

			if (!database.Validate(false, false))
				return blockIds;

			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			using BlockTableRecord space = transaction.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;

			BlockReference.Position = point;

			UpdateCircuitKeys(transaction);
			UpdateLinkTerms(transaction);
			UpdateWireNo(transaction, spaceId, aeLadders);

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
		/// <param name="transaction">The database transaction to perform operations within.</param>
		/// <param name="spaceId">The space to get wire number ladders from.</param>
		/// <param name="aeLadders">The list of wire number ladders.</param>
		public void UpdateWireNo(Transaction transaction, ObjectId spaceId, IList<AeLadder> aeLadders)
		{
			if (!spaceId.Validate(false))
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
		/// <param name="transaction">The database transaction to perform operations within.</param>
		public void UpdateCircuitKeys(Transaction transaction)
		{
			using AeXData aeXData = new();
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

				blockReference.SetAttributes(write);
			}

			// Find and replace withiin blocks if BLOCKREPLACE attribute key is found
			if (AttributeKeys.ContainsKey("$BLOCKREPLACE$") && AttributeKeys["$BLOCKREPLACE$"].Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				foreach (ObjectId blockId in blocks)
				{
					if (!blockId.Validate(false))
						continue;

					AeCircuit circuit = new(blockId);

					// Remove blocks already processed.
					foreach (ObjectId objectId in blocks)
					{
						if (circuit.blocks.Contains(objectId))
							circuit.blocks.Remove(objectId);
					}

					circuit.AttributeKeys = AttributeKeys;
					circuit.BlockKeys = BlockKeys;
					circuit.UpdateCircuitKeys(transaction);
				}
			}
		}

		/// <summary>
		/// Update all of the terminals with fresh linkterm values.
		/// </summary>
		/// <param name="transaction">The database transaction to perform operations within.</param>
		public void UpdateLinkTerms(Transaction transaction)
		{
			using AeXData aeXData = new();
			Dictionary<string, Attrib> attributes = new();
			foreach (ObjectId id in linkTermCollection)
			{
				using BlockReference blockReference = transaction.GetObject(id, OpenMode.ForWrite) as BlockReference;
				attributes["LINKTERM"] = new(1000, Guid.NewGuid().ToString().ToUpper());
				blockReference.SetAttributes(attributes);
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

		/// <summary>
		/// Find The WIRENO attribute in a block.
		/// </summary>
		/// <param name="attributeId">The attribute in a block with a WIRENO attribute.</param>
		/// <returns></returns>
		private ObjectId FindWireNoAttribute(ObjectId attributeId)
		{
			using AeXData aeXData = new();
			Database database = attributeId.Database;

			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			using AttributeReference attribute = transaction.GetObject(attributeId, OpenMode.ForRead) as AttributeReference;
			
			if (attribute.Tag.Equals("WIRENO"))
			{
				using BlockReference blockReference = transaction.GetObject(attribute.OwnerId, OpenMode.ForRead) as BlockReference;

				foreach (var item in blockReference.GetAttributesWithIds())
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

		#region Delegates, Events, Handlers

		private void Database_ObjectAppended(object sender, ObjectEventArgs e)
		{
			blockIds.Add(e.DBObject.ObjectId);
		}

		#endregion
	}
}
