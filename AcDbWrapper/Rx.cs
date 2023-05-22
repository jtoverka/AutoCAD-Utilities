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
using Autodesk.AutoCAD.Runtime;

namespace AcDbWrapper
{
    /// <summary>
    /// Extension class for Rx* objects and classes
    /// </summary>
    public static class Rx
    {
        /// <summary>
        /// Get the RXClass of any DBObject type
        /// </summary>
        /// <typeparam name="T">Type to get the RXClass from.</typeparam>
        /// <returns>The RXClass of the type.</returns>
        /// <example>
        /// GetClass<AlignedDimension>();
        /// </example>
        public static RXClass GetClass<T>() where T : DBObject
        {
            return RXObject.GetClass(typeof(T));
        }

        /// <summary>
        /// Gets whether the ObjectId is derived from or is a certain RXClass.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="DBObject"/></typeparam>
        /// <param name="objectId">The ID of the object.</param>
        /// <returns><see langword="true"/> if the id is of type <typeparamref name="T"/>; otherwise, return <see langword="false"/>.</returns>
        public static bool IsMatch<T>(this ObjectId objectId) where T : DBObject
        {
            return objectId.IsMatch<T>(false);
        }

        /// <summary>
        /// Gets whether the ObjectId is derived from or is a certain RXClass.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="DBObject"/></typeparam>
        /// <param name="objectId">The ID of the object.</param>
        /// <param name="error">Throw an error if the validation test fails.</param>
        /// <returns><see langword="true"/> if the id is of type <typeparamref name="T"/>; otherwise, return <see langword="false"/>.</returns>
        public static bool IsMatch<T>(this ObjectId objectId, bool error) where T : DBObject
        {
            return IsMatch(objectId, error, GetClass<T>());
        }

        /// <summary>
        /// Gets whether the ObjectId is derived from or is a certain RXClass.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="DBObject"/></typeparam>
        /// <param name="objectId">The ID of the object.</param>
        /// <param name="objectClass">The class of the object.</param>
        /// <returns><see langword="true"/> if the id is of type <typeparamref name="T"/>; otherwise, return <see langword="false"/>.</returns>
        public static bool IsMatch(this ObjectId objectId, RXClass objectClass)
        {
            return objectId.Validate(false)
                && (objectId.ObjectClass == objectClass
                || objectId.ObjectClass.IsDerivedFrom(objectClass));
        }

        /// <summary>
        /// Gets whether the ObjectId is derived from or is a certain RXClass.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="DBObject"/></typeparam>
        /// <param name="objectId">The ID of the object.</param>
        /// <param name="error">Throw an error if the validation test fails.</param>
        /// <param name="objectClass">The class of the object.</param>
        /// <returns><see langword="true"/> if the id is of type <typeparamref name="T"/>; otherwise, return <see langword="false"/>.</returns>
        public static bool IsMatch(this ObjectId objectId, bool error, RXClass objectClass)
        {
            return objectId.Validate(error)
                && (objectId.ObjectClass == objectClass
                || objectId.ObjectClass.IsDerivedFrom(objectClass));
        }
    }
}
