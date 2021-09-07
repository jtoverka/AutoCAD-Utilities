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
		/// <returns>Returns <c>true</c> if the object is equivalent to the specified object; otherwise, false.</returns>
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
		/// <returns><c>true</c> if the two objects are equivalent; Otherwise, <c>false</c>.</returns>
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
		/// <returns><c>true</c> if the two objects are equivalent, Otherwise, <c>false</c>.</returns>
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
		/// <returns><c>true</c> if the two objects are equivalent; Otherwise, <c>false</c>.</returns>
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
		/// <returns><c>true</c> if the two objects are not equivalent; Otherwise, <c>false</c>.</returns>
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
		/// <returns><c>true</c> if the two objects are not equivalent, Otherwise, <c>false</c>.</returns>
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
		/// <returns><c>true</c> if the two objects are not equivalent; Otherwise, false.</returns>
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
