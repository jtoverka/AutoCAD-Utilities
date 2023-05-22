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

namespace AcDbWrapper
{
    /// <summary>
    /// The data types stored in XData
    /// </summary>
    /// <remarks>
    /// <see href="https://help.autodesk.com/view/OARX/2022/ENU/?guid=GUID-3F0380A5-1C15-464D-BC66-2C5F094BCFB9"/>
    /// </remarks>
    public enum XDataType
    {
        /// <summary>
        /// ASCII string (up to 255 bytes long) in extended data
        /// </summary>
        ASCII = 1000,

        /// <summary>
        /// Registered application name (ASCII string up to 31 bytes long)
        /// </summary>
        RegApp = 1001,

        /// <summary>
        /// Extended data control string "{" or "}"
        /// </summary>
        Control = 1002,

        /// <summary>
        /// Extended data layer name
        /// </summary>
        Layer = 1003,

        /// <summary>
        /// Chunk of bytes (up to 127 bytes long) in extended data
        /// </summary>
        Bytes = 1004,

        /// <summary>
        /// Entity handle in extended data; text string of up to 16 hexadecimal digits
        /// </summary>
        Handle = 1005,

        /// <summary>
        /// A X coordinate in extended data
        /// </summary>
        PointX = 1010,

        /// <summary>
        /// A Y coordinate in extended data
        /// </summary>
        PointY = 1020,

        /// <summary>
        /// A Z coordinate in extended data
        /// </summary>
        PointZ = 1030,

        /// <summary>
        /// A 3D world space X position in extended data
        /// </summary>
        PointWspX = 1011,

        /// <summary>
        /// A 3D world space Y position in extended data
        /// </summary>
        PointWspY = 1021,

        /// <summary>
        /// A 3D world space Z position in extended data
        /// </summary>
        PointWspZ = 1031,

        /// <summary>
        /// A 3D world space X displacement in extended data
        /// </summary>
        PointWsdX = 1012,

        /// <summary>
        /// A 3D world space Y displacement in extended data
        /// </summary>
        PointWsdY = 1022,

        /// <summary>
        /// A 3D world space Z displacement in extended data
        /// </summary>
        PointWsdZ = 1032,

        /// <summary>
        /// A 3D world space X direction in extended data
        /// </summary>
        PointWsvX = 1013,

        /// <summary>
        /// A 3D world space Y direction in extended data
        /// </summary>
        PointWsvY = 1023,

        /// <summary>
        /// A 3D world space Z direction in extended data
        /// </summary>
        PointWsvZ = 1033,

        /// <summary>
        /// Extended data double-precision floating-point value
        /// </summary>
        Float = 1040,

        /// <summary>
        /// Extended data distance value
        /// </summary>
        Distance = 1041,

        /// <summary>
        /// Extended data distance value
        /// </summary>
        Scale = 1042,

        /// <summary>
        /// Extended data 16-bit signed integer
        /// </summary>
        Integer16 = 1070,

        /// <summary>
        /// Extended data 32-bit signed long
        /// </summary>
        Integer32 = 1071,
    }
}
