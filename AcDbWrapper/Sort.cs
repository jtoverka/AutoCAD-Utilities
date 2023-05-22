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
using System;
using System.Collections.Generic;

namespace AcDbWrapper
{
	/// <summary>
	/// Represents a container for a database's entities.
	/// This will sort and partition each entity type.
	/// </summary>
	public sealed partial class Sort : IUniqueDatabase
	{
        #region Instance Properties

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="class">The key of the value to get or set.</param>
        /// <returns></returns>
        [Obsolete("Planned Removal in future, please use GetEntities(RXClass class)")]
        public ObjectIdSet this[RXClass @class]
		{
			get { return Entities[@class]; }
		}

        /// <summary>
        /// Gets the entity dictionary.
        /// </summary>
        /// <remarks>
        /// The entities are sorted by RXClass.
        /// </remarks>
        public SortDictionary<RXClass> Entities { get; }

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

            Entities = new(database, (@class, set) => 
            {
                return !(set.ObjectClass == @class || set.ObjectClass.IsDerivedFrom(@class));
            });
			Database = database;

			database.ObjectAppended += Database_ObjectAppended;
			database.ObjectErased += Database_ObjectErased;
			database.ObjectUnappended += Database_ObjectUnappended;
			database.ObjectReappended += Database_ObjectReappended;
			database.ObjectModified += Database_ObjectModified;
            database.Disposed += Database_Disposed;

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
                {
                    EntityTableAddId(Entities, entityId);
                }
			}
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
        public bool ContainsKey(RXClass @class) => Entities.ContainsKey(@class);

		#endregion

		#region Delegates, Events, Handlers

		/// <summary>
		/// Invoked on a new object being appended to the database.
		/// </summary>
		public event EventHandler<SortEventArgs> ObjectAppended;

		/// <summary>
		/// Invoked on an object being erased from the database.
		/// </summary>
		public event EventHandler<SortEventArgs> ObjectErased;

		/// <summary>
		/// Invoked on an object being unappended from the database.
		/// </summary>
		public event EventHandler<SortEventArgs> ObjectUnappended;

		/// <summary>
		/// Invoked on an object being reappended to the database.
		/// </summary>
		public event EventHandler<SortEventArgs> ObjectReappended;

		/// <summary>
		/// Invoked on an object being modified.
		/// </summary>
		public event EventHandler<SortEventArgs> ObjectModified;

        /// <summary>
        /// Invoked when a database is disposed.
        /// </summary>
        public event EventHandler<EventArgs> DatabaseDisposed;

		/// <summary>
		/// Database object appended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectAppended(object sender, ObjectEventArgs e)
		{
            ObjectId id = e.DBObject.Id;

            if (id.IsMatch<Entity>())
            {
                EntityTableAddId(Entities, e.DBObject.Id);
            }
			ObjectAppended?.Invoke(this, new(e.DBObject.Id));
		}

		/// <summary>
		/// Database object erased handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
		{
            EntityTableRemoveId(Entities, e.DBObject.Id);
			ObjectErased?.Invoke(this, new(e.DBObject.Id));
		}

		/// <summary>
		/// Database unappended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectUnappended(object sender, ObjectEventArgs e)
		{
            EntityTableRemoveId(Entities, e.DBObject.Id);
			ObjectUnappended?.Invoke(this, new(e.DBObject.Id));
		}

		/// <summary>
		/// Database reappended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectReappended(object sender, ObjectEventArgs e)
		{
            EntityTableAddId(Entities, e.DBObject.Id);
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

        /// <summary>
        /// Database has been disposed.
        /// </summary>
        /// <param name="sender">Object that triggered the event.</param>
        /// <param name="e">The event data.</param>
        private void Database_Disposed(object sender, EventArgs e)
        {
            Database.ObjectAppended -= Database_ObjectAppended;
            Database.ObjectErased -= Database_ObjectErased;
            Database.ObjectUnappended -= Database_ObjectUnappended;
            Database.ObjectReappended -= Database_ObjectReappended;
            Database.ObjectModified -= Database_ObjectModified;
            Database.Disposed -= Database_Disposed;

            DatabaseDisposed?.Invoke(this, new());
        }

        #endregion
    }

