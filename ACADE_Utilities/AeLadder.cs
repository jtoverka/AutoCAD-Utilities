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
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACADE_Utilities
{
	/// <summary>
	/// Represents a ladder wire diagram in AutoCAD Electrical.
	/// </summary>
	public class AeLadder
	{
		#region Fields

		private readonly ObjectId wdmIdField = ObjectId.Null;
		private readonly ObjectId wdmlrIdField = ObjectId.Null;

		private Point3d wdmlrPointField = Point3d.Origin;
		private double rungDist = 1;
		private int rungCount = 0;
		private double ladderWidth = 0;
		private bool horizontal = true;
		private string wireNumberFirst;

		private string pN = "";
		private string pL = "";
		private string pI = "";
		private string pP = "";
		private string pS = "";
		private string pD = "";
		private readonly string pG = "";
		private readonly string pX = "";
		private readonly string pA = "";
		private readonly string pB = "";

		private string wireFormatString = "%N";

		#endregion

		#region Properties

		/// <summary>
		/// Gets the ladder space id.
		/// </summary>
		public ObjectId SpaceId { get; } = ObjectId.Null;

		#endregion
		
		#region Constructors

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		public AeLadder(ObjectId wdmId, ObjectId wdmlrId)
		{
			wdmId.Validate(Sort.rxBlockReference, true);
			wdmlrId.Validate(Sort.rxBlockReference, true);

			if (wdmId.Database != wdmlrId.Database)
				throw new ArgumentException("The two ladder blocks do not reside in the same database.");

			wdmId.Database.Validate(true, true);

			bool started = wdmId.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			wdmIdField = wdmId;
			wdmlrIdField = wdmlrId;

			using BlockReference wdmlr = transaction.GetObject(wdmlrId, OpenMode.ForRead) as BlockReference;
			SpaceId = wdmlr.BlockId;

			Refresh();
		}
		
		#endregion

		#region Methods

		/// <summary>
		/// Gets the priority factor. 0 is the highest priority, null is invalid.
		/// The higher the number, the lower the priority.
		/// </summary>
		/// <param name="point">The wire number point.</param>
		/// <returns>The nullable double priority index.</returns>
		/// <remarks>
		/// The priority factor is used to determine which valid AeLadder to use 
		/// over another valid AeLadder.
		/// </remarks>
		public double? GetPriority(Point3d point)
		{
			if (!GetRungIndex(point).HasValue)
				return null;

			if (horizontal)
				return point.X - wdmlrPointField.X + rungDist / 2;
			else
				return point.Y - wdmlrPointField.Y + rungDist / 2;
		}

		/// <summary>
		/// Updates the field values for this object.
		/// </summary>
		public void Refresh()
		{
			bool started = wdmIdField.Database.GetOrStartTransaction(out Transaction transaction);
			using Disposable disposable = new(transaction, started);

			using AeXData aeXData = new();
			using BlockReference wdm = transaction.GetObject(wdmIdField, OpenMode.ForRead) as BlockReference;
			using BlockReference wdmlr = transaction.GetObject(wdmlrIdField, OpenMode.ForRead) as BlockReference;
			Dictionary<string, Attrib> wdm_Attributes = wdm.GetAttributes();
			Dictionary<string, Attrib> wdmlr_Attributes = wdmlr.GetAttributes();

			wdmlrPointField = wdmlr.Position;

			// Default values
			string ladderWidthString = "0";
			string rungFirstNumString = "0";
			string incrementString = "1";
			string rungDistString = "1";
			string rungCountString = "0";
			string rungOrientation = "H";

			//Search through the WD_M attributes and collect important information.

			if (wdm_Attributes.ContainsKey("DLADW"))
				ladderWidthString = wdm_Attributes["DLADW"].Text;

			if (wdm_Attributes.ContainsKey("IEC_LOC"))
				pL = wdm_Attributes["IEC_LOC"].Text;

			if (wdm_Attributes.ContainsKey("IEC_INST"))
				pI = wdm_Attributes["IEC_INST"].Text;

			if (wdm_Attributes.ContainsKey("IEC_PROJ"))
				pP = wdm_Attributes["IEC_PROJ"].Text;

			if (wdm_Attributes.ContainsKey("SHEET"))
				pS = wdm_Attributes["SHEET"].Text;

			if (wdm_Attributes.ContainsKey("SHEETDWGNAME"))
				pD = wdm_Attributes["SHEETDWGNAME"].Text;

			if (wdm_Attributes.ContainsKey("RUNGFIRST"))
				rungFirstNumString = wdm_Attributes["RUNGFIRST"].Text;
			if (wdmlr_Attributes.ContainsKey("RUNGFIRST"))
				rungFirstNumString = wdmlr_Attributes["RUNGFIRST"].Text;

			if (wdm_Attributes.ContainsKey("RUNGINC"))
				incrementString = wdm_Attributes["RUNGINC"].Text;
			if (wdmlr_Attributes.ContainsKey("RUNGINC"))
				incrementString = wdmlr_Attributes["RUNGINC"].Text;

			if (wdm_Attributes.ContainsKey("RUNGDIST"))
				rungDistString = wdm_Attributes["RUNGDIST"].Text;
			if (wdmlr_Attributes.ContainsKey("RUNGDIST"))
				rungDistString = wdmlr_Attributes["RUNGDIST"].Text;

			if (wdm_Attributes.ContainsKey("RUNGCNT"))
				rungCountString = wdm_Attributes["RUNGCNT"].Text;
			if (wdmlr_Attributes.ContainsKey("RUNGCNT"))
				rungCountString = wdmlr_Attributes["RUNGCNT"].Text;

			if (wdm_Attributes.ContainsKey("RUNGHORV"))
				rungOrientation = wdm_Attributes["RUNGHORV"].Text;
			if (wdmlr_Attributes.ContainsKey("RUNGHORV"))
				rungOrientation = wdmlr_Attributes["RUNGHORV"].Text;

			if (wdm_Attributes.ContainsKey("WIREFMT"))
				wireFormatString = wdm_Attributes["WIREFMT"].Text;
			if (wdmlr_Attributes.ContainsKey("WIREFMT"))
				wireFormatString = wdmlr_Attributes["WIREFMT"].Text;

			Double.TryParse(ladderWidthString, out ladderWidth);
			Double.TryParse(rungDistString, out rungDist);
			int.TryParse(rungCountString, out rungCount);
			horizontal = !rungOrientation.Equals("V", StringComparison.OrdinalIgnoreCase);
			wireNumberFirst = rungFirstNumString;
		}

		/// <summary>
		/// Gets the rung index based on insertion point.
		/// </summary>
		/// <param name="point">The wire number insert point.</param>
		/// <returns>Rung index if the point is inside the ladder; Otherwise, null.</returns>
		private int? GetRungIndex(Point3d point)
		{
			if (rungDist == 0)
				return null;

			// Get rung index depending on if it's horizontal or vertical
			int index;
			double distance;
			if (horizontal)
			{
				// Calculate ladder rung
				distance = wdmlrPointField.Y - point.Y + rungDist / 2;
				index = (int)(distance / rungDist);

				// Calculate position on rung
				distance = point.X - wdmlrPointField.X;
			}
			else
			{
				// Calculate ladder rung
				distance = wdmlrPointField.X - point.X + rungDist / 2;
				index = (int)(distance / rungDist);

				// Calculate position on rung
				distance = point.Y - wdmlrPointField.Y;
			}

			// return null if outside ladder bounds, otherwise return index
			if (distance < -rungDist / 2 || distance > ladderWidth)
				return null;
			else if (index < 0 || index > rungCount)
				return null;
			else
				return index;
		}

		/// <summary>
		/// Calculates the wire number based on index.
		/// </summary>
		/// <param name="index">Ladder rung index.</param>
		private void CalculateNumber(int index)
		{
			// separate alpha characters into character array (in integer form)
			static int[] AlphaDigits(int a)
			{
				if (a == 0) return new int[1] { 0 };

				var digits = new List<int>();

				for (; a != 0; a /= 27)
					digits.Add(a % 27);

				var arr = digits.ToArray();
				Array.Reverse(arr);
				return arr;
			}
			// separate integer into separate digits
			static int[] NumericDigits(int n)
			{
				if (n == 0) return new int[1] { 0 };

				var digits = new List<int>();

				for (; n != 0; n /= 10)
					digits.Add(n % 10);

				var arr = digits.ToArray();
				Array.Reverse(arr);
				return arr;
			}
			// check if character is a number
			static bool Number(char character)
			{
				return (character >= '0' && character <= '9');
			}
			// check if a character is a lower case letter
			static bool LowerLetter(char character)
			{
				return (character >= 'a' && character <= 'z');
			}
			// check if a character is an upper case letter
			static bool UpperLetter(char character)
			{
				return (character >= 'A' && character <= 'Z');
			}

			// Get first wire number, split and reverse
			char[] characters = wireNumberFirst.Reverse().ToArray();

			// Get addition, split and reverse
			int[] addition = NumericDigits(index).Reverse().ToArray();

			// Overflow in case numbers add up and overflow
			int overflow = 0;

			bool numeric;
			char offset;
			int add;

			for (int i = 0; i <= addition.Length; i++)
			{
				char character = characters[i];

				if (i == addition.Length)
					add = 0;
				else
					add = addition[i];

				if (Number(character))
				{
					offset = '0';
					numeric = true;
				}
				else if (LowerLetter(character))
				{
					offset = 'a';
					numeric = false;
				}
				else if (UpperLetter(character))
				{
					offset = 'A';
					numeric = false;
				}
				else
					continue;

				// convert character to number and add number and overflow
				add = (int)(character - offset) + add + overflow;

				int[] added;
				if (numeric)
					added = NumericDigits(add);
				else
					added = AlphaDigits(add);

				// Update character
				character = (char)(offset + added[0]);

				// Get overflow if it exists
				if (added.Length != 1)
					overflow = added[1];

				// Update character
				characters[i] = character;
			}

			// Put value into pN field
			pN = new string(characters.Reverse().ToArray());
		}

		/// <summary>
		/// Generates the wirenumber based on insertion point.
		/// </summary>
		/// <param name="point">Insertion point.</param>
		/// <returns>The string value of the wire number.</returns>
		public string GetWireNumber(Point3d point)
		{
			string noWireNumberFound = "???";

			int? index = GetRungIndex(point);

			if (!index.HasValue)
				return noWireNumberFound;

			CalculateNumber(index.Value);

			// Build wire number
			string wireNumber = wireFormatString;

			wireNumber = wireNumber.Replace("%S", pS);
			wireNumber = wireNumber.Replace("%s", pS);

			wireNumber = wireNumber.Replace("%D", pD);
			wireNumber = wireNumber.Replace("%d", pD);

			wireNumber = wireNumber.Replace("%G", pG);
			wireNumber = wireNumber.Replace("%g", pG);

			wireNumber = wireNumber.Replace("%N", pN);
			wireNumber = wireNumber.Replace("%n", pN);

			wireNumber = wireNumber.Replace("%X", pX);
			wireNumber = wireNumber.Replace("%x", pX);

			wireNumber = wireNumber.Replace("%P", pP);
			wireNumber = wireNumber.Replace("%p", pP);

			wireNumber = wireNumber.Replace("%I", pI);
			wireNumber = wireNumber.Replace("%i", pI);

			wireNumber = wireNumber.Replace("%L", pL);
			wireNumber = wireNumber.Replace("%l", pL);

			wireNumber = wireNumber.Replace("%A", pA);
			wireNumber = wireNumber.Replace("%a", pA);

			wireNumber = wireNumber.Replace("%B", pB);
			wireNumber = wireNumber.Replace("%b", pB);

			return wireNumber;
		}

		#endregion
	}
}
