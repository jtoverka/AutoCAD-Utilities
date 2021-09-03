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
using System.ComponentModel;

namespace ACAD_Utilities
{
	/// <summary>
	/// Represents a .NET database container to implement overkill.
	/// </summary>
	public class Overkill : INotifyPropertyChanged
	{
		#region Fields

		// flag for error checks.
		private bool check = true;

		private readonly Transaction transactionField = null;
		private readonly Database databaseField = null;

		// Dictionary to sort out buckets of entities by type.
		private readonly Sort sortField = null;

		// Property field defaults.
		private bool removeZeroLengthLinesField = true;
		private bool removeZeroDiameterCirclesField = true;
		private double toleranceField = 0.0000001;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the flag to remove zero length lines.
		/// </summary>
		public bool RemoveZeroLengthLines
		{
			get { return removeZeroLengthLinesField; }
			set
			{
				if (removeZeroLengthLinesField == value)
					return;

				removeZeroLengthLinesField = value;
				OnPropertyChanged(this, new(nameof(RemoveZeroLengthLines)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to remove zero diameter circles.
		/// </summary>
		public bool RemoveZeroDiameterCircles
		{
			get { return removeZeroDiameterCirclesField; }
			set
			{
				if (removeZeroLengthLinesField == value)
					return;

				removeZeroDiameterCirclesField = value;
				OnPropertyChanged(this, new(nameof(RemoveZeroDiameterCircles)));
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
		/// <param name="transaction"></param>
		/// <param name="database"></param>
		public Overkill(Transaction transaction, Database database)
		{
			if (check)
			{
				database.Validate(true);
				transaction.Validate(true);
			}

			databaseField = database;
			transactionField = transaction;

			sortField = new(transaction, database);
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
		/// Remove duplicate lines.
		/// </summary>
		public void Overkill_Line()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			if (!sortField.Entities.ContainsKey(Sort.rxLine))
				return;

			foreach (ObjectId lineId in sortField.Entities[Sort.rxLine])
			{
				if (!lineId.Validate(false))
					continue;

				using Line line = transactionField.GetObject(lineId, OpenMode.ForRead) as Line;
				if (RemoveZeroLengthLines)
				{
					if (line.Length < Tolerance)
						line.Erase();
				}
			}
		}

		/// <summary>
		/// Remove duplicate polylines.
		/// </summary>
		public void Overkill_PolyLine()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate polyline2d objects.
		/// </summary>
		public void Overkill_PolyLine2d()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate polyline3d objects.
		/// </summary>
		public void Overkill_PolyLine3d()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate block references.
		/// </summary>
		public void Overkill_BlockReference()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate attribute definitions.
		/// </summary>
		public void Overkill_AttributeDefinition()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate attribute references.
		/// </summary>
		public void Overkill_AttributeReference()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate text.
		/// </summary>
		public void Overkill_DBText()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate MText.
		/// </summary>
		public void Overkill_MText()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicate Circles.
		/// </summary>
		public void Overkill_Circle()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}
			// Insert additional code here
		}

		/// <summary>
		/// Remove duplicates of all entities.
		/// </summary>
		public void Overkill_All()
		{
			if (check)
			{
				databaseField.Validate(false);
				transactionField.Validate(false);
			}

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
