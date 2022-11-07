// Decompiled with JetBrains decompiler
// Type: VEMC.MeshBuilder
// Assembly: VEMC, Version=3.5.2.0, Culture=neutral, PublicKeyToken=null
// MVID: D31E32F7-EF00-4E54-8BC6-F1DFA3DF5B6D
// Assembly location: C:\Users\Tom\Documents\Mother4RestoredENIGMA_COPY\Mother4\bin\Debug\Brickroad.exe

using ClipperLib;
using LibTessDotNet;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace VEMC
{
    internal class MeshBuilder
    {
        private List<List<IntPoint>> solution;

        public List<List<IntPoint>> Solution
        {
            get
            {
                return this.solution;
            }
        }

        public int PathCount
        {
            get
            {
                return this.solution.Count;
            }
        }

        public MeshBuilder()
        {
            this.solution = new List<List<IntPoint>>();
        }

        public void AddPath(int x, int y, Point[] points)
        {
            List<IntPoint> list = new List<IntPoint>();
            int num = points.Length;
            for (int i = 0; i < num; i++)
            {
                IntPoint item;
                
                item = new IntPoint((long)(x + points[i].X), (long)(y + points[i].Y));
                list.Add(item);
            }
            this.AddPath(list);
        }

        public void AddPath(IntPoint[] points)
        {
            List<IntPoint> path = new List<IntPoint>();
            path.AddRange((IEnumerable<IntPoint>)points);
            this.AddPath(path);
        }

        public void AddPath(List<IntPoint> path)
        {
            List<List<IntPoint>> intPointListList = new List<List<IntPoint>>();
            intPointListList.Add(path);
            Clipper clipper = new Clipper(0);
            ((ClipperBase)clipper).AddPaths(this.solution, (PolyType)0, true);
            ((ClipperBase)clipper).AddPaths(intPointListList, (PolyType)1, true);
            clipper.Execute((ClipType)1, this.solution);
        }

        public void Simplify()
        {
            Tess tess = new Tess();
            int count = this.solution.Count;
            for (int i = 0; i < count; i++)
            {
                List<IntPoint> list = this.solution[i];
                int count2 = list.Count;
                ContourVertex[] array = new ContourVertex[count2];
                for (int j = 0; j < count2; j++)
                {
                    ContourVertex[] array2 = array;
                    int num = j;
                    ContourVertex contourVertex = default(ContourVertex);
                    Vec3 position = default(Vec3);
                    position.X = (float)list[j].X;
                    position.Y = (float)list[j].Y;
                    contourVertex.Position = position;
                    array2[num] = contourVertex;
                }
                tess.AddContour(array);
            }
            tess.Tessellate((WindingRule)2, 0, 3);
            this.solution.Clear();
            int elementCount = tess.ElementCount;
            for (int k = 0; k < elementCount; k++)
            {
                Vec3 position2 = tess.Vertices[tess.Elements[k * 3]].Position;
                Vec3 position3 = tess.Vertices[tess.Elements[k * 3 + 1]].Position;
                Vec3 position4 = tess.Vertices[tess.Elements[k * 3 + 2]].Position;
                List<IntPoint> list2 = new List<IntPoint>();
                list2.Add(new IntPoint((double)position2.X, (double)position2.Y));
                list2.Add(new IntPoint((double)position3.X, (double)position3.Y));
                list2.Add(new IntPoint((double)position4.X, (double)position4.Y));
                this.solution.Add(list2);
            }
        }


        public void DebugDraw(int w, int h)
        {
            if (this.solution.Count == 0)
                return;
            Bitmap bitmap = new Bitmap(w, h);
            using (Graphics graphics = Graphics.FromImage((Image)bitmap))
            {
                graphics.Clear(Color.White);
                for (int index = 0; index < w; index += 320)
                    graphics.DrawLine(Pens.LightGray, index, 0, index, h);
                for (int index = 0; index < h; index += 180)
                    graphics.DrawLine(Pens.LightGray, 0, index, w, index);
                using (List<List<IntPoint>>.Enumerator enumerator1 = this.solution.GetEnumerator())
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
                                //IntPoint.op_Inequality(intPoint1, current2)
                                //i think this is roughly intPoint 1 != current2
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
