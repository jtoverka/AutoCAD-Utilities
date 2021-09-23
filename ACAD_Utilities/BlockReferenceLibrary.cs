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
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace ACAD_Utilities
{
    /// <summary>
    /// Methods to process block references
    /// </summary>
    public static class BlockReferenceLibrary
    {
        /// <summary>
        /// Insert a block reference into into a <see cref="BlockTableRecord"/>. <see cref="Layout">Layouts</see> are also considered BlockTableRecords.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="blockSpaceId"></param>
        /// <param name="insertPoint"></param>
        /// <param name="blockname"></param>
        /// <param name="attributes"></param>
        /// <param name="xScale"></param>
        /// <param name="yScale"></param>
        /// <param name="zScale"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        /// <exception cref="Exception" />
        public static BlockReference InsertBlock(this Transaction transaction, ObjectId blockSpaceId, Point3d insertPoint, string blockname, Dictionary<string, Attrib> attributes = null, double xScale = 1, double yScale = 1, double zScale = 1, double rotate = 0)
        {
            Database database = blockSpaceId.Database;
            BlockReference blockReference = null;

            // Block Space can be a Layout like Model space, or Paper space as well as a block definition.
            using DBObject AcDbObject = transaction.GetObject(blockSpaceId, OpenMode.ForWrite);

            using BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

            // Check if block definition exist
            if (blockTable.Has(blockname))
            {
                // Get block definition
                ObjectId blockTableRecordId = blockTable[blockname];

                // Insert block
                blockReference = new BlockReference(insertPoint, blockTableRecordId)
                {
                    ScaleFactors = new Scale3d(xScale, yScale, zScale),
                    Rotation = rotate,
                };

                // Add block reference
                using BlockTableRecord blockTableRecord = AcDbObject as BlockTableRecord;
                blockTableRecord.AppendEntity(blockReference);
                transaction.AddNewlyCreatedDBObject(blockReference, true);

                // Write attribute data to block reference
                blockReference.SetAttributes(transaction, attributes);
            }
			else
			{
                blockReference?.Dispose();
                throw new Exception($"block '{blockname}' does not exist");
            }
            
            return blockReference;
        }
        /// <summary>
        /// Gets the block effective name
        /// </summary>
        /// <param name="blockReference"></param>
        /// <returns></returns>
        public static string GetEffectiveName(this BlockReference blockReference)
        {
            if (blockReference.IsDynamicBlock)
            {
                using BlockTableRecord blockTableRecord = blockReference.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
                return blockTableRecord.Name;
            }
            else
            {
                return blockReference.Name;
            }
        }
    }
}
