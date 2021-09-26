using ACAD_Utilities;
using System;

namespace ACADE_Utilities
{
	/// <summary>
	/// Provides an initilizer for AutoCAD Electrical XData library.
	/// </summary>
	public class AeXData : IDisposable
	{
		/// <summary>
		/// Old XDataLibrary attribute prefix.
		/// </summary>
		private readonly string oldPrefixField;

		/// <summary>
		/// Old XDataLibrary attribute suffix.
		/// </summary>
		private readonly string oldSuffixField;

		/// <summary>
		/// AutoCAD Electrical attribute prefix.
		/// </summary>
		public static string Prefix { get; } = "VIA_WD_";

		/// <summary>
		/// AutoCAD Electrical attribute suffix.
		/// </summary>
		public static string Suffix { get; } = string.Empty;

		/// <summary>
		/// Initializes XDataLibrary for use with AutoCAD Electrical.
		/// </summary>
		public AeXData()
		{
			oldPrefixField = XDataLibrary.AttributePrefix;
			oldSuffixField = XDataLibrary.AttributeSuffix;
			XDataLibrary.AttributePrefix = Prefix;
			XDataLibrary.AttributeSuffix = Suffix;
		}

		/// <summary>
		/// Reverts the Attribute prefix and suffix for XData.
		/// </summary>
		public void Dispose()
		{
			XDataLibrary.AttributePrefix = oldPrefixField;
			XDataLibrary.AttributeSuffix = oldSuffixField;
		}
	}
}
