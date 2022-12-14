// Decompiled with JetBrains decompiler

using ClipperLib;
using LibTessDotNet;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

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
            List<IntPoint> list = new List<IntPoint>();
            int num = points.Length;
            for (int i = 0; i < num; i++)
            {
                IntPoint item;

                item = new IntPoint(x + points[i].X, y + points[i].Y);
                list.Add(item);
            }
            AddPath(list);
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
            int count = solution.Count;
            for (int i = 0; i < count; i++)
            {
                List<IntPoint> list = solution[i];
                int count2 = list.Count;
                ContourVertex[] array = new ContourVertex[count2];
                for (int j = 0; j < count2; j++)
                {
                    ContourVertex[] array2 = array;
                    int num = j;
                    ContourVertex contourVertex = default(ContourVertex);
                    Vec3 position = default(Vec3);
                    position.X = list[j].X;
                    position.Y = list[j].Y;
                    contourVertex.Position = position;
                    array2[num] = contourVertex;
                }
                tess.AddContour(array);
            }
            tess.Tessellate((WindingRule)2, 0, 3);
            solution.Clear();
            int elementCount = tess.ElementCount;
            for (int k = 0; k < elementCount; k++)
            {
                Vec3 position2 = tess.Vertices[tess.Elements[k * 3]].Position;
                Vec3 position3 = tess.Vertices[tess.Elements[k * 3 + 1]].Position;
                Vec3 position4 = tess.Vertices[tess.Elements[k * 3 + 2]].Position;
                List<IntPoint> list2 = new List<IntPoint>
                {
                    new IntPoint(position2.X, position2.Y),
                    new IntPoint(position3.X, position3.Y),
                    new IntPoint(position4.X, position4.Y)
                };
                solution.Add(list2);
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
