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

using AcDbWrapper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;

namespace AceWrapper
{
	/// <summary>
	/// Represents a container for AutoCAD Electrical drawings.
	/// </summary>
	public class AeDrawing
	{
		#region Static fields

		private static readonly Dictionary<Database, AeDrawing> drawingDatabases = new();

		#endregion

		#region Fields

		private readonly HashSet<ObjectId> lineIds = new();
		private readonly HashSet<ObjectId> blockReferenceIds = new();
		private readonly Dictionary<ObjectId, Attributes> blockAttributes = new();
		private readonly Database database = null;
		private readonly Dictionary<AeObject, HashSet<ObjectId>> aeSortField = new();
		private readonly Dictionary<ObjectId, AeObject> aeReSortField = new();
		private readonly Sort sortField;
		private readonly IList<AeLadder> aeLadders = new List<AeLadder>();

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value associated with the specified key.
		/// </summary>
		/// <param name="index">The key of the value to get or set.</param>
		/// <returns>The value associated with the specified key. If the specified 
		/// key is not found, a get operation throws a <see cref="KeyNotFoundException"/>, and
		/// a set operation creates a new element with the specified key.</returns>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="ArgumentNullException"/>
		public HashSet<ObjectId> this[AeObject index]
		{
			get { return aeSortField[index]; }
			set { aeSortField[index] = value; }
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="class">The key of the value to get.</param>
		/// <returns>The value associated with the specified key. If the specified
		/// key is not found, a get operation throws a <see cref="KeyNotFoundException"/>.
		/// </returns>
		public HashSet<ObjectId> this[RXClass @class]
		{
			get { return sortField.Entities[@class]; }
		}

		/// <summary>
		/// Gets the list of Wire number ladders for this drawing.
		/// </summary>
		public IList<AeLadder> AeLadders
		{ 
			get { return aeLadders; }
		}

		/// <summary>
		/// Gets the database this object wraps.
		/// </summary>
		public Database Database
		{
			get { return database; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instances of this class.
		/// </summary>
		/// <param name="database">The drawing database.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ObjectDisposedException"/>
		private AeDrawing(Database database)
		{
			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			sortField = Sort.GetSort(database);
			this.database = database;

			sortField.ObjectAppended += SortField_ObjectAppended;
			sortField.ObjectErased += SortField_ObjectErased;
			sortField.ObjectReappended += SortField_ObjectReappended;
			sortField.ObjectUnappended += SortField_ObjectUnappended;
			sortField.ObjectModified += SortField_ObjectModified;

			foreach (AeObject value in Enum.GetValues(typeof(AeObject)))
			{
				aeSortField[value] = new();
			}

			if (sortField.ContainsKey(Sort.rxBlockReference))
				SortBlockReferences(sortField.Entities[Sort.rxBlockReference], transaction);

			if (sortField.ContainsKey(Sort.rxLine))
				SortLines(sortField.Entities[Sort.rxLine], transaction);
		}

		/// <summary>
		/// Initializes a new instances of this class.
		/// </summary>
		/// <param name="drawing">The drawing database.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="ObjectDisposedException"/>
		private AeDrawing(Sort drawing)
		{
			bool started = drawing.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			sortField = drawing;

			sortField.ObjectAppended += SortField_ObjectAppended;
			sortField.ObjectErased += SortField_ObjectErased;
			sortField.ObjectReappended += SortField_ObjectReappended;
			sortField.ObjectUnappended += SortField_ObjectUnappended;
			sortField.ObjectModified += SortField_ObjectModified;

			foreach (AeObject value in Enum.GetValues(typeof(AeObject)))
			{
				aeSortField[value] = new();
			}

			if (sortField.ContainsKey(Sort.rxBlockReference))
				SortBlockReferences(sortField.Entities[Sort.rxBlockReference], transaction);

			if (sortField.ContainsKey(Sort.rxLine))
				SortLines(sortField.Entities[Sort.rxLine], transaction);
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Get the AeDrawing equivalent to a drawing Database.
		/// </summary>
		/// <param name="database">The drawing database.</param>
		/// <returns>The AeDrawing.</returns>
		public static AeDrawing GetOrCreate(Database database)
		{
			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			database.Validate(true, true);

			if (!drawingDatabases.ContainsKey(database))
				drawingDatabases[database] = new(database);

			return drawingDatabases[database];
		}

		/// <summary>
		/// Get the AeDrawing equivalent to a drawing Database.
		/// </summary>
		/// <param name="drawing">The drawing database.</param>
		/// <returns>The AeDrawing.</returns>
		public static AeDrawing GetOrCreate(Sort drawing)
		{
			bool started = drawing.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			if (!drawingDatabases.ContainsKey(drawing.Database))
				drawingDatabases[drawing.Database] = new( drawing);

			return drawingDatabases[drawing.Database];
		}

		#endregion
		
		#region Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="class"></param>
		/// <returns></returns>
		public bool ContainsKey(RXClass @class)
		{
			return sortField.ContainsKey(@class);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aeObject"></param>
		/// <returns></returns>
		public bool ContainsKey(AeObject aeObject)
		{
			return aeSortField.ContainsKey(aeObject);
		}

		/// <summary>
		/// Insert circuit into drawing.
		/// </summary>
		/// <param name="circuit">The circuit to add.</param>
		/// <param name="spaceId">The space to add circuit.</param>
		/// <param name="point">The point in space to add circuit.</param>
		public void Insert(AeCircuit circuit, ObjectId spaceId, Point3d point)
		{
			bool started = spaceId.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(()=> { transaction.Finish(); }, started);

			Refresh();

			circuit.AutoInsertCircuit(spaceId, point, aeLadders);
		}

		/// <summary>
		/// Prompts the user to select a point to insert the circuit.
		/// </summary>
		/// <param name="circuit">The circuit to add.</param>
		public void Insert(AeCircuit circuit)
		{
			Refresh();

			circuit.ManualInsertCircuit(aeLadders);
		}

		/// <summary>
		/// Sorts a collection of block references.
		/// </summary>
		/// <param name="blockIds">The collection of block references to sort.</param>
		/// <param name="transaction">The database transaction used to perform operations.</param>
		private void SortBlockReferences(HashSet<ObjectId> blockIds, Transaction transaction)
		{
			if (!transaction.Validate(false, false))
				return;

			foreach (ObjectId blockId in blockIds)
			{
				if (!blockId.Validate(Sort.rxBlockReference, false))
					continue;

				if (!aeReSortField.ContainsKey(blockId))
					aeReSortField[blockId] = new();

				using BlockReference blockReference = transaction.GetObject(blockId, OpenMode.ForRead) as BlockReference;

				Attributes attributes = AeXData.Default();
                attributes.GetAttributes(blockId);

				blockAttributes[blockId] = attributes;

				if (blockReference.GetEffectiveName().Equals("WDDOT", StringComparison.OrdinalIgnoreCase))
				{
					aeSortField[AeObject.WDDOT].Add(blockId);
					aeReSortField[blockId] = AeObject.WDDOT;
				}
				else if (blockReference.GetEffectiveName().StartsWith("HT", StringComparison.OrdinalIgnoreCase))
				{
					aeSortField[AeObject.Terminal].Add(blockId);
					aeReSortField[blockId] = AeObject.Terminal;
				}
				else if (blockReference.GetEffectiveName().Equals("WD_WIRENO", StringComparison.OrdinalIgnoreCase))
				{
					aeSortField[AeObject.Wireno].Add(blockId);
					aeReSortField[blockId] = AeObject.Wireno;
				}
				else if (blockReference.GetEffectiveName().StartsWith("WD_MLR", StringComparison.OrdinalIgnoreCase))
				{
					aeSortField[AeObject.WD_MLR].Add(blockId);
					aeReSortField[blockId] = AeObject.WD_MLR;
				}
				else if (blockReference.GetEffectiveName().Equals("WD_M", StringComparison.OrdinalIgnoreCase))
				{
					aeSortField[AeObject.WD_M].Add(blockId);
					aeReSortField[blockId] = AeObject.WD_M;
				}
				else
				{
					aeSortField[AeObject.Block].Add(blockId);
					aeReSortField[blockId] = AeObject.Block;
				}
			}
		}

		/// <summary>
		/// Sorts a collection of lines
		/// </summary>
		/// <param name="lineIds">The collection of lines to sort.</param>
		/// <param name="transaction">The database transaction used to perform operations.</param>
		private void SortLines(HashSet<ObjectId> lineIds, Transaction transaction)
		{
			if (!transaction.Validate(false, false))
				return;

			ObjectId wdmId = ObjectId.Null;
			foreach (ObjectId id in aeSortField[AeObject.WD_M])
			{
				wdmId = id;
				if (wdmId.Validate(false))
					break;
			}

			if (wdmId.IsNull)
				return;

			if (!blockAttributes.ContainsKey(wdmId))
				return;

			Attributes attributes = AeXData.Default();
			attributes.GetAttributes(wdmId);

			if (attributes.ContainsKey("WIRE_LAYS"))
            {
                string[] layers = attributes["WIRE_LAYS"].Text.Split(',');
                Wildcard[] wildcards = new Wildcard[layers.Length];
                for (int i = 0; i < wildcards.Length; i++)
                {
                    wildcards[i] = new(layers[i].Trim());
                }

                foreach (ObjectId lineId in lineIds)
                {
                    if (!lineId.Validate(Sort.rxLine, false))
                        continue;

                    using Line line = transaction.GetObject(lineId, OpenMode.ForRead) as Line;

                    foreach (Wildcard wildcard in wildcards)
                    {
                        if (wildcard.IsMatch(line.Layer))
                        {
                            aeSortField[AeObject.Wire].Add(lineId);
                            break;
                        }
                    }
                }
            }
		}

		/// <summary>
		/// Searches the drawing for wire number ladders.
		/// </summary>
		public void Refresh()
		{
			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			SortLines(lineIds, transaction);
			lineIds.Clear();

			SortBlockReferences(blockReferenceIds, transaction);
			blockReferenceIds.Clear();

			aeLadders.Clear();

			HashSet<ObjectId> wdms = aeSortField[AeObject.WD_M];
			ObjectId wdmId = ObjectId.Null;
			foreach (ObjectId id in wdms)
			{
				if (id.Validate(false))
				{
					wdmId = id;
					break;
				}
			}

			if (wdmId == ObjectId.Null)
				return;

			HashSet<ObjectId> mlrs = aeSortField[AeObject.WD_MLR];
			foreach (ObjectId id in mlrs)
			{
				if (id.Validate(false))
				{
					AeLadder aeLadder = new(wdmId, id);
					aeLadders.Add(aeLadder);
				}
			}
		}

		/// <summary>
		/// Removes the ObjectId from the sortation record.
		/// </summary>
		/// <param name="id">The ObjectId to remove from the record.</param>
		private void RemoveObject(ObjectId id)
		{
			if (!aeReSortField.ContainsKey(id))
				return;

			if (id.ObjectClass == Sort.rxLine)
				lineIds.Remove(id);
			if (id.ObjectClass == Sort.rxBlockReference)
				blockReferenceIds.Remove(id);

			aeSortField[aeReSortField[id]].Remove(id);
			aeReSortField.Remove(id);
		}

		/// <summary>
		/// Adds the objectId to the sortation record.
		/// </summary>
		/// <param name="id">The (re)appended object to add to the record.</param>
		private void AddObject(ObjectId id)
		{
			if (!id.Validate(false))
				return;

			if (id.ObjectClass == Sort.rxLine)
				lineIds.Add(id);
			if (id.ObjectClass == Sort.rxBlockReference)
				blockReferenceIds.Add(id);
		}

		#endregion

		#region Delegates, Events, Handlers

		/// <summary>
		/// Object appended to drawing.
		/// </summary>
		/// <param name="sender">The object that invoked the event.</param>
		/// <param name="e">The event parameters.</param>
		private void SortField_ObjectAppended(object sender, SortEventArgs e) => AddObject(e.ObjectId);

		/// <summary>
		/// Object erased from drawing.
		/// </summary>
		/// <param name="sender">The object that invoked the event.</param>
		/// <param name="e">The event parameters.</param>
		private void SortField_ObjectErased(object sender, SortEventArgs e) => RemoveObject(e.ObjectId); 

		/// <summary>
		/// Object reappended from drawing.
		/// </summary>
		/// <param name="sender">The object that invoked the event.</param>
		/// <param name="e">The event parameters.</param>
		private void SortField_ObjectReappended(object sender, SortEventArgs e) => AddObject(e.ObjectId);

		/// <summary>
		/// Object unappended from drawing.
		/// </summary>
		/// <param name="sender">The object that invoked the event.</param>
		/// <param name="e">The event parameters.</param>
		private void SortField_ObjectUnappended(object sender, SortEventArgs e) => RemoveObject(e.ObjectId);

		/// <summary>
		/// Object modified in drawing.
		/// </summary>
		/// <param name="sender">The object that invoked the event.</param>
		/// <param name="e">The event parameters.</param>
		private void SortField_ObjectModified(object sender, SortEventArgs e) => AddObject(e.ObjectId);

		#endregion
	}
}