    /// <summary>
    /// Represents the static parts of the database object.
    /// </summary>
	public sealed partial class Sort
    {
        #region Static Fields

        /// <summary>
        /// Stores all databases and the respective sortation object.
        /// </summary>
        private static readonly Dictionary<Database, Sort> SortDictionary = new();

        /// <summary>
        /// A thread-safe lock object
        /// </summary>
        private static readonly object lockField = new();

        /// <summary>
        /// The AlignedDimension RXClass object.
        /// </summary>
        public static readonly RXClass rxAlignedDimension = Rx.GetClass<AlignedDimension>();
        /// <summary>
        /// The Arc RXClass object.
        /// </summary>
        public static readonly RXClass rxArc = Rx.GetClass<Arc>();
        /// <summary>
        /// The ArcDimension RXClass object.
        /// </summary>
        public static readonly RXClass rxArcDimension = Rx.GetClass<ArcDimension>();
        /// <summary>
        /// The AttributeDefinition RXClass object.
        /// </summary>
        public static readonly RXClass rxAttributeDefinition = Rx.GetClass<AttributeDefinition>();
        /// <summary>
        /// The AttributeReference RXClass object.
        /// </summary>
        public static readonly RXClass rxAttributeReference = Rx.GetClass<AttributeReference>();
        /// <summary>
        /// The BlockBegin RXClass object.
        /// </summary>
        public static readonly RXClass rxBlockBegin = Rx.GetClass<BlockBegin>();
        /// <summary>
        /// The BlockEnd RXClass object.
        /// </summary>
        public static readonly RXClass rxBlockEnd = Rx.GetClass<BlockEnd>();
        /// <summary>
        /// The BlockReference RXClass object.
        /// </summary>
        public static readonly RXClass rxBlockReference = Rx.GetClass<BlockReference>();
        /// <summary>
        /// The Body RXClass object.
        /// </summary>
        public static readonly RXClass rxBody = Rx.GetClass<Body>();
        /// <summary>
        /// The Circle RXClass object.
        /// </summary>
        public static readonly RXClass rxCircle = Rx.GetClass<Circle>();
        /// <summary>
        /// The Curve RXClass object.
        /// </summary>
        public static readonly RXClass rxCurve = Rx.GetClass<Curve>();
        /// <summary>
        /// The DBPoint RXClass object.
        /// </summary>
        public static readonly RXClass rxDBPoint = Rx.GetClass<DBPoint>();
        /// <summary>
        /// The DBText RXClass object.
        /// </summary>
        public static readonly RXClass rxDBText = Rx.GetClass<DBText>();
        /// <summary>
        /// The DetailSymbol RXClass object.
        /// </summary>
        public static readonly RXClass rxDetailSymbol = Rx.GetClass<DetailSymbol>();
        /// <summary>
        /// The DgnReference RXClass object.
        /// </summary>
        public static readonly RXClass rxDgnReference = Rx.GetClass<DgnReference>();
        /// <summary>
        /// The DiametricDimension RXClass object.
        /// </summary>
        public static readonly RXClass rxDiametricDimension = Rx.GetClass<DiametricDimension>();
        /// <summary>
        /// The Dimension RXClass object.
        /// </summary>
        public static readonly RXClass rxDimension = Rx.GetClass<Dimension>();
        /// <summary>
        /// The DwfReference RXClass object.
        /// </summary>
        public static readonly RXClass rxDwfReference = Rx.GetClass<DwfReference>();
        /// <summary>
        /// The Ellipse RXClass object.
        /// </summary>
        public static readonly RXClass rxEllipse = Rx.GetClass<Ellipse>();
        /// <summary>
        /// The Entity RXClass object.
        /// </summary>
        public static readonly RXClass rxEntity = Rx.GetClass<Entity>();
        /// <summary>
        /// The Face RXClass object.
        /// </summary>
        public static readonly RXClass rxFace = Rx.GetClass<Face>();
        /// <summary>
        /// The FaceRecord RXClass object.
        /// </summary>
        public static readonly RXClass rxFaceRecord = Rx.GetClass<FaceRecord>();
        /// <summary>
        /// The FeatureControlFrame RXClass object.
        /// </summary>
        public static readonly RXClass rxFeatureControlFrame = Rx.GetClass<FeatureControlFrame>();
        /// <summary>
        /// The GeomapImage RXClass object.
        /// </summary>
        public static readonly RXClass rxGeomapImage = Rx.GetClass<GeomapImage>();
        /// <summary>
        /// The GeoPositionMarker RXClass object.
        /// </summary>
        public static readonly RXClass rxGeoPositionMarker = Rx.GetClass<GeoPositionMarker>();
        /// <summary>
        /// The Hatch RXClass object.
        /// </summary>
        public static readonly RXClass rxHatch = Rx.GetClass<Hatch>();
        /// <summary>
        /// The Image RXClass object.
        /// </summary>
        public static readonly RXClass rxImage = Rx.GetClass<Image>();
        /// <summary>
        /// The Leader RXClass object.
        /// </summary>
        public static readonly RXClass rxLeader = Rx.GetClass<Leader>();
        /// <summary>
        /// The Light RXClass object.
        /// </summary>
        public static readonly RXClass rxLight = Rx.GetClass<Light>();
        /// <summary>
        /// The Line RXClass object.
        /// </summary>
        public static readonly RXClass rxLine = Rx.GetClass<Line>();
        /// <summary>
        /// The LineAngularDimension2 RXClass object.
        /// </summary>
        public static readonly RXClass rxLineAngularDimension2 = RXObject.GetClass(typeof(LineAngularDimension2));
        /// <summary>
        /// The LoftedSurface RXClass object.
        /// </summary>
        public static readonly RXClass rxLoftedSurface = Rx.GetClass<LoftedSurface>();
        /// <summary>
        /// The MInsertBlock RXClass object.
        /// </summary>
        public static readonly RXClass rxMInsertBlock = Rx.GetClass<MInsertBlock>();
        /// <summary>
        /// The MLeader RXClass object.
        /// </summary>
        public static readonly RXClass rxMLeader = Rx.GetClass<MLeader>();
        /// <summary>
        /// The Mline RXClass object.
        /// </summary>
        public static readonly RXClass rxMline = Rx.GetClass<Mline>();
        /// <summary>
        /// The MText RXClass object.
        /// </summary>
        public static readonly RXClass rxMText = Rx.GetClass<MText>();
        /// <summary>
        /// The Ole2Frame RXClass object.
        /// </summary>
        public static readonly RXClass rxOle2Frame = RXObject.GetClass(typeof(Ole2Frame));
        /// <summary>
        /// The PdfReference RXClass object.
        /// </summary>
        public static readonly RXClass rxPdfReference = Rx.GetClass<PdfReference>();
        /// <summary>
        /// The PlaneSurface RXClass object.
        /// </summary>
        public static readonly RXClass rxPlaneSurface = Rx.GetClass<PlaneSurface>();
        /// <summary>
        /// The Point3AngularDimension RXClass object.
        /// </summary>
        public static readonly RXClass rxPoint3AngularDimension = RXObject.GetClass(typeof(Point3AngularDimension));
        /// <summary>
        /// The PointCloud RXClass object.
        /// </summary>
        public static readonly RXClass rxPointCloud = Rx.GetClass<PointCloud>();
        /// <summary>
        /// The PointCloudEx RXClass object.
        /// </summary>
        public static readonly RXClass rxPointCloudEx = Rx.GetClass<PointCloudEx>();
        /// <summary>
        /// The PolyFaceMesh RXClass object.
        /// </summary>
        public static readonly RXClass rxPolyFaceMesh = Rx.GetClass<PolyFaceMesh>();
        /// <summary>
        /// The PolyFaceMeshVertex RXClass object.
        /// </summary>
        public static readonly RXClass rxPolyFaceMeshVertex = Rx.GetClass<PolyFaceMeshVertex>();
        /// <summary>
        /// The PolygonMesh RXClass object.
        /// </summary>
        public static readonly RXClass rxPolygonMesh = Rx.GetClass<PolygonMesh>();
        /// <summary>
        /// The PolygonMeshVertex RXClass object.
        /// </summary>
        public static readonly RXClass rxPolygonMeshVertex = Rx.GetClass<PolygonMeshVertex>();
        /// <summary>
        /// The Polyline RXClass object.
        /// </summary>
        public static readonly RXClass rxPolyline = Rx.GetClass<Polyline>();
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
        public static readonly RXClass rxProxyEntity = Rx.GetClass<ProxyEntity>();
        /// <summary>
        /// The RadialDimension RXClass object.
        /// </summary>
        public static readonly RXClass rxRadialDimension = Rx.GetClass<RadialDimension>();
        /// <summary>
        /// The RadialDimensionLarge RXClass object.
        /// </summary>
        public static readonly RXClass rxRadialDimensionLarge = Rx.GetClass<RadialDimensionLarge>();
        /// <summary>
        /// The Ray RXClass object.
        /// </summary>
        public static readonly RXClass rxRay = Rx.GetClass<Ray>();
        /// <summary>
        /// The RasterImage RXClass object.
        /// </summary>
        public static readonly RXClass rxRasterImage = Rx.GetClass<RasterImage>();
        /// <summary>
        /// The Region RXClass object.
        /// </summary>
        public static readonly RXClass rxRegion = Rx.GetClass<Region>();
        /// <summary>
        /// The RotatedDimension RXClass object.
        /// </summary>
        public static readonly RXClass rxRotatedDimension = Rx.GetClass<RotatedDimension>();
        /// <summary>
        /// The Section RXClass object.
        /// </summary>
        public static readonly RXClass rxSection = Rx.GetClass<Section>();
        /// <summary>
        /// The SectionSymbol RXClass object.
        /// </summary>
        public static readonly RXClass rxSectionSymbol = Rx.GetClass<SectionSymbol>();
        /// <summary>
        /// The SequenceEnd RXClass object.
        /// </summary>
        public static readonly RXClass rxSequenceEnd = Rx.GetClass<SequenceEnd>();
        /// <summary>
        /// The Shape RXClass object.
        /// </summary>
        public static readonly RXClass rxShape = Rx.GetClass<Shape>();
        /// <summary>
        /// The Solid RXClass object.
        /// </summary>
        public static readonly RXClass rxSolid = Rx.GetClass<Solid>();
        /// <summary>
        /// The Solid3d RXClass object.
        /// </summary>
        public static readonly RXClass rxSolid3d = RXObject.GetClass(typeof(Solid3d));
        /// <summary>
        /// The SubDMesh RXClass object.
        /// </summary>
        public static readonly RXClass rxSubDMesh = Rx.GetClass<SubDMesh>();
        /// <summary>
        /// The Surface RXClass object.
        /// </summary>
        public static readonly RXClass rxSurface = Rx.GetClass<Surface>();
        /// <summary>
        /// The Spline RXClass object.
        /// </summary>
        public static readonly RXClass rxSpline = Rx.GetClass<Spline>();
        /// <summary>
        /// The SweptSurface RXClass object.
        /// </summary>
        public static readonly RXClass rxSweptSurface = Rx.GetClass<SweptSurface>();
        /// <summary>
        /// The Table RXClass object.
        /// </summary>
        public static readonly RXClass rxTable = Rx.GetClass<Table>();
        /// <summary>
        /// The UnderlayReference RXClass object.
        /// </summary>
        public static readonly RXClass rxUnderlayReference = Rx.GetClass<UnderlayReference>();
        /// <summary>
        /// The Vertex RXClass object.
        /// </summary>
        public static readonly RXClass rxVertex = Rx.GetClass<Vertex>();
        /// <summary>
        /// The Vertex2d RXClass object.
        /// </summary>
        public static readonly RXClass rxVertex2d = RXObject.GetClass(typeof(Vertex2d));
        /// <summary>
        /// The ViewBorder RXClass object.
        /// </summary>
        public static readonly RXClass rxViewBorder = Rx.GetClass<ViewBorder>();
        /// <summary>
        /// The Viewport RXClass object.
        /// </summary>
        public static readonly RXClass rxViewport = Rx.GetClass<Viewport>();
        /// <summary>
        /// The ViewRepBlockReference RXClass object.
        /// </summary>
        public static readonly RXClass rxViewRepBlockReference = Rx.GetClass<ViewRepBlockReference>();
        /// <summary>
        /// The Wipeout RXClass object.
        /// </summary>
        public static readonly RXClass rxWipeout = Rx.GetClass<Wipeout>();
        /// <summary>
        /// The Xline RXClass object.
        /// </summary>
        public static readonly RXClass rxXline = Rx.GetClass<Xline>();

