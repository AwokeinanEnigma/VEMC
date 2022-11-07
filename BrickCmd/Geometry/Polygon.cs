using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using ClipperLib;
using LibTessDotNet;

namespace VEMC.Geometry
{
	internal class Polygon
	{
		public PolyTree PolyTree
		{
			get
			{
				return this.polyTree;
			}
		}
		public Polygon(GraphicsPath path)
		{
			this.polyTree = new PolyTree();
			for (int i = 0; i < path.PointCount; i++)
			{
				PointF pointF = path.PathPoints[i];
				this.polyTree.Contour.Add(new IntPoint((double)pointF.X, (double)pointF.Y));
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
			ContourVertex[] array = new ContourVertex[this.polyTree.Contour.Count];
			ContourVertex contourVertex = default(ContourVertex);
			for (int i = 0; i < array.Length; i++)
			{
				Vec3 position = default(Vec3);
				position.X = (float)this.polyTree.Contour[i].X;
				position.Y = (float)this.polyTree.Contour[i].Y;
				contourVertex.Position = position;
				array[i] = contourVertex;
				contourVertex = default(ContourVertex);
			}
			tess.AddContour(array);
			tess.Tessellate((WindingRule)2, 0, 3);
			return tess.Vertices;
		}
		private static Clipper clipper = new Clipper(0);
		private PolyTree polyTree;
	}
}
