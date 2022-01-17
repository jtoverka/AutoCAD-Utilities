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
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;

namespace ACAD_Utilities
{
	/// <summary>
	/// Represents a container for a database's entities.
	/// This will sort and partition each entity type.
	/// </summary>
	public sealed class Sort
	{
		#region Static Fields

		private static readonly Dictionary<Database, Sort> SortDictionary = new();

		/// <summary>
		/// A thread-safe lock object
		/// </summary>
		private static readonly object lockField = new();
		/// <summary>
		/// The AlignedDimension RXClass object.
		/// </summary>
		public static readonly RXClass rxAlignedDimension = RXObject.GetClass(typeof(AlignedDimension));
		/// <summary>
		/// The Arc RXClass object.
		/// </summary>
		public static readonly RXClass rxArc = RXObject.GetClass(typeof(Arc));
		/// <summary>
		/// The ArcDimension RXClass object.
		/// </summary>
		public static readonly RXClass rxArcDimension = RXObject.GetClass(typeof(ArcDimension));
		/// <summary>
		/// The AttributeDefinition RXClass object.
		/// </summary>
		public static readonly RXClass rxAttributeDefinition = RXObject.GetClass(typeof(AttributeDefinition));
		/// <summary>
		/// The AttributeReference RXClass object.
		/// </summary>
		public static readonly RXClass rxAttributeReference = RXObject.GetClass(typeof(AttributeReference));
		/// <summary>
		/// The BlockBegin RXClass object.
		/// </summary>
		public static readonly RXClass rxBlockBegin = RXObject.GetClass(typeof(BlockBegin));
		/// <summary>
		/// The BlockEnd RXClass object.
		/// </summary>
		public static readonly RXClass rxBlockEnd = RXObject.GetClass(typeof(BlockEnd));
		/// <summary>
		/// The BlockReference RXClass object.
		/// </summary>
		public static readonly RXClass rxBlockReference = RXObject.GetClass(typeof(BlockReference));
		/// <summary>
		/// The Body RXClass object.
		/// </summary>
		public static readonly RXClass rxBody = RXObject.GetClass(typeof(Body));
		/// <summary>
		/// The Circle RXClass object.
		/// </summary>
		public static readonly RXClass rxCircle = RXObject.GetClass(typeof(Circle));
		/// <summary>
		/// The Curve RXClass object.
		/// </summary>
		public static readonly RXClass rxCurve = RXObject.GetClass(typeof(Curve));
		/// <summary>
		/// The DBPoint RXClass object.
		/// </summary>
		public static readonly RXClass rxDBPoint = RXObject.GetClass(typeof(DBPoint));
		/// <summary>
		/// The DBText RXClass object.
		/// </summary>
		public static readonly RXClass rxDBText = RXObject.GetClass(typeof(DBText));
		/// <summary>
		/// The DetailSymbol RXClass object.
		/// </summary>
		public static readonly RXClass rxDetailSymbol = RXObject.GetClass(typeof(DetailSymbol));
		/// <summary>
		/// The DgnReference RXClass object.
		/// </summary>
		public static readonly RXClass rxDgnReference = RXObject.GetClass(typeof(DgnReference));
		/// <summary>
		/// The DiametricDimension RXClass object.
		/// </summary>
		public static readonly RXClass rxDiametricDimension = RXObject.GetClass(typeof(DiametricDimension));
		/// <summary>
		/// The Dimension RXClass object.
		/// </summary>
		public static readonly RXClass rxDimension = RXObject.GetClass(typeof(Dimension));
		/// <summary>
		/// The DwfReference RXClass object.
		/// </summary>
		public static readonly RXClass rxDwfReference = RXObject.GetClass(typeof(DwfReference));
		/// <summary>
		/// The Ellipse RXClass object.
		/// </summary>
		public static readonly RXClass rxEllipse = RXObject.GetClass(typeof(Ellipse));
		/// <summary>
		/// The Entity RXClass object.
		/// </summary>
		public static readonly RXClass rxEntity = RXObject.GetClass(typeof(Entity));
		/// <summary>
		/// The Face RXClass object.
		/// </summary>
		public static readonly RXClass rxFace = RXObject.GetClass(typeof(Face));
		/// <summary>
		/// The FaceRecord RXClass object.
		/// </summary>
		public static readonly RXClass rxFaceRecord = RXObject.GetClass(typeof(FaceRecord));
		/// <summary>
		/// The FeatureControlFrame RXClass object.
		/// </summary>
		public static readonly RXClass rxFeatureControlFrame = RXObject.GetClass(typeof(FeatureControlFrame));
		/// <summary>
		/// The GeomapImage RXClass object.
		/// </summary>
		public static readonly RXClass rxGeomapImage = RXObject.GetClass(typeof(GeomapImage));
		/// <summary>
		/// The GeoPositionMarker RXClass object.
		/// </summary>
		public static readonly RXClass rxGeoPositionMarker = RXObject.GetClass(typeof(GeoPositionMarker));
		/// <summary>
		/// The Hatch RXClass object.
		/// </summary>
		public static readonly RXClass rxHatch = RXObject.GetClass(typeof(Hatch));
		/// <summary>
		/// The Image RXClass object.
		/// </summary>
		public static readonly RXClass rxImage = RXObject.GetClass(typeof(Image));
		/// <summary>
		/// The Leader RXClass object.
		/// </summary>
		public static readonly RXClass rxLeader = RXObject.GetClass(typeof(Leader));
		/// <summary>
		/// The Light RXClass object.
		/// </summary>
		public static readonly RXClass rxLight = RXObject.GetClass(typeof(Light));
		/// <summary>
		/// The Line RXClass object.
		/// </summary>
		public static readonly RXClass rxLine = RXObject.GetClass(typeof(Line));
		/// <summary>
		/// The LineAngularDimension2 RXClass object.
		/// </summary>
		public static readonly RXClass rxLineAngularDimension2 = RXObject.GetClass(typeof(LineAngularDimension2));
		/// <summary>
		/// The LoftedSurface RXClass object.
		/// </summary>
		public static readonly RXClass rxLoftedSurface = RXObject.GetClass(typeof(LoftedSurface));
		/// <summary>
		/// The MInsertBlock RXClass object.
		/// </summary>
		public static readonly RXClass rxMInsertBlock = RXObject.GetClass(typeof(MInsertBlock));
		/// <summary>
		/// The MLeader RXClass object.
		/// </summary>
		public static readonly RXClass rxMLeader = RXObject.GetClass(typeof(MLeader));
		/// <summary>
		/// The Mline RXClass object.
		/// </summary>
		public static readonly RXClass rxMline = RXObject.GetClass(typeof(Mline));
		/// <summary>
		/// The MText RXClass object.
		/// </summary>
		public static readonly RXClass rxMText = RXObject.GetClass(typeof(MText));
		/// <summary>
		/// The Ole2Frame RXClass object.
		/// </summary>
		public static readonly RXClass rxOle2Frame = RXObject.GetClass(typeof(Ole2Frame));
		/// <summary>
		/// The PdfReference RXClass object.
		/// </summary>
		public static readonly RXClass rxPdfReference = RXObject.GetClass(typeof(PdfReference));
		/// <summary>
		/// The PlaneSurface RXClass object.
		/// </summary>
		public static readonly RXClass rxPlaneSurface = RXObject.GetClass(typeof(PlaneSurface));
		/// <summary>
		/// The Point3AngularDimension RXClass object.
		/// </summary>
		public static readonly RXClass rxPoint3AngularDimension = RXObject.GetClass(typeof(Point3AngularDimension));
		/// <summary>
		/// The PointCloud RXClass object.
		/// </summary>
		public static readonly RXClass rxPointCloud = RXObject.GetClass(typeof(PointCloud));
		/// <summary>
		/// The PointCloudEx RXClass object.
		/// </summary>
		public static readonly RXClass rxPointCloudEx = RXObject.GetClass(typeof(PointCloudEx));
		/// <summary>
		/// The PolyFaceMesh RXClass object.
		/// </summary>
		public static readonly RXClass rxPolyFaceMesh = RXObject.GetClass(typeof(PolyFaceMesh));
		/// <summary>
		/// The PolyFaceMeshVertex RXClass object.
		/// </summary>
		public static readonly RXClass rxPolyFaceMeshVertex = RXObject.GetClass(typeof(PolyFaceMeshVertex));
		/// <summary>
		/// The PolygonMesh RXClass object.
		/// </summary>
		public static readonly RXClass rxPolygonMesh = RXObject.GetClass(typeof(PolygonMesh));
		/// <summary>
		/// The PolygonMeshVertex RXClass object.
		/// </summary>
		public static readonly RXClass rxPolygonMeshVertex = RXObject.GetClass(typeof(PolygonMeshVertex));
		/// <summary>
		/// The Polyline RXClass object.
		/// </summary>
		public static readonly RXClass rxPolyline = RXObject.GetClass(typeof(Polyline));
		/// <summary>
		/// The Polyline2d RXClass object.
		/// </summary>
		public static readonly RXClass rxPolyline2d = RXObject.GetClass(typeof(Polyline2d));
		/// <summary>
		/// The Polyline3d RXClass object.
		/// </summary>
		public static readonly RXClass rxPolyline3d = RXObject.GetClass(typeof(Polyline3d));
		/// <summary>
		/// The PolylineVertex3d RXClass object.
		/// </summary>
		public static readonly RXClass rxPolylineVertex3d = RXObject.GetClass(typeof(PolylineVertex3d));
		/// <summary>
		/// The ProxyEntity RXClass object.
		/// </summary>
		public static readonly RXClass rxProxyEntity = RXObject.GetClass(typeof(ProxyEntity));
		/// <summary>
		/// The RadialDimension RXClass object.
		/// </summary>
		public static readonly RXClass rxRadialDimension = RXObject.GetClass(typeof(RadialDimension));
		/// <summary>
		/// The RadialDimensionLarge RXClass object.
		/// </summary>
		public static readonly RXClass rxRadialDimensionLarge = RXObject.GetClass(typeof(RadialDimensionLarge));
		/// <summary>
		/// The Ray RXClass object.
		/// </summary>
		public static readonly RXClass rxRay = RXObject.GetClass(typeof(Ray));
		/// <summary>
		/// The RasterImage RXClass object.
		/// </summary>
		public static readonly RXClass rxRasterImage = RXObject.GetClass(typeof(RasterImage));
		/// <summary>
		/// The Region RXClass object.
		/// </summary>
		public static readonly RXClass rxRegion = RXObject.GetClass(typeof(Region));
		/// <summary>
		/// The RotatedDimension RXClass object.
		/// </summary>
		public static readonly RXClass rxRotatedDimension = RXObject.GetClass(typeof(RotatedDimension));
		/// <summary>
		/// The Section RXClass object.
		/// </summary>
		public static readonly RXClass rxSection = RXObject.GetClass(typeof(Section));
		/// <summary>
		/// The SectionSymbol RXClass object.
		/// </summary>
		public static readonly RXClass rxSectionSymbol = RXObject.GetClass(typeof(SectionSymbol));
		/// <summary>
		/// The SequenceEnd RXClass object.
		/// </summary>
		public static readonly RXClass rxSequenceEnd = RXObject.GetClass(typeof(SequenceEnd));
		/// <summary>
		/// The Shape RXClass object.
		/// </summary>
		public static readonly RXClass rxShape = RXObject.GetClass(typeof(Shape));
		/// <summary>
		/// The Solid RXClass object.
		/// </summary>
		public static readonly RXClass rxSolid = RXObject.GetClass(typeof(Solid));
		/// <summary>
		/// The Solid3d RXClass object.
		/// </summary>
		public static readonly RXClass rxSolid3d = RXObject.GetClass(typeof(Solid3d));
		/// <summary>
		/// The SubDMesh RXClass object.
		/// </summary>
		public static readonly RXClass rxSubDMesh = RXObject.GetClass(typeof(SubDMesh));
		/// <summary>
		/// The Surface RXClass object.
		/// </summary>
		public static readonly RXClass rxSurface = RXObject.GetClass(typeof(Surface));
		/// <summary>
		/// The Spline RXClass object.
		/// </summary>
		public static readonly RXClass rxSpline = RXObject.GetClass(typeof(Spline));
		/// <summary>
		/// The SweptSurface RXClass object.
		/// </summary>
		public static readonly RXClass rxSweptSurface = RXObject.GetClass(typeof(SweptSurface));
		/// <summary>
		/// The Table RXClass object.
		/// </summary>
		public static readonly RXClass rxTable = RXObject.GetClass(typeof(Table));
		/// <summary>
		/// The UnderlayReference RXClass object.
		/// </summary>
		public static readonly RXClass rxUnderlayReference = RXObject.GetClass(typeof(UnderlayReference));
		/// <summary>
		/// The Vertex RXClass object.
		/// </summary>
		public static readonly RXClass rxVertex = RXObject.GetClass(typeof(Vertex));
		/// <summary>
		/// The Vertex2d RXClass object.
		/// </summary>
		public static readonly RXClass rxVertex2d = RXObject.GetClass(typeof(Vertex2d));
		/// <summary>
		/// The ViewBorder RXClass object.
		/// </summary>
		public static readonly RXClass rxViewBorder = RXObject.GetClass(typeof(ViewBorder));
		/// <summary>
		/// The Viewport RXClass object.
		/// </summary>
		public static readonly RXClass rxViewport = RXObject.GetClass(typeof(Viewport));
		/// <summary>
		/// The ViewRepBlockReference RXClass object.
		/// </summary>
		public static readonly RXClass rxViewRepBlockReference = RXObject.GetClass(typeof(ViewRepBlockReference));
		/// <summary>
		/// The Wipeout RXClass object.
		/// </summary>
		public static readonly RXClass rxWipeout = RXObject.GetClass(typeof(Wipeout));
		/// <summary>
		/// The Xline RXClass object.
		/// </summary>
		public static readonly RXClass rxXline = RXObject.GetClass(typeof(Xline));

