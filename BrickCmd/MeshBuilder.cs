// Decompiled with JetBrains decompiler

using ClipperLib;
using LibTessDotNet;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace VEMC
{
    internal class MeshBuilder
    {
        private readonly List<List<IntPoint>> solution;

        public List<List<IntPoint>> Solution => solution;

        public int PathCount => solution.Count;

        public MeshBuilder()
        {
            solution = new List<List<IntPoint>>();
        }

        public void AddPath(int x, int y, Point[] points)
        {
            // Create an empty list to store the points
            List<IntPoint> pointList = new List<IntPoint>();

            // Get the number of points in the input array
            int numPoints = points.Length;

            // Iterate through each point in the input array
            for (int i = 0; i < numPoints; i++)
            {
                // Create a new IntPoint at the specified offset from the input point
                IntPoint offsetPoint = new IntPoint(x + points[i].X, y + points[i].Y);

                // Add the point to the list
                pointList.Add(offsetPoint);
            }

            // Add the list of points to the path
            AddPath(pointList);
        }

        public void AddPath(IntPoint[] points)
        {
            List<IntPoint> path = new List<IntPoint>();
            path.AddRange(points);
            AddPath(path);
        }

        public void AddPath(List<IntPoint> path)
        {
            List<List<IntPoint>> intPointListList = new List<List<IntPoint>>
            {
                path
            };
            Clipper clipper = new Clipper(0);
            clipper.AddPaths(solution, 0, true);
            clipper.AddPaths(intPointListList, (PolyType)1, true);
            clipper.Execute((ClipType)1, solution);
        }

        public void Simplify()
        {
            Tess tess = new Tess();

            // Add all the contours to the tessellator
            foreach (var contour in solution)
            {
                var contourVertices = contour.Select(point => new ContourVertex
                {
                    Position = new Vec3
                    {
                        X = point.X,
                        Y = point.Y
                    }
                }).ToArray();
                tess.AddContour(contourVertices);
            }

            // Tessellate the contours
            tess.Tessellate(WindingRule.Positive, 0, 3);

            // Clear the original solution
            solution.Clear();

            // Iterate through all the tessellated elements and add them to the solution
            for (int i = 0; i < tess.ElementCount; i++)
            {
                Vec3 vertex1 = tess.Vertices[tess.Elements[i * 3]].Position;
                Vec3 vertex2 = tess.Vertices[tess.Elements[i * 3 + 1]].Position;
                Vec3 vertex3 = tess.Vertices[tess.Elements[i * 3 + 2]].Position;
                solution.Add(new List<IntPoint>
    {
        new IntPoint(vertex1.X, vertex1.Y),
        new IntPoint(vertex2.X, vertex2.Y),
        new IntPoint(vertex3.X, vertex3.Y)
    });
            }
        }


        public void DebugDraw(int w, int h)
        {
            if (solution.Count == 0)
            {
                return;
            }

            Bitmap bitmap = new Bitmap(w, h);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                for (int index = 0; index < w; index += 320)
                {
                    graphics.DrawLine(Pens.LightGray, index, 0, index, h);
                }

                for (int index = 0; index < h; index += 180)
                {
                    graphics.DrawLine(Pens.LightGray, 0, index, w, index);
                }

                using (List<List<IntPoint>>.Enumerator enumerator1 = solution.GetEnumerator())
                {
                    while (enumerator1.MoveNext())
                    {
                        List<IntPoint> current1 = enumerator1.Current;
                        IntPoint intPoint1 = current1[0];
                        IntPoint intPoint2 = intPoint1;
                        using (List<IntPoint>.Enumerator enumerator2 = current1.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                IntPoint current2 = enumerator2.Current;
                                if (intPoint1 != current2)
                                {
                                    Point pt2 = new Point((int)current2.X, (int)current2.Y);
                                    Point pt1 = new Point((int)intPoint1.X, (int)intPoint1.Y);
                                    graphics.DrawLine(Pens.Black, pt1, pt2);
                                    graphics.DrawRectangle(Pens.Red, pt2.X - 1, pt2.Y - 1, 2, 2);
                                    graphics.DrawRectangle(Pens.Red, pt1.X - 1, pt1.Y - 1, 2, 2);
                                    intPoint1 = current2;
                                }
                            }
                        }
                        Point pt2_1 = new Point((int)intPoint2.X, (int)intPoint2.Y);
                        Point pt1_1 = new Point((int)intPoint1.X, (int)intPoint1.Y);
                        graphics.DrawLine(Pens.Black, pt1_1, pt2_1);
                        graphics.DrawRectangle(Pens.Red, pt2_1.X - 1, pt2_1.Y - 1, 2, 2);
                        graphics.DrawRectangle(Pens.Red, pt1_1.X - 1, pt1_1.Y - 1, 2, 2);
                    }
                }
            }
            bitmap.Save("collision.png", ImageFormat.Png);
            bitmap.Dispose();
        }
    }
}
