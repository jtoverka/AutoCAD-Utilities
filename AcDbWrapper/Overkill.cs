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
using System.ComponentModel;

namespace AcDbWrapper
{
	/// <summary>
	/// Represents a database wrapper to implement overkill.
	/// </summary>
	public class Overkill : INotifyPropertyChanged
	{
		#region Fields

		// flag for error checks.
		private bool check = true;

		private readonly Database databaseField = null;

		// Dictionary to sort out buckets of entities by type.
		private readonly Sort sortField = null;

		// Property field defaults.
		private bool ignoreColorField = true;
		private bool ignoreLayerField = true;
		private bool ignoreLinetypeField = true;
		private bool ignoreLinetypeScaleField = true;
		private bool ignoreLineweightField = true;
		private bool ignoreThicknessField = true;
		private bool ignoreTransparencyField = true;
		private bool ignorePlotStyleField = true;
		private bool ignoreMaterialField = true;

		// Tolerance
		private double toleranceField = 0.0000001;

		// Options
		private bool optimizePolylineSegmentsField = true;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the flag to ignore colors of duplicate objects.
		/// </summary>
		public bool IgnoreColor
		{
			get { return ignoreColorField; }
			set
			{
				if (ignoreColorField == value)
					return;

				ignoreColorField = value;
				OnPropertyChanged(this, new(nameof(IgnoreColor)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to ignore layers of duplicate objects.
		/// </summary>
		public bool IgnoreLayer
		{
			get { return ignoreLayerField; }
			set
			{
				if (ignoreLayerField == value)
					return;

				ignoreLayerField = value;
				OnPropertyChanged(this, new(nameof(IgnoreLayer)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to ignore linetypes of duplicate objects.
		/// </summary>
		public bool IgnoreLinetype
		{
			get { return ignoreLinetypeField; }
			set
			{
				if (ignoreLinetypeField == value)
					return;

				ignoreLinetypeField = value;
				OnPropertyChanged(this, new(nameof(IgnoreLinetype)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to ignore linetype scales of duplicate objects.
		/// </summary>
		public bool IgnoreLinetypeScale
		{
			get { return ignoreLinetypeScaleField; }
			set
			{
				if (ignoreLinetypeScaleField == value)
					return;

				ignoreLinetypeScaleField = value;
				OnPropertyChanged(this, new(nameof(IgnoreLinetypeScale)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to ignore lineweights of duplicate objects.
		/// </summary>
		public bool IgnoreLineweight
		{
			get { return ignoreLineweightField; }
			set
			{
				if (ignoreLineweightField == value)
					return;

				ignoreLineweightField = value;
				OnPropertyChanged(this, new(nameof(IgnoreLineweight)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to ignore thickness of duplicate objects.
		/// </summary>
		public bool IgnoreThickness
		{
			get { return ignoreThicknessField; }
			set
			{
				if (ignoreThicknessField == value)
					return;

				ignoreThicknessField = value;
				OnPropertyChanged(this, new(nameof(IgnoreThickness)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to ignore transparency of duplicate objects.
		/// </summary>
		public bool IgnoreTransparency
		{
			get { return ignoreTransparencyField; }
			set
			{
				if (ignoreTransparencyField == value)
					return;

				ignoreTransparencyField = value;
				OnPropertyChanged(this, new(nameof(IgnoreTransparency)));
			}
		}
		/// <summary>
		/// Gets or sets the flag to ignore plot styles of duplicate objects.
		/// </summary>
		public bool IgnorePlotStyle
		{
			get { return ignorePlotStyleField; }
			set
			{
				if (ignorePlotStyleField == value)
					return;

				ignorePlotStyleField = value;
				OnPropertyChanged(this, new(nameof(IgnorePlotStyle)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to ignore material of duplicate objects.
		/// </summary>
		public bool IgnoreMaterial
		{
			get { return ignoreMaterialField; }
			set
			{
				if (ignoreMaterialField == value)
					return;

				ignoreMaterialField = value;
				OnPropertyChanged(this, new(nameof(IgnoreMaterial)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to remove redundant segments within polylines.
		/// </summary>
		public bool OptimizePolylineSegments
		{
			get { return optimizePolylineSegmentsField; }
			set
			{
				if (optimizePolylineSegmentsField == value)
					return;

				optimizePolylineSegmentsField = value;
				OnPropertyChanged(this, new(nameof(OptimizePolylineSegments)));
			}
		}

		/// <summary>
		/// Gets or sets the measurement tolerance to determine a match.
		/// </summary>
		public double Tolerance
		{
			get { return toleranceField; }
			set
			{
				value = Math.Abs(value);

				if (toleranceField == value)
					return;

				toleranceField = value;
				OnPropertyChanged(this, new(nameof(Tolerance)));
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Create a new instance of this class.
		/// </summary>
		/// <param name="database"></param>
		public Overkill(Database database)
		{
			if (check)
			{
				database.Validate(true);
			}

			databaseField = database;

			sortField = Sort.GetSort(database);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Invoke property change event.
		/// </summary>
		/// <param name="sender">The object that invoked the event.</param>
		/// <param name="e">The property that changed.</param>
		protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(sender, e);
		}

		/// <summary>
		/// Find if the two entities have matching properties.
		/// </summary>
		/// <param name="entity1Id">First ObjectId entity to match.</param>
		/// <param name="entity2Id">Second ObjectId entity to match.</param>
		/// <returns></returns>
		protected virtual bool MatchObjectProperties(ObjectId entity1Id, ObjectId entity2Id)
		{
			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			using Entity entity1 = transaction.GetObject(entity1Id, OpenMode.ForRead) as Entity;
			using Entity entity2 = transaction.GetObject(entity2Id, OpenMode.ForRead) as Entity;

			bool match = true;

			if (!IgnoreColor)
				match = match && entity1.Color == entity2.Color;
			if (!IgnoreLayer)
				match = match && entity1.LayerId == entity2.LayerId;
			if (!IgnoreLinetype)
				match = match && entity1.LinetypeId == entity2.LinetypeId;
			if (!IgnoreLinetypeScale)
				match = match && Math.Abs(entity1.LinetypeScale - entity2.LinetypeScale) <= Tolerance;
			if (!IgnoreLineweight)
				match = match && entity1.LineWeight == entity2.LineWeight;
			if (!IgnoreTransparency)
				match = match && entity1.Transparency == entity2.Transparency;
			if (!IgnorePlotStyle)
				match = match && entity1.PlotStyleNameId == entity2.PlotStyleNameId;
			if (!IgnoreMaterial)
				match = match && entity1.MaterialId == entity2.MaterialId;

			return match;
		}

		/// <summary>
		/// Remove duplicate lines.
		/// </summary>
		public virtual void Overkill_Line()
		{
			if (check)
			{
				databaseField.Validate(false);
			}
			if (!sortField.ContainsKey(Sort.rxLine))
				return;

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			HashSet<ObjectId> lineIds = sortField[Sort.rxLine];
			HashSet<ObjectId> usedIds = new();

			foreach (ObjectId line1Id in lineIds)
			{
				usedIds.Add(line1Id);

				if (!line1Id.Validate(false))
					continue;

				using Line line1 = transaction.GetObject(line1Id, OpenMode.ForRead) as Line;
				
				if (line1.Length < Tolerance)
				{
					line1.UpgradeOpen();
					line1.Erase();
					continue;
				}

				foreach (ObjectId line2Id in lineIds)
				{
					if (usedIds.Contains(line1Id))
						continue;

					if (!line2Id.Validate(false))
						continue;

					using Line line2 = transaction.GetObject(line2Id, OpenMode.ForRead) as Line;

					if (line1.BlockId != line2.BlockId)
						continue;

					bool match = (line2.StartPoint.DistanceTo(line1.StartPoint) < Tolerance
							  && line2.EndPoint.DistanceTo(line1.EndPoint) < Tolerance)
							  || (line2.EndPoint.DistanceTo(line1.StartPoint) < Tolerance
							  && line2.StartPoint.DistanceTo(line1.EndPoint) < Tolerance);

					match = match && MatchObjectProperties(line1Id, line2Id);

					if (!IgnoreThickness)
						match = match && Math.Abs(line1.Thickness - line2.Thickness) <= Tolerance;
					
					if (match)
					{
						line2.UpgradeOpen();
						line2.Erase();
					}
				}
			}
		}

		/// <summary>
		/// Remove duplicate polylines.
		/// </summary>
		public virtual void Overkill_PolyLine()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			if (!sortField.ContainsKey(Sort.rxPolyline))
				return;

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			HashSet<ObjectId> lineIds = sortField[Sort.rxPolyline];
			HashSet<ObjectId> usedIds = new();

			foreach (ObjectId line1Id in lineIds)
			{
				usedIds.Add(line1Id);

				if (!line1Id.Validate(false))
					continue;

				using Polyline line1 = transaction.GetObject(line1Id, OpenMode.ForRead) as Polyline;

				if (line1.Length < Tolerance)
				{
					line1.UpgradeOpen();
					line1.Erase();
					continue;
				}
			}
		}

		/// <summary>
		/// Remove duplicate polyline2d objects.
		/// </summary>
		public virtual void Overkill_PolyLine2d()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate polyline3d objects.
		/// </summary>
		public virtual void Overkill_PolyLine3d()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate block references.
		/// </summary>
		public virtual void Overkill_BlockReference()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate attribute definitions.
		/// </summary>
		public virtual void Overkill_AttributeDefinition()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate attribute references.
		/// </summary>
		public virtual void Overkill_AttributeReference()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate text.
		/// </summary>
		public virtual void Overkill_DBText()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate MText.
		/// </summary>
		public virtual void Overkill_MText()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate Circles.
		/// </summary>
		public virtual void Overkill_Circle()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			HashSet<ObjectId> circleIds = sortField[Sort.rxCircle];
			HashSet<ObjectId> usedIds = new();
			foreach (ObjectId circle1Id in circleIds)
			{
				usedIds.Add(circle1Id);

				if (!circle1Id.Validate(false))
					continue;

				using Circle circle1 = transaction.GetObject(circle1Id, OpenMode.ForRead) as Circle;

				if (circle1.Diameter < Tolerance)
				{
					circle1.UpgradeOpen();
					circle1.Erase();
					continue;
				}

				foreach (ObjectId circle2Id in circleIds)
				{
					if (usedIds.Contains(circle2Id))
						continue;

					using Circle circle2 = transaction.GetObject(circle2Id, OpenMode.ForRead) as Circle;

					bool match = MatchObjectProperties(circle1Id, circle2Id);

					if (!IgnoreThickness)
						match = match && Math.Abs(circle1.Thickness - circle2.Thickness) < Tolerance;

					if (!match)
						continue;

					match = match && circle1.Center.DistanceTo(circle2.Center) < Tolerance
						 && Math.Abs(circle1.Diameter - circle2.Diameter) < Tolerance;

					if (match)
					{
						circle1.UpgradeOpen();
						circle1.Erase();
					}
				}
			}
		}

		/// <summary>
		/// Remove duplicates of all entities.
		/// </summary>
		public virtual void Overkill_All()
		{
			if (check)
			{
				databaseField.Validate(false);
			}

			bool started = databaseField.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			check = false;

			try
			{
				Overkill_Line();
				Overkill_PolyLine();
				Overkill_PolyLine2d();
				Overkill_PolyLine3d();
				Overkill_BlockReference();
				Overkill_AttributeDefinition();
				Overkill_AttributeReference();
				Overkill_DBText();
				Overkill_MText();
				Overkill_Circle();
			}
			catch
			{
				throw;
			}
			finally
			{
				check = true;
			}
		}

		#endregion

		#region Delegates, Events, Handlers

		/// <summary>
		/// Invoked on property changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