		#endregion

		#region Instance Fields

		private readonly Dictionary<RXClass, HashSet<ObjectId>> entitiesField;

		#endregion

		#region Instance Properties

		/// <summary>
		/// Gets or sets the value associated with the specified key.
		/// </summary>
		/// <param name="class">The key of the value to get or set.</param>
		/// <returns></returns>
		public HashSet<ObjectId> this[RXClass @class]
		{
			get { return entitiesField[@class]; }
			set { entitiesField[@class] = value; }
		}

		/// <summary>
		/// Gets the database this object sorts.
		/// </summary>
		public Database Database { get; }

		#endregion

		#region Constructor

		/// <summary>
		/// Create a new instance of this class.
		/// </summary>
		/// <param name="database"></param>
		private Sort(Database database)
		{
			database.Validate(true);

			bool started = database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			entitiesField = new();
			Database = database;

			database.ObjectAppended += Database_ObjectAppended;
			database.ObjectErased += Database_ObjectErased;
			database.ObjectUnappended += Database_ObjectUnappended;
			database.ObjectReappended += Database_ObjectReappended;
			database.ObjectModified += Database_ObjectModified;

			ObjectIdCollection spaces = new();

			using BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
			spaces.Add(blockTable[BlockTableRecord.ModelSpace]);

			// Get Layout dictionary
			using DBDictionary layouts = transaction.GetObject(database.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

			// Iterate through the layouts
			foreach (DBDictionaryEntry layoutEntry in layouts)
			{
				ObjectId layoutId = layoutEntry.Value;
				
				if (!layoutId.Validate(false))
					continue;

				// Get the layout
				using Layout layout = transaction.GetObject(layoutId, OpenMode.ForRead) as Layout;
				spaces.Add(layout.BlockTableRecordId);
			}

			// Iterate through the entities in each layout
			foreach (ObjectId spaceId in spaces)
			{
				using BlockTableRecord space = transaction.GetObject(spaceId, OpenMode.ForRead) as BlockTableRecord;

				// Put each entity type into it's respective buckets.
				foreach (ObjectId entityId in space)
					AddId(entitiesField, entityId);
			}
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Sort and add ObjectID to dictionary.
		/// </summary>
		/// <param name="entities">Dictionary of sorted ObjectIds</param>
		/// <param name="entityId">ObjectID to add.</param>
		public static void AddId(Dictionary<RXClass, HashSet<ObjectId>> entities, ObjectId entityId)
		{
			if (!entityId.Validate(false))
				return;

			RXClass objectClass = entityId.ObjectClass;

			if (objectClass == null)
				return;

			// Create a generic container if one does not exist
			if (!entities.ContainsKey(rxEntity))
				entities[rxEntity] = new();

			// Create a specific container if one does not exist
			if (!entities.ContainsKey(objectClass))
				entities[objectClass] = new();

			// Place entity into it's respective buckets.
			entities[rxEntity].Add(entityId);
			entities[objectClass].Add(entityId);
		}

		/// <summary>
		/// Removes the ObjectID from the dictionary.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="entityId"></param>
		public static void RemoveId(Dictionary<RXClass, HashSet<ObjectId>> entities, ObjectId entityId)
		{
			if (entities.ContainsKey(rxEntity))
				entities[rxEntity].Remove(entityId);

			if (entities.ContainsKey(entityId.ObjectClass))
				entities[entityId.ObjectClass].Remove(entityId);
		}

		/// <summary>
		/// Gets the sorted database object
		/// </summary>
		/// <param name="database">The database to sort.</param>
		/// <returns></returns>
		public static Sort GetSort(Database database)
		{
			database.Validate(true, true);

			// Prevent multiple threads from stumbling over the lock.
			if (!SortDictionary.ContainsKey(database))
			{
				// First thread locks onto the singleton factory.
				lock (lockField)
				{
					// Create sorted dictionary on the database if one does not exist.
					if (!SortDictionary.ContainsKey(database))
						SortDictionary[database] = new(database);
				}
			}

			return SortDictionary[database];
		}

		#endregion

		#region Instance Methods

		/// <summary>
		/// Determines whether the Dictioanry contains the specified key. />
		/// contains the specified key.
		/// </summary>
		/// <param name="class"></param>
		/// <returns><see langword="true"/> if the Dictionary contains an element with the specified key; Otherwise, <see langword="false"/>.</returns>
		/// <exception cref="System.ArgumentNullException"/>
		public bool ContainsKey(RXClass @class) => entitiesField.ContainsKey(@class);

		#endregion

		#region Delegates, Events, Handlers

		/// <summary>
		/// Invoked on a new object being appended to the database.
		/// </summary>
		public event SortEventHandler ObjectAppended;

		/// <summary>
		/// Invoked on an object being erased from the database.
		/// </summary>
		public event SortEventHandler ObjectErased;

		/// <summary>
		/// Invoked on an object being unappended from the database.
		/// </summary>
		public event SortEventHandler ObjectUnappended;

		/// <summary>
		/// Invoked on an object being reappended to the database.
		/// </summary>
		public event SortEventHandler ObjectReappended;

		/// <summary>
		/// Invoked on an object being modified.
		/// </summary>
		public event SortEventHandler ObjectModified;

		/// <summary>
		/// Database object appended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectAppended(object sender, ObjectEventArgs e)
		{
			AddId(entitiesField, e.DBObject.Id);
			ObjectAppended?.Invoke(this, new(e.DBObject.Id));
		}

		/// <summary>
		/// Database object erased handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
		{
			RemoveId(entitiesField, e.DBObject.Id);
			ObjectErased?.Invoke(this, new(e.DBObject.Id));
		}

		/// <summary>
		/// Database unappended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectUnappended(object sender, ObjectEventArgs e)
		{
			RemoveId(entitiesField, e.DBObject.Id);
			ObjectUnappended?.Invoke(this, new(e.DBObject.Id));
		}

		/// <summary>
		/// Database reappended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectReappended(object sender, ObjectEventArgs e)
		{
			AddId(entitiesField, e.DBObject.Id);
			ObjectReappended?.Invoke(this, new(e.DBObject.Id));
		}

		/// <summary>
		/// Database object modified handler.
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectModified(object sender, ObjectEventArgs e)
		{
			ObjectModified?.Invoke(this, new(e.DBObject.Id));
		}

		#endregion
	}
}
