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

namespace ACAD_Utilities
{
	/// <summary>
	/// Provides extended methods to work with XData.
	/// </summary>
	public static class XDataLibrary
	{
		#region Static Fields

		private static string attributePrefixField = string.Empty;
		private static string attributeSuffixField = string.Empty;

		#endregion

		#region Static Properties

		/// <summary>
		/// Gets or sets the xdata attribute prefix.
		/// </summary>
		public static string AttributePrefix
		{
			get { return attributePrefixField; }
			set
			{
				if (value == null)
					value = string.Empty;

				attributePrefixField = value;
			}
		}

		/// <summary>
		/// Gets or sets the xdata attribute suffix.
		/// </summary>
		public static string AttributeSuffix
		{
			get { return attributeSuffixField; }
			set
			{
				if (value == null)
					value = string.Empty;

				attributeSuffixField = value;
			}
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Converts an XData registered application to an attribute tag.
		/// </summary>
		/// <param name="value">Registered application name.</param>
		/// <returns>Attribute tagstring.</returns>
		private static string XDataToAttributeTag(string value)
		{
			if (value == null)
				return string.Empty;

			if (value.Equals(string.Empty))
				return value;

			if (!attributePrefixField.Equals(string.Empty)
			 && value.StartsWith(attributePrefixField))
			{
				value = value.Remove(0, attributePrefixField.Length);
			}

			if (!attributeSuffixField.Equals(string.Empty)
			 && value.EndsWith(attributeSuffixField))
			{
				value = value.Remove(value.LastIndexOf(attributeSuffixField), value.Length);
			}

			return value;
		}

		/// <summary>
		/// Converts an attribute tagstring to an XData registered application.
		/// </summary>
		/// <param name="value">Attribute tagstring.</param>
		/// <returns>XData registered application value.</returns>
		private static string AttributeTagToXData(string value)
		{
			if (value == null)
				value = string.Empty;

			return attributePrefixField + value + attributeSuffixField;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dBObject"></param>
		/// <returns></returns>
		public static Dictionary<string, Attrib> GetXData(this DBObject dBObject)
		{
			Dictionary<string, Attrib> attributes = new();

			// Get xdata
			using ResultBuffer xdata = dBObject.XData;
			if (xdata != null)
			{
				string application = null;
				foreach (TypedValue typedValue in xdata)
				{
					// Registered Application
					if (typedValue.TypeCode == 1001)
					{
						string value = typedValue.Value.ToString();

						application = XDataToAttributeTag(value);
					}
						

					// Only accept strings or handles
					if ((typedValue.TypeCode == 1000
						|| typedValue.TypeCode == 1005)
						&& application != null)
					{
						string value = typedValue.Value.ToString();
						attributes.Add(application, new(typedValue.TypeCode, value));
						application = null;
					}
				}
			}

			return attributes;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="dBObject"></param>
		/// <param name="attributes"></param>
		public static void SetXData(this Transaction transaction, DBObject dBObject, Dictionary<string, Attrib> attributes)
		{
			// Make sure the block reference is able to be written
			dBObject.UpgradeOpen();
			Database database = dBObject.Database;

			// The attributes that have not been output yet will be written to the block reference as XData
			using ResultBuffer buffer = new();
			using ResultBuffer xdata = dBObject.XData;
			string tag = null;

			// XData that has not been output yet
			Dictionary<string, Attrib> overflowXData = new(attributes);

			// Overwrite existing XData 'attributes'
			if (xdata != null)
			{
				foreach (TypedValue typedValue in xdata)
				{
					if (typedValue.TypeCode == 1001)
					{
						string value = typedValue.Value.ToString();
						tag = XDataToAttributeTag(value);
						buffer.Add(typedValue);
					}
					if ((typedValue.TypeCode == 1000
						|| typedValue.TypeCode == 1005)
						&& tag != null)
					{
						TypedValue value;

						if (attributes.ContainsKey(tag))
						{
							overflowXData.Remove(tag);
							value = new TypedValue(attributes[tag].Code, attributes[tag].Text);
						}
						else
						{
							value = typedValue;
						}

						tag = null;
						buffer.Add(value);
					}
					else
					{
						buffer.Add(typedValue);
					}
				}
			}

			using RegAppTable table = transaction.GetObject(database.RegAppTableId, OpenMode.ForWrite) as RegAppTable;

			// If attribute data does not have an attribute or existing XData to write to, create new XData
			foreach (KeyValuePair<string, Attrib> attribute in overflowXData)
			{
				string application = AttributeTagToXData(attribute.Key);

				AddRegAppTableRecord(transaction, database, application);

				buffer.Add(new TypedValue(1001, application));
				buffer.Add(new TypedValue(1002, "{"));
				buffer.Add(new TypedValue(attribute.Value.Code, attribute.Value.Text));
				buffer.Add(new TypedValue(1002, "}"));
			}

			dBObject.XData = buffer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="db"></param>
		/// <param name="name"></param>
		public static void AddRegAppTableRecord(Transaction transaction, Database db, string name)
		{
			using RegAppTable regAppTable = transaction.GetObject(db.RegAppTableId, OpenMode.ForRead) as RegAppTable;

			if (!regAppTable.Has(name))
			{
				regAppTable.UpgradeOpen();
				using RegAppTableRecord regAppTableRecord = new()
				{
					Name = name
				};
				regAppTable.Add(regAppTableRecord);
				transaction.AddNewlyCreatedDBObject(regAppTableRecord, true);
			}
		}

		#endregion
	}
}