        #endregion

        #region Static Methods

        /// <summary>
        /// Sort and add ObjectID to dictionary.
        /// </summary>
        /// <param name="entities">Dictionary of sorted ObjectIds</param>
        /// <param name="entityId">ObjectID to add.</param>
        [Obsolete()]
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
        /// Sort and add ObjectID to dictionary.
        /// </summary>
        /// <param name="table">Dictionary of sorted ObjectIds</param>
        /// <param name="id">ObjectId to add.</param>
        private static void EntityTableAddId(SortDictionary<RXClass> table, ObjectId id)
        {
            if (!id.Validate(false))
                return;

            RXClass objectClass = id.ObjectClass;
            Database objectDatabase = id.Database;

            if (objectClass == null || objectDatabase == null)
                return;

            // Create a generic container if one does not exist
            if (!table.ContainsKey(rxEntity))
                table[rxEntity] = new(objectDatabase, rxEntity);

            // Create a specific container if one does not exist
            if (!table.ContainsKey(objectClass))
                table[objectClass] = new(objectDatabase, objectClass);

            // Place entity into its respective buckets.
            table[rxEntity].Add(id);
            table[objectClass].Add(id);
        }

        /// <summary>
        /// Sort and add ObjectId to dictionary.
        /// </summary>
        /// <param name="table">Dictionary of sorted ObjectIds</param>
        /// <param name="id">ObjectId to add.</param>
        private static void SymbolTableAddId(SortDictionary<RXClass> table, ObjectId id)
        {
            if (!id.Validate(false))
                return;

            if (!id.IsMatch<SymbolTableRecord>() || id.Database == null)
                return;

            RXClass objectClass = id.ObjectClass;
            Database objectDatabase = id.Database;

            bool started = objectDatabase.GetOrStartTransaction(out Transaction transaction);
            using Disposable dispose = new(transaction, started);

            using SymbolTableRecord symbolTableRecord = transaction.GetObject(id, OpenMode.ForRead) as SymbolTableRecord;
            if (symbolTableRecord != null)
            {
                if (!table.ContainsKey(objectClass))
                    table[objectClass] = new(objectDatabase, objectClass);

                string name = symbolTableRecord.Name;

                // Add id to the table
                table[objectClass].Add(id);
            }
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
                    {
                        Sort sort = new(database);
                        sort.DatabaseDisposed += Sort_DatabaseDisposed;
                        SortDictionary[database] = sort;
                    }
                }
            }

