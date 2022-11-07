using ClipperLib;
using LibTessDotNet;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VEMC.Geometry
{
    internal class Polygon
    {
        public PolyTree PolyTree => polyTree;
        public Polygon(GraphicsPath path)
        {
            polyTree = new PolyTree();
            for (int i = 0; i < path.PointCount; i++)
            {
                PointF pointF = path.PathPoints[i];
                polyTree.Contour.Add(new IntPoint(pointF.X, pointF.Y));
            }
        }
        public Polygon(PolyTree polyTree)
        {
            this.polyTree = polyTree;
        }
        public Polygon Clip(ClipType type, Polygon polygon)
        {
            PolyTree polyTree = new PolyTree();
            Polygon.clipper.AddPath(this.polyTree.Contour, 0, true);
            Polygon.clipper.AddPath(polygon.polyTree.Contour, (PolyType)1, true);
            Polygon.clipper.Execute(type, polyTree);
            return new Polygon(polyTree);
        }
        public ContourVertex[] ToTristrip()
        {
            Tess tess = new Tess();
            ContourVertex[] array = new ContourVertex[polyTree.Contour.Count];
            ContourVertex contourVertex = default(ContourVertex);
            for (int i = 0; i < array.Length; i++)
            {
                Vec3 position = default(Vec3);
                position.X = polyTree.Contour[i].X;
                position.Y = polyTree.Contour[i].Y;
                contourVertex.Position = position;
                array[i] = contourVertex;
                contourVertex = default(ContourVertex);
            }
            tess.AddContour(array);
            tess.Tessellate((WindingRule)2, 0, 3);
            return tess.Vertices;
        }
        private static readonly Clipper clipper = new Clipper(0);
        private readonly PolyTree polyTree;
    }
}
