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
using System.Xml.Serialization;

namespace ACADE_Utilities
{
	/// <summary>
	/// Specifies the type of AutoCAD Electrical object.
	/// </summary>
	[Serializable]
	public enum AeObject
	{
		/// <summary>
		/// The vanilla AutoCAD block.
		/// </summary>
		[XmlEnum]
		Block,
		/// <summary>
		/// The AutoCAD Electrical Component block.
		/// </summary>
		[XmlEnum] 
		Component,
		/// <summary>
		/// The AutoCAD Electrical Conduit.
		/// </summary>
		[XmlEnum] 
		Conduit,
		/// <summary>
		/// The AutoCAD Electrical Footprint block.
		/// </summary>
		[XmlEnum] 
		Footprint,
		/// <summary>
		/// The AutoCAD Electrical Nameplate block.
		/// </summary>
		[XmlEnum] 
		Nameplate,
		/// <summary>
		/// The AutoCAD Electrical Terminal block.
		/// </summary>
		[XmlEnum] 
		Terminal,
		/// <summary>
		/// The AutoCAD Electrical wire junction block.
		/// </summary>
		[XmlEnum] 
		WDDOT,
		/// <summary>
		/// The AutoCAD Electrical settings block.
		/// </summary>
		[XmlEnum] 
		WD_M,
		/// <summary>
		/// The AutoCAD Electrical wire diagram ladder block.
		/// </summary>
		[XmlEnum] 
		WD_MLR,
		/// <summary>
		/// The AutoCAD Electrical wire, a line on a wire layer.
		/// </summary>
		[XmlEnum] 
		Wire,
		/// <summary>
		/// The AutoCAD Electrical wireno block.
		/// </summary>
		[XmlEnum] 
		Wireno,
	}
}
