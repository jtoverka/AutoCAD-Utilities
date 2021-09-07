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

		#endregion

		#region Constructor

		/// <summary>
		/// Create a new instance of this class.
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="database"></param>
		public Sort(Transaction transaction, Database database)
		{
			database.Validate(true);
			transaction.Validate(true);

			entitiesField = new();

			database.ObjectAppended += Database_ObjectAppended;
			database.ObjectErased += Database_ObjectErased;
			database.ObjectUnappended += Database_ObjectUnappended;
			database.ObjectReappended += Database_ObjectReappended;

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

		/// <summary>
		/// Create a new instance of this class.
		/// </summary>
		/// <param name="entities"></param>
		public Sort(HashSet<ObjectId> entities)
		{
			// Put each entity type into it's respective buckets.
			foreach (ObjectId id in entities)
				AddId(entitiesField, id);
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

			// Check to see if it really is an entity
			if (entityId.ObjectClass.GetRuntimeType() != typeof(Entity))
				return;

			// Create a generic container if one does not exist
			if (!entities.ContainsKey(rxEntity))
				entities[rxEntity] = new();

			// Create a specific container if one does not exist
			if (!entities.ContainsKey(entityId.ObjectClass))
				entities[entityId.ObjectClass] = new();

			// Put each entity type into it's respective buckets.
			if (entityId.ObjectClass == rxAlignedDimension)
			{
				entities[rxEntity].Add(entityId);
				entities[rxAlignedDimension].Add(entityId);
			}
			else if (entityId.ObjectClass == rxArc)
			{
				entities[rxEntity].Add(entityId);
				entities[rxArc].Add(entityId);
			}
			else if (entityId.ObjectClass == rxArcDimension)
			{
				entities[rxEntity].Add(entityId);
				entities[rxArcDimension].Add(entityId);
			}
			else if (entityId.ObjectClass == rxAttributeDefinition)
			{
				entities[rxEntity].Add(entityId);
				entities[rxAttributeDefinition].Add(entityId);
			}
			else if (entityId.ObjectClass == rxAttributeReference)
			{
				entities[rxEntity].Add(entityId);
				entities[rxAttributeReference].Add(entityId);
			}
			else if (entityId.ObjectClass == rxBlockBegin)
			{
				entities[rxEntity].Add(entityId);
				entities[rxBlockBegin].Add(entityId);
			}
			else if (entityId.ObjectClass == rxBlockEnd)
			{
				entities[rxEntity].Add(entityId);
				entities[rxBlockEnd].Add(entityId);
			}
			else if (entityId.ObjectClass == rxBlockReference)
			{
				entities[rxEntity].Add(entityId);
				entities[rxBlockReference].Add(entityId);
			}
			else if (entityId.ObjectClass == rxBody)
			{
				entities[rxEntity].Add(entityId);
				entities[rxBody].Add(entityId);
			}
			else if (entityId.ObjectClass == rxCircle)
			{
				entities[rxEntity].Add(entityId);
				entities[rxCircle].Add(entityId);
			}
			else if (entityId.ObjectClass == rxCurve)
			{
				entities[rxEntity].Add(entityId);
				entities[rxCurve].Add(entityId);
			}
			else if (entityId.ObjectClass == rxDBPoint)
			{
				entities[rxEntity].Add(entityId);
				entities[rxDBPoint].Add(entityId);
			}
			else if (entityId.ObjectClass == rxDBText)
			{
				entities[rxEntity].Add(entityId);
				entities[rxDBText].Add(entityId);
			}
			else if (entityId.ObjectClass == rxDetailSymbol)
			{
				entities[rxEntity].Add(entityId);
				entities[rxDetailSymbol].Add(entityId);
			}
			else if (entityId.ObjectClass == rxDgnReference)
			{
				entities[rxEntity].Add(entityId);
				entities[rxDgnReference].Add(entityId);
			}
			else if (entityId.ObjectClass == rxDiametricDimension)
			{
				entities[rxEntity].Add(entityId);
				entities[rxDiametricDimension].Add(entityId);
			}
			else if (entityId.ObjectClass == rxDimension)
			{
				entities[rxEntity].Add(entityId);
				entities[rxDimension].Add(entityId);
			}
			else if (entityId.ObjectClass == rxDwfReference)
			{
				entities[rxEntity].Add(entityId);
				entities[rxDwfReference].Add(entityId);
			}
			else if (entityId.ObjectClass == rxEllipse)
			{
				entities[rxEntity].Add(entityId);
				entities[rxEllipse].Add(entityId);
			}
			else if (entityId.ObjectClass == rxEntity)
			{
				entities[rxEntity].Add(entityId);
				entities[rxEntity].Add(entityId);
			}
			else if (entityId.ObjectClass == rxFace)
			{
				entities[rxEntity].Add(entityId);
				entities[rxFace].Add(entityId);
			}
			else if (entityId.ObjectClass == rxFaceRecord)
			{
				entities[rxEntity].Add(entityId);
				entities[rxFaceRecord].Add(entityId);
			}
			else if (entityId.ObjectClass == rxFeatureControlFrame)
			{
				entities[rxEntity].Add(entityId);
				entities[rxFeatureControlFrame].Add(entityId);
			}
			else if (entityId.ObjectClass == rxGeomapImage)
			{
				entities[rxEntity].Add(entityId);
				entities[rxGeomapImage].Add(entityId);
			}
			else if (entityId.ObjectClass == rxGeoPositionMarker)
			{
				entities[rxEntity].Add(entityId);
				entities[rxGeoPositionMarker].Add(entityId);
			}
			else if (entityId.ObjectClass == rxHatch)
			{
				entities[rxEntity].Add(entityId);
				entities[rxHatch].Add(entityId);
			}
			else if (entityId.ObjectClass == rxImage)
			{
				entities[rxEntity].Add(entityId);
				entities[rxImage].Add(entityId);
			}
			else if (entityId.ObjectClass == rxLeader)
			{
				entities[rxEntity].Add(entityId);
				entities[rxLeader].Add(entityId);
			}
			else if (entityId.ObjectClass == rxLight)
			{
				entities[rxEntity].Add(entityId);
				entities[rxLight].Add(entityId);
			}
			else if (entityId.ObjectClass == rxLine)
			{
				entities[rxEntity].Add(entityId);
				entities[rxLine].Add(entityId);
			}
			else if (entityId.ObjectClass == rxLineAngularDimension2)
			{
				entities[rxEntity].Add(entityId);
				entities[rxLineAngularDimension2].Add(entityId);
			}
			else if (entityId.ObjectClass == rxLoftedSurface)
			{
				entities[rxEntity].Add(entityId);
				entities[rxLoftedSurface].Add(entityId);
			}
			else if (entityId.ObjectClass == rxMInsertBlock)
			{
				entities[rxEntity].Add(entityId);
				entities[rxMInsertBlock].Add(entityId);
			}
			else if (entityId.ObjectClass == rxMLeader)
			{
				entities[rxEntity].Add(entityId);
				entities[rxMLeader].Add(entityId);
			}
			else if (entityId.ObjectClass == rxMline)
			{
				entities[rxEntity].Add(entityId);
				entities[rxMline].Add(entityId);
			}
			else if (entityId.ObjectClass == rxMText)
			{
				entities[rxEntity].Add(entityId);
				entities[rxMText].Add(entityId);
			}
			else if (entityId.ObjectClass == rxOle2Frame)
			{
				entities[rxEntity].Add(entityId);
				entities[rxOle2Frame].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPdfReference)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPdfReference].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPlaneSurface)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPlaneSurface].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPoint3AngularDimension)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPoint3AngularDimension].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPointCloud)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPointCloud].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPointCloudEx)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPointCloudEx].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPolyFaceMesh)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPolyFaceMesh].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPolyFaceMeshVertex)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPolyFaceMeshVertex].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPolygonMesh)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPolygonMesh].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPolygonMeshVertex)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPolygonMeshVertex].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPolyline)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPolyline].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPolyline2d)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPolyline2d].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPolyline3d)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPolyline3d].Add(entityId);
			}
			else if (entityId.ObjectClass == rxPolylineVertex3d)
			{
				entities[rxEntity].Add(entityId);
				entities[rxPolylineVertex3d].Add(entityId);
			}
			else if (entityId.ObjectClass == rxProxyEntity)
			{
				entities[rxEntity].Add(entityId);
				entities[rxProxyEntity].Add(entityId);
			}
			else if (entityId.ObjectClass == rxRadialDimension)
			{
				entities[rxEntity].Add(entityId);
				entities[rxRadialDimension].Add(entityId);
			}
			else if (entityId.ObjectClass == rxRadialDimensionLarge)
			{
				entities[rxEntity].Add(entityId);
				entities[rxRadialDimensionLarge].Add(entityId);
			}
			else if (entityId.ObjectClass == rxRay)
			{
				entities[rxEntity].Add(entityId);
				entities[rxRay].Add(entityId);
			}
			else if (entityId.ObjectClass == rxRasterImage)
			{
				entities[rxEntity].Add(entityId);
				entities[rxRasterImage].Add(entityId);
			}
			else if (entityId.ObjectClass == rxRegion)
			{
				entities[rxEntity].Add(entityId);
				entities[rxRegion].Add(entityId);
			}
			else if (entityId.ObjectClass == rxRotatedDimension)
			{
				entities[rxEntity].Add(entityId);
				entities[rxRotatedDimension].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSection)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSection].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSectionSymbol)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSectionSymbol].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSequenceEnd)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSequenceEnd].Add(entityId);
			}
			else if (entityId.ObjectClass == rxShape)
			{
				entities[rxEntity].Add(entityId);
				entities[rxShape].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSolid)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSolid].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSolid3d)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSolid3d].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSubDMesh)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSubDMesh].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSurface)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSurface].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSpline)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSpline].Add(entityId);
			}
			else if (entityId.ObjectClass == rxSweptSurface)
			{
				entities[rxEntity].Add(entityId);
				entities[rxSweptSurface].Add(entityId);
			}
			else if (entityId.ObjectClass == rxTable)
			{
				entities[rxEntity].Add(entityId);
				entities[rxTable].Add(entityId);
			}
			else if (entityId.ObjectClass == rxUnderlayReference)
			{
				entities[rxEntity].Add(entityId);
				entities[rxUnderlayReference].Add(entityId);
			}
			else if (entityId.ObjectClass == rxVertex)
			{
				entities[rxEntity].Add(entityId);
				entities[rxVertex].Add(entityId);
			}
			else if (entityId.ObjectClass == rxVertex2d)
			{
				entities[rxEntity].Add(entityId);
				entities[rxVertex2d].Add(entityId);
			}
			else if (entityId.ObjectClass == rxViewBorder)
			{
				entities[rxEntity].Add(entityId);
				entities[rxViewBorder].Add(entityId);
			}
			else if (entityId.ObjectClass == rxViewport)
			{
				entities[rxEntity].Add(entityId);
				entities[rxViewport].Add(entityId);
			}
			else if (entityId.ObjectClass == rxViewRepBlockReference)
			{
				entities[rxEntity].Add(entityId);
				entities[rxViewRepBlockReference].Add(entityId);
			}
			else if (entityId.ObjectClass == rxWipeout)
			{
				entities[rxEntity].Add(entityId);
				entities[rxWipeout].Add(entityId);
			}
			else if (entityId.ObjectClass == rxXline)
			{
				entities[rxEntity].Add(entityId);
				entities[rxXline].Add(entityId);
			}
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

		#endregion

		#region Instance Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="class"></param>
		/// <returns></returns>
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
		/// Database object appended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectAppended(object sender, ObjectEventArgs e)
		{
			ObjectId id = e.DBObject.ObjectId;
			AddId(entitiesField, id);
			ObjectAppended?.Invoke(this, new(id));
		}

		/// <summary>
		/// Database object erased handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
		{
			ObjectId id = e.DBObject.ObjectId;
			RemoveId(entitiesField, id);
			ObjectErased?.Invoke(this, new(id));
		}

		/// <summary>
		/// Database unappended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectUnappended(object sender, ObjectEventArgs e)
		{
			ObjectId id = e.DBObject.ObjectId;
			RemoveId(entitiesField, id);
			ObjectUnappended?.Invoke(this, new(id));
		}

		/// <summary>
		/// Database reappended handler to keep the dictionary fresh
		/// </summary>
		/// <param name="sender">Object that triggered the event.</param>
		/// <param name="e">Data on the object appended.</param>
		private void Database_ObjectReappended(object sender, ObjectEventArgs e)
		{
			ObjectId id = e.DBObject.ObjectId;
			AddId(entitiesField, id);
			ObjectReappended?.Invoke(this, new(id));
		}

		#endregion
	}
}
