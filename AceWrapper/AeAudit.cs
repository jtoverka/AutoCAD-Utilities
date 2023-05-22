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
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace AceWrapper
{
	/// <summary>
	/// Represents a container for the purpose of Auditing AutoCAD Electrical drawings.
	/// </summary>
	public sealed class AeAudit : INotifyPropertyChanged
	{
		#region Fields

		private readonly AeDrawing aeDrawing;
		private bool wireGapPointerField = true;
		private bool bogusWireNumberField = true;
		private bool zeroLengthWiresField = true;
		private bool wireNumberFloaterField = true;
		private double toleranceField = 0.025;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the flag to find problems related to missing wires which 
		/// were connected through gap pointers.
		/// </summary>
		public bool WireGap
		{
			get { return wireGapPointerField; }
			set
			{
				if (wireGapPointerField == value)
					return;

				wireGapPointerField = value;
				PropertyChanged?.Invoke(this, new(nameof(WireGap)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to find and erase wires pointing to nonexistant
		/// wire numbers.
		/// </summary>
		public bool BogusWireNumber
		{
			get { return bogusWireNumberField; }
			set
			{
				if (bogusWireNumberField == value)
					return;

				bogusWireNumberField = value;
				PropertyChanged?.Invoke(this, new(nameof(BogusWireNumber)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to find and erase zero length line entities
		/// on the wire layer.
		/// </summary>
		public bool ZeroLengthWires
		{
			get { return zeroLengthWiresField; }
			set
			{
				if (zeroLengthWiresField == value)
					return;

				zeroLengthWiresField = value;
				PropertyChanged?.Invoke(this, new(nameof(ZeroLengthWires)));
			}
		}

		/// <summary>
		/// Gets or sets the flag to look for and erase wire numbers that are not
		/// linked to a wire network.
		/// </summary>
		public bool WireNumberFloater
		{
			get { return wireNumberFloaterField; }
			set
			{
				if (wireNumberFloaterField == value)
					return;

				wireNumberFloaterField = value;
				PropertyChanged?.Invoke(this, new(nameof(WireNumberFloater)));
			}
		}

		/// <summary>
		/// Gets or sets the distance tolerance between wire number blocks and wire lines.
		/// </summary>
		public double Tolerance
		{
			get { return toleranceField; }
			set
			{
				if (toleranceField == value)
					return;

				toleranceField = value;
				PropertyChanged?.Invoke(this, new(nameof(Tolerance)));
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initialize a new instance of this class.
		/// </summary>
		/// <param name="database">The database to do an electrical audit.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ObjectDisposedException"/>
		public AeAudit(Database database)
		{
			database.Validate(true);
			aeDrawing = AeDrawing.GetOrCreate(database);
		}

		/// <summary>
		/// Initialize a new instance of this class.
		/// </summary>
		/// <param name="aeDrawing">The drawing to do an electrical audit.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="ObjectDisposedException"/>
		public AeAudit(AeDrawing aeDrawing)
		{
			this.aeDrawing = aeDrawing;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Performs a drawing wide audit.
		/// </summary>
		public void Audit()
		{
			bool started = aeDrawing.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			aeDrawing.Refresh();

			ObjectId[] ids = aeDrawing[AeObject.Wire].ToArray();
			for (int i = 0; i < ids.Length; i++)
			{
				ObjectId lineId = ids[i];

				Attributes lineData = AeXData.Default();
				lineData.GetAttributes(lineId);

				bool hasWnptr = lineData.TryGetValue("WNPTR", out DataPoint value);
				string wnptr = value?.Text ?? string.Empty;
				ObjectId wirenoId = ObjectId.Null;

				if (long.TryParse(wnptr, NumberStyles.HexNumber, null, out long handleId))
				{
					Handle handle = new(handleId);
                    lineId.Database.TryGetObjectId(handle, out wirenoId);
				}

                using Line line = transaction.GetObject(lineId, OpenMode.ForRead) as Line;
                if (ZeroLengthWires && line.Length == 0)
				{
					line.UpgradeOpen();
					line.Erase();
				}

				if (BogusWireNumber && hasWnptr && !wirenoId.Validate(false))
				{
					using ResultBuffer buffer = new(new TypedValue(1001, "VIA_WD_WNPTR"));
					line.UpgradeOpen();
					line.XData = buffer;
				}
				/*
				if (WireNumberFloater && hasWnptr && wirenoId.Validate(false))
				{
					using BlockReference wireno = transaction.GetObject(wirenoId, OpenMode.ForRead) as BlockReference;

					if (!GeometricLibrary.LineMember(wireno.Position, line.StartPoint, line.EndPoint, Tolerance))
					{

					}
				}
				*/
			}
			/*
			ids = aeDrawing[AeObject.Wireno].ToArray();
			for (int i = 0; i < ids.Length; i++)
			{
				ObjectId id = ids[i];

				using BlockReference wireno = transaction.GetObject(id, OpenMode.ForRead) as BlockReference;

			}
			*/
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