            return SortDictionary[database];
        }

        /// <summary>
        /// Removes the ObjectID from the dictionary.
        /// </summary>
        /// <param name="entities">The dictionary to remove <see cref="ObjectId"/></param>
        /// <param name="entityId">The <see cref="ObjectId"/> to remove from dictionary</param>
        [Obsolete()]
        public static void RemoveId(Dictionary<RXClass, HashSet<ObjectId>> entities, ObjectId entityId)
        {
            if (entities.ContainsKey(rxEntity))
                entities[rxEntity].Remove(entityId);

            if (entities.ContainsKey(entityId.ObjectClass))
                entities[entityId.ObjectClass].Remove(entityId);
        }

        /// <summary>
        /// Removes the <see cref="ObjectId"/> from the dictionary.
        /// </summary>
        /// <param name="entities">The dictionary to remove <see cref="ObjectId"/></param>
        /// <param name="entityId">The <see cref="ObjectId"/> to remove from dictionary</param>
        private static void EntityTableRemoveId(SortDictionary<RXClass> entities, ObjectId entityId)
        {
            if (entities.ContainsKey(rxEntity))
                entities[rxEntity].Remove(entityId);

            if (entities.ContainsKey(entityId.ObjectClass))
                entities[entityId.ObjectClass].Remove(entityId);
        }

        /// <summary>
        /// Release the database from the sort dictionary when disposed.
        /// </summary>
        /// <param name="sender">The object that invoked the event.</param>
        /// <param name="e">The event data.</param>
        private static void Sort_DatabaseDisposed(object sender, EventArgs e)
        {
            if (sender is Sort sort)
            {
                sort.DatabaseDisposed -= Sort_DatabaseDisposed;
                SortDictionary.Remove(sort.Database);
            }
        }

        #endregion
    }
}
