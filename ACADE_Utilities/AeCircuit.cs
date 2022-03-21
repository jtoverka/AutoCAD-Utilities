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
using System.ComponentModel;
using System.IO;
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

		// The block definition of the block reference.
		private readonly ObjectId blockId;

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

		// ObjectId of Dimensions, value of old text
		private readonly Dictionary<ObjectId, string> dimText = new();

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
		public double Tolerance { get; set; } = 0.025;

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

		/// <summary>
		/// Gets or sets the flag to find and replace table cells.
		/// </summary>
		[DefaultValue(true)]
		public bool FindAndReplaceTable { get; set; } = true;

		/// <summary>
		/// Gets or sets the flag to find and replace MText.
		/// </summary>
		[DefaultValue(true)]
		public bool FindAndReplaceMText { get; set; } = true;

		/// <summary>
		/// Gets or sets the flag to find and replace text.
		/// </summary>
		[DefaultValue(true)]
		public bool FindAndReplaceText { get; set; } = true;

		/// <summary>
		/// Gets or sets the flag to find and replace attributes.
		/// </summary>
		[DefaultValue(true)]
		public bool FindAndReplaceAttributes { get; set; } = true;

		/// <summary>
		/// Gets or sets the flag to find and replace Dimension text.
		/// </summary>
		[DefaultValue(true)]
		public bool FindAndReplaceDimensionText { get; set; } = true;

		/// <summary>
		/// Gets or sets the flag to find and replace Xref blocks.
		/// </summary>
		[DefaultValue(true)]
		public bool FindAndReplaceXrefBlocks { get; set; } = true;

		/// <summary>
		/// Gets or sets the flag to find and replace within block definitions.
		/// </summary>
		[DefaultValue(true)]
		public bool FindAndReplaceWithinBlocks { get; set; } = true;

		/// <summary>
		/// Gets or sets the flag to find and replace dynamic block properties.
		/// </summary>
		[DefaultValue(true)]
		public bool FindAndReplaceDynamicProperties { get; set; } = true;

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
			this.blockId = blockId;

			BlockReference = new BlockReference(Point3d.Origin, blockId);

			UpdateCircuitSort(this);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sorts the entities in the block.
		/// </summary>
		/// <param name="circuit">The circuit with the block to sort.</param>
		private void UpdateCircuitSort(AeCircuit circuit)
		{
			Database database = circuit.blockId.Database;

			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			using BlockTableRecord block = transaction.GetObject(circuit.blockId, OpenMode.ForRead) as BlockTableRecord;

			circuit.wires.Clear();
			circuit.blocks.Clear();
			circuit.blockAttributes.Clear();
			circuit.blockReferences.Clear();
			circuit.linkTermCollection.Clear();
			circuit.wireNoAttributes.Clear();
			circuit.terminalAttributes.Clear();
			circuit.dBText.Clear();
			circuit.dimText.Clear();
			circuit.mText.Clear();
			circuit.tables.Clear();
			circuit.circuitNetwork.Clear();

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

					circuit.wires.Add(objectId, attributeIds["WIRENO"].Item2.Value);
				}
				else if (objectId.ObjectClass == Sort.rxBlockReference)
				{
					using BlockReference blockReference = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;

					// Add sub blocks for later use.
					if (!circuit.blocks.Contains(blockReference.BlockTableRecord))
						circuit.blocks.Add(blockReference.BlockTableRecord);

					Dictionary<string, Tuple<Attrib, ObjectId?>> attributeIds = blockReference.GetAttributesWithIds();

					circuit.blockAttributes.Add(objectId, attributeIds);
					circuit.blockReferences.Add(objectId);

					if (attributeIds.ContainsKey("LINKTERM"))
						circuit.linkTermCollection.Add(objectId);

					if (wireNo.IsMatch(blockReference.Name) && attributeIds.ContainsKey("WIRENO") && attributeIds["WIRENO"].Item2.HasValue)
						circuit.wireNoAttributes.Add(attributeIds["WIRENO"].Item2.Value, attributeIds["WIRENO"].Item1.Text);

					foreach (var item in attributeIds)
					{
						string attributeTag = item.Key;
						if ((terminal.IsMatch(attributeTag) || attributeTag.Equals("WIRENO")) && item.Value.Item2.HasValue)
							circuit.terminalAttributes.Add(item.Value.Item2.Value);
					}
				}
				else if (objectId.ObjectClass == Sort.rxDBText)
				{
					using DBText text = transaction.GetObject(objectId, OpenMode.ForRead) as DBText;
					circuit.dBText.Add(objectId, text.TextString);
				}
				else if (objectId.ObjectClass == Sort.rxAlignedDimension
					  || objectId.ObjectClass == Sort.rxArcDimension
					  || objectId.ObjectClass == Sort.rxDiametricDimension
					  || objectId.ObjectClass == Sort.rxLineAngularDimension2
					  || objectId.ObjectClass == Sort.rxPoint3AngularDimension
					  || objectId.ObjectClass == Sort.rxRadialDimension
					  || objectId.ObjectClass == Sort.rxRadialDimensionLarge
					  || objectId.ObjectClass == Sort.rxRotatedDimension)
				{
					using Dimension dimension = transaction.GetObject(objectId, OpenMode.ForRead) as Dimension;
					circuit.dimText.Add(objectId, dimension.DimensionText);
				}
				else if (objectId.ObjectClass == Sort.rxMText)
				{
					using MText text = transaction.GetObject(objectId, OpenMode.ForRead) as MText;
					circuit.mText.Add(objectId, text.Contents);
				}
				else if (objectId.ObjectClass == Sort.rxTable)
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

					circuit.tables.Add(new(objectId, cells));
				}
			}

			// Go through the terminal attributes
			// Try and find connections to wireno blocks
			foreach (ObjectId id in circuit.terminalAttributes)
			{
				ObjectId WireNoId = FindWireNoAttribute(id);

				if (WireNoId.IsNull)
					continue;

				if (!circuit.circuitNetwork.ContainsKey(WireNoId))
					circuit.circuitNetwork[WireNoId] = new ObjectIdCollection();

				circuit.circuitNetwork[WireNoId].Add(id);
			}
		}

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
			using Disposable disposable = new(() => { transaction.Finish(); }, started);

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

			using BlockTableRecord blockTableRecord = transaction.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
			blockTableRecord.Erase();

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
			using Disposable disposable = new(()=>{ transaction.Finish(); }, started);

			using BlockTableRecord space = transaction.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;

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

			using BlockTableRecord blockTableRecord = transaction.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
			blockTableRecord.Erase();

			return blockIds;
		}

		/// <summary>
		/// Update all of the wire numbers in the circuit.
		/// </summary>
		/// <param name="spaceId">The space to get wire number ladders from.</param>
		/// <param name="aeLadders">The list of wire number ladders.</param>
		public void UpdateWireNo(ObjectId spaceId, IList<AeLadder> aeLadders)
		{
			if (!spaceId.Validate(false))
				return;

			bool started = blockId.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(() => { transaction.Commit(); transaction.Dispose(); }, started);

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
		/// <remarks>
		/// $XREF$ - Filepath to xref
		/// $ROTATE$ - Rotation of xref
		/// </remarks>
		public void UpdateCircuitKeys()
		{
			bool started = blockId.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(() => { transaction.Finish(); }, started);

			using AeXData aeXData = new();
			if (AttributeKeys == null || BlockKeys == null)
				return;

			if (FindAndReplaceTable)
			{
				foreach (var item in tables)
				{
					using Table table = transaction.GetObject(item.Item1, OpenMode.ForWrite) as Table;

					foreach (var tuple in item.Item2)
					{
						Cell cell = table.Cells[tuple.Item1, tuple.Item2];

						//If the cell is locked, unlock it.
						if (cell.IsContentEditable.Value == false)
							cell.State = CellStates.ContentModifiedAfterUpdate;

						cell.Value = FindAndReplace(tuple.Item3, AttributeKeys);
					}
				}
			}

			if (FindAndReplaceMText)
			{
				foreach (KeyValuePair<ObjectId, string> item in mText)
				{
					using MText mtext = transaction.GetObject(item.Key, OpenMode.ForRead) as MText;
					string value = FindAndReplace(item.Value, AttributeKeys);

					if (value == mtext.Contents)
						continue;

					mtext.UpgradeOpen();
					mtext.Contents = value;
				}
			}

			if (FindAndReplaceText)
			{
				foreach (KeyValuePair<ObjectId, string> item in dBText)
				{
					using DBText text = transaction.GetObject(item.Key, OpenMode.ForRead) as DBText;
					string value = FindAndReplace(item.Value, AttributeKeys);

					if (value == text.TextString)
						continue;

					text.UpgradeOpen();
					text.TextString = value;
					AlignText(text);
				}
			}

			if (FindAndReplaceDimensionText)
			{
				foreach (KeyValuePair<ObjectId, string> item in dimText)
				{
					using Dimension dimension = transaction.GetObject(item.Key, OpenMode.ForRead) as Dimension;
					string value = FindAndReplace(item.Value, AttributeKeys);

					if (value == dimension.DimensionText)
						continue;

					dimension.UpgradeOpen();
					dimension.DimensionText = value;
				}
			}

			if (FindAndReplaceAttributes)
			{
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
			}

			if (FindAndReplaceWithinBlocks)
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
					circuit.UpdateCircuitKeys();
				}
			}

			if (FindAndReplaceXrefBlocks)
			{
				AttributeKeys.TryGetValue("$XREF$", out string filepath);
				AttributeKeys.TryGetValue("$ROTATE$", out string rotation);
				rotation ??= "0";
				filepath ??= "";

				double rotate = 0;
				if (rotation.Equals("90"))
					rotate = 1.57079633;
				else if (rotation.Equals("180"))
					rotate = 3.14159265;
				else if (rotation.Equals("270"))
					rotate = 4.71238898;

				if (Path.GetExtension(filepath).Equals(string.Empty))
					filepath += ".dwg";

				string name = "XREF-" + Path.GetFileNameWithoutExtension(filepath);

				blockId.Database.XrefEditEnabled = true;

				using BlockTable blockTable = transaction.GetObject(blockId.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

				foreach (ObjectId objectId in blockReferences)
				{
					using BlockReference blockReference = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;
					using BlockTableRecord record = transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

					if (record.IsFromExternalReference)
					{
						blockReference.UpgradeOpen();
						blockReference.Rotation = rotate;

						if (!blockTable.Has(name))
						{
							record.UpgradeOpen();
							record.PathName = filepath;
							record.Name = name;
						}
						else
						{
							using BlockTableRecord existing = transaction.GetObject(blockTable[name], OpenMode.ForRead) as BlockTableRecord;
							if (existing.IsFromExternalReference)
							{
								using BlockReference reference = new(blockReference.Position, blockTable[name]);
								reference.Rotation = rotate;
								blockReference.Erase();

								using BlockTableRecord block = transaction.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
								block.AppendEntity(reference);
								transaction.AddNewlyCreatedDBObject(reference, true);
							}
						}
					}
				}
			}

			if (FindAndReplaceDynamicProperties)
			{
				using BlockTable blockTable = transaction.GetObject(blockId.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

				foreach (ObjectId objectId in blockReferences)
				{
					using BlockReference blockReference = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;

					if (blockReference.IsDynamicBlock)
					{
						blockReference.UpgradeOpen();

						using DynamicBlockReferencePropertyCollection properties = blockReference.DynamicBlockReferencePropertyCollection;
						for (int i = 0; i < properties.Count; i++)
						{
							DynamicBlockReferenceProperty property = properties[i];

							try
							{
								if (!property.ReadOnly && AttributeKeys.ContainsKey(property.PropertyName))
								{
									DynamicBlockReferencePropertyUnitsType type = property.UnitsType;
									if (type == DynamicBlockReferencePropertyUnitsType.NoUnits)
									{
										property.Value = AttributeKeys[property.PropertyName];
									}
									else
									{
										property.Value = double.Parse(AttributeKeys[property.PropertyName]);
									}
								}
							}
							catch (System.Exception ex) { _ = ex; }
						}
					}
				}
			}
		}

		/// <summary>
		/// Update all of the terminals with fresh linkterm values.
		/// </summary>
		public void UpdateLinkTerms()
		{
			bool started = blockId.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(() => { transaction.Commit(); transaction.Dispose(); }, started);

			using AeXData aeXData = new();
			Dictionary<string, Attrib> attributes = new();
			foreach (ObjectId id in linkTermCollection)
			{
				using BlockReference blockReference = transaction.GetObject(id, OpenMode.ForWrite) as BlockReference;
				attributes["LINKTERM"] = new(1000, Guid.NewGuid().ToString().ToUpper());
				blockReference.SetAttributes(attributes);
			}
		}

		/// <summary>
		/// Find and replace block markers with another circuit.
		/// </summary>
		/// <param name="block">The block name to be used as a marker.</param>
		/// <param name="id">The ID attribute to match.</param>
		/// <param name="circuit">The circuit to put in place of the block marker.</param>
		public void InsertSubCircuit(string block, string id, AeCircuit circuit)
		{
			bool started = blockId.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(() => { transaction.Finish(); }, started);

			foreach (ObjectId objectId in blockReferences)
			{
				using BlockReference blockReference = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;

				// Continue with next block reference if match fails
				if (!BlockReference.GetEffectiveName().Equals(block, StringComparison.OrdinalIgnoreCase))
					continue;

				Dictionary<string, Attrib> attributes = blockReference.GetAttributes();

				// Continue with next block reference if match fails
				if (!(attributes.ContainsKey("ID") && attributes["ID"].Text.Equals(id, StringComparison.OrdinalIgnoreCase)))
					continue;

				Point3d insertionPoint = blockReference.Position;

				// Remove block marker
				blockReference.UpgradeOpen();
				blockReference.Erase();

				// Add new circuit in place of marker
				using BlockTableRecord blockTableRecord = transaction.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
				using BlockReference insertCircuit = new(insertionPoint, circuit.blockId);
				blockTableRecord.AppendEntity(insertCircuit);
			}

			UpdateCircuitSort(this);
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
