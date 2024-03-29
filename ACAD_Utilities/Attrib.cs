﻿/*This is free and unencumbered software released into the public domain.
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

using System;
using System.ComponentModel;

namespace ACAD_Utilities
{
	/// <summary>
	/// Represents a container of data for an AutoCAD attribute / XData.
	/// </summary>
	[Serializable]
	public sealed class Attrib : INotifyPropertyChanged
	{
		#region Fields

		private string textField = "";
		private int codeField = 1000;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the text value to be used for either xdata or attribute data.
		/// </summary>
		public string Text
		{
			get { return textField; }
			set
			{
				if (textField == value)
					return;

				textField = value;
				PropertyChanged?.Invoke(this, new(nameof(Text)));
			}
		}

		/// <summary>
		/// Gets or sets the xdata code to be used in the event the <see cref="Text"/>
		/// property is used for xdata.
		/// </summary>
		public int Code
		{
			get { return codeField; }
			set
			{
				if (codeField == value)
					return;

				codeField = value;
				PropertyChanged?.Invoke(this, new(nameof(Code)));
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		public Attrib() { }

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="value">Text value.</param>
		public Attrib(string value)
		{
			textField = value;
		}

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="code">XData code.</param>
		/// <param name="value">Text value.</param>
		public Attrib(int code, string value)
		{
			codeField = code;
			textField = value;
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Determines whether this instance and a specified object, have the same value.
		/// </summary>
		/// <param name="obj">The object to compare in this instance.</param>
		/// <returns>Returns <see langword="true"/> if the object is equivalent to the specified object; Otherwise, <see langword="false"/>.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() == typeof(string))
			{
				return textField.Equals(obj) && Code == 1000;
			}
			if (obj.GetType() == typeof(Attrib))
			{
				Attrib class1 = (Attrib)obj;
				return class1.Text == Text && class1.Code == Code;
			}

			return obj.ToString().Equals(textField) && Code == 1000;
		}

		/// <summary>
		/// Returns a hash code for this object.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return codeField + " : " + textField;
		}

		#endregion

		#region Operators

		/// <summary>
		/// Casts a string to an <see cref="Attrib"/> object.
		/// </summary>
		/// <param name="text">The string to cast.</param>
		public static implicit operator Attrib(string text)
		{
			return new(text);
		}

		/// <summary>
		/// Casts an <see cref="Attrib"/> object to a string.
		/// </summary>
		/// <param name="attrib">The <see cref="Attrib"/> object to cast.</param>
		public static explicit operator string(Attrib attrib)
		{
			return attrib.Text;
		}

		/// <summary>
		/// Determines if one <see cref="Attrib"/> object is equivalent to another
		/// <see cref="Attrib"/> object.
		/// </summary>
		/// <param name="class1">The first <see cref="Attrib"/> object to compare.</param>
		/// <param name="class2">The second <see cref="Attrib"/> object to compare.</param>
		/// <returns><see langword="true"/> if the two objects are equivalent; Otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(Attrib class1, Attrib class2)
		{
			if (class1 is null && class2 is null)
				return true;
			else if (class1 is null ^ class2 is null)
				return false;

			return class1.Equals(class2);
		}

		/// <summary>
		/// Determines if one <see cref="Attrib"/> object is equivalent to a
		/// <see cref="string"/> object.
		/// </summary>
		/// <param name="class1">The <see cref="Attrib"/> object to compare.</param>
		/// <param name="class2">The <see cref="string"/> object to compare.</param>
		/// <returns><see langword="true"/> if the two objects are equivalent, Otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(Attrib class1, string class2)
		{
			if (class1 is null && class2 is null)
				return true;
			else if (class1 is null ^ class2 is null)
				return false;

			return class1.Equals(class2);
		}

		/// <summary>
		/// Determines if one <see cref="string"/> object is equivalent to a
		/// <see cref="Attrib"/> object.
		/// </summary>
		/// <param name="class1">The <see cref="string"/> object to compare.</param>
		/// <param name="class2">The <see cref="Attrib"/> object to compare.</param>
		/// <returns><see langword="true"/> if the two objects are equivalent; Otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(string class1, Attrib class2)
		{
			if (class1 is null && class2 is null)
				return true;
			else if (class1 is null ^ class2 is null)
				return false;

			return class1.Equals(class2);
		}

		/// <summary>
		/// Determines if one <see cref="Attrib"/> object is not equivalent to another
		/// <see cref="Attrib"/> object.
		/// </summary>
		/// <param name="class1">The first <see cref="Attrib"/> object to compare.</param>
		/// <param name="class2">The second <see cref="Attrib"/> object to compare.</param>
		/// <returns><see langword="true"/> if the two objects are not equivalent; Otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(Attrib class1, Attrib class2)
		{
			return !(class1 == class2);
		}

		/// <summary>
		/// Determines if one <see cref="Attrib"/> object is not equivalent to a
		/// <see cref="string"/> object.
		/// </summary>
		/// <param name="class1">The <see cref="Attrib"/> object to compare.</param>
		/// <param name="class2">The <see cref="string"/> object to compare.</param>
		/// <returns><see langword="true"/> if the two objects are not equivalent, Otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(Attrib class1, string class2)
		{
			return !(class1 == class2);
		}

		/// <summary>
		/// Determines if one <see cref="string"/> object is not equivalent to a
		/// <see cref="Attrib"/> object.
		/// </summary>
		/// <param name="class1">The <see cref="string"/> object to compare.</param>
		/// <param name="class2">The <see cref="Attrib"/> object to compare.</param>
		/// <returns><see langword="true"/> if the two objects are not equivalent; Otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(string class1, Attrib class2)
		{
			return !(class1 == class2);
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
