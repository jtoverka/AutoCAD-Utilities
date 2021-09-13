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

using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace ACAD_Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class GeometricLibrary
    {
        /// <summary>
        /// Explodes an entity, placing the sub-entities in it's owner's space, and deleting the original entity.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static ObjectIdCollection Explode(this Transaction transaction, ObjectId objectId)
        {
            // Collection of entities after exploding
            using DBObjectCollection objectCollection = new();

            // Entity to explode
            using Entity entity = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;

            using BlockTableRecord parentSpace = transaction.GetObject(entity.OwnerId, OpenMode.ForWrite) as BlockTableRecord;

            // Explode entity
            entity.Explode(objectCollection);

            // Erase entity after exploding
            entity.Erase();

            // Add sub-entities to owner space
            foreach (DBObject item in objectCollection)
            {
                parentSpace.AppendEntity(item as Entity);
                transaction.AddNewlyCreatedDBObject(item as Entity, true);
            }
            ObjectIdCollection collection = new();
            foreach (DBObject item in objectCollection)
            {
                collection.Add(item.Id);
            }

            return collection;
        }

        /// <summary>
        /// Creates a circle entity defined by three supplied points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <returns><see cref="Circle"/></returns>
        public static Circle GetCircle(Point3d point1, Point3d point2, Point3d point3)
        {
            //
            Point3d middle1_2 = point1.Add(point2.GetAsVector()).DivideBy(2);
            Point3d middle2_3 = point2.Add(point3.GetAsVector()).DivideBy(2);

            Vector3d vector1_2 = point2.Subtract(point1.GetAsVector()).GetAsVector();
            Vector3d vector2_3 = point3.Subtract(point2.GetAsVector()).GetAsVector();

            Point3d vPoint1 = middle1_2.Add(new Point3d(-vector1_2.Y, vector1_2.X, 0).GetAsVector());
            Point3d vPoint2 = middle2_3.Add(new Point3d(-vector2_3.Y, vector2_3.X, 0).GetAsVector());

            using Line3d line1 = new(middle1_2, vPoint1);
            using Line3d line2 = new(middle2_3, vPoint2);

            Point3d center = line1.IntersectWith(line2)[0];

            Vector3d normal = new(0, 0, 1);

            double radius = center.DistanceTo(point1);

            return new Circle(center, normal, radius);
        }

        /// <summary>
        /// Creates a polyline arc from three points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <returns></returns>
        public static PolylineCurve3d GetPolyline(Point3d point1, Point3d point2, Point3d point3)
        {
            Point3dCollection points = new(new Point3d[] { point1, point2, point3 });
            return new PolylineCurve3d(points);
        }

        /// <summary>
        /// Gets the bulge ratio from three points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <returns></returns>
        public static double GetBulge(Point3d point1, Point3d point2, Point3d point3)
        {
            double angle1 = Math.PI - point2.GetAsVector().GetAngleTo(point1.GetAsVector());
            double angle2 = point2.GetAsVector().GetAngleTo(point3.GetAsVector());

            double average = (angle1 + angle2) / 2;

            return (Math.Sin(average) / Math.Cos(average));
        }
        /// <summary>
        /// Compares two points within a distance tolerance
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Equals(this Point3d point1, Point3d point2, double distance)
        {
            return point1.DistanceTo(point2) < distance;
        }
        /// <summary>
        /// Compares two points within an x and y tolerance
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Equals(this Point3d point1, Point3d point2, double x, double y)
        {
            Vector3d vector = point1.GetVectorTo(point2);
            return vector.X < x && vector.Y < y;
        }

        /// <summary>
        /// Determines if a point is on or near the line with a specified tolerance.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="line">The two points that determines a line to check.</param>
        /// <param name="tolerance">The allowable distance between the point and the nearest point on the line.</param>
        /// <returns><c>true</c> if the point in on or near the line within a specified tolerance; otherwise <c>false</c>.</returns>
        public static bool LineMember(Point3d point, Tuple<Point3d, Point3d> line, double tolorance)
        {
            double x0 = point.X;
            double y0 = point.Y;
            double x1;
            double y1;
            double x2;
            double y2;
            if (line.Item1.X < line.Item2.X)
            {
                x1 = line.Item1.X;
                y1 = line.Item1.Y;
                x2 = line.Item2.X;
                y2 = line.Item2.Y;
            }
            else
            {
                x1 = line.Item2.X;
                y1 = line.Item2.Y;
                x2 = line.Item1.X;
                y2 = line.Item1.Y;
            }
            double distanceToEndpoint1;
            double distanceToEndpoint2;

            //Distance between point and line
            double a = y1 - y2;
            double b = x2 - x1;
            double c = (y2 * x1) - (x2 * y1);
            double numerator = Math.Abs((a * x0) + (b * y0) + c);
            double denominator = Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
            double distanceToLine = numerator / denominator;

            //Distance from endpoints
            distanceToEndpoint1 = Math.Sqrt(Math.Pow((x1 - x0), 2) + Math.Pow((y1 - y0), 2));
            distanceToEndpoint2 = Math.Sqrt(Math.Pow((x2 - x0), 2) + Math.Pow((y2 - y0), 2));

            return ((distanceToEndpoint1 <= tolorance) || (distanceToEndpoint2 <= tolorance) || (distanceToLine <= tolorance));
        }

        /// <summary>
        /// Determines if a point is on or near the line with a specified tolerance.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="line">The line to check.</param>
        /// <param name="tolerance">The allowable distance between the point and the nearest point on the line.</param>
        /// <returns><c>true</c> if the point in on or near the line within a specified tolerance; otherwise <c>false</c>.</returns>
        public static bool LineMember(Point3d point, Line line, double tolerance)
        {
            return LineMember(point, new Tuple<Point3d, Point3d>(line.StartPoint, line.EndPoint), tolerance);
        }

        /// <summary>
        /// Determines if two wires are networked based on a distance tolerance.
        /// </summary>
        /// <param name="wire1">The first wire to compare.</param>
        /// <param name="wire2">The second wire to compare.</param>
        /// <param name="tolerance">The allowable distance between the end of the wire and another wire.</param>
        /// <returns>byte flags indicating the matches made.</returns>
        public static byte WireNet(Tuple<Point3d, Point3d> wire1, Tuple<Point3d, Point3d> wire2, double tolerance)
        {
            byte result = 0;

            if (LineMember(wire1.Item1, wire2, tolerance)) { result += 1; }
            if (LineMember(wire1.Item2, wire2, tolerance)) { result += 2; }
            if (LineMember(wire2.Item1, wire1, tolerance)) { result += 4; }
            if (LineMember(wire2.Item2, wire1, tolerance)) { result += 8; }

            return result;
        }

        /// <summary>
        /// Determines if two wires are networked based on a distance tolerance.
        /// </summary>
        /// <param name="wire1">The first wire to compare.</param>
        /// <param name="wire2">The second wire to compare.</param>
        /// <param name="tolerance">The allowable distance between the end of the wire and another wire.</param>
        /// <returns>byte flags indicating the matches made.</returns>
        public static byte WireNet(Line wire1, Line wire2, double tolerance)
        {
            return WireNet(new Tuple<Point3d, Point3d>(wire1.StartPoint, wire1.EndPoint), new Tuple<Point3d, Point3d>(wire2.StartPoint, wire2.EndPoint), tolerance);
        }

        /// <summary>
        /// Creates a polyline with any number of points
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public static Polyline CreatePolyline(params Point3d[] pointList)
        {
            Plane xyPlane = new(Point3d.Origin, Vector3d.ZAxis);
            Polyline pline = new();
            pline.SetDatabaseDefaults();
            int index = 0;

            foreach (Point3d point in pointList)
            {
                pline.AddVertexAt(index, point.Convert2d(xyPlane), 0, 0, 0);
                index++;
            }

            return pline;
        }
    }
}