using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using ClipperLib;
using LibTessDotNet;

namespace VEMC.Geometry
{
	// Token: 0x02000020 RID: 32
	internal class Polygon
	{
		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x000078F0 File Offset: 0x00005AF0
		public PolyTree PolyTree
		{
			get
			{
				return this.polyTree;
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x000078F8 File Offset: 0x00005AF8
		public Polygon(GraphicsPath path)
		{
			this.polyTree = new PolyTree();
			for (int i = 0; i < path.PointCount; i++)
			{
				PointF pointF = path.PathPoints[i];
				this.polyTree.Contour.Add(new IntPoint((double)pointF.X, (double)pointF.Y));
			}
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000795E File Offset: 0x00005B5E
		public Polygon(PolyTree polyTree)
		{
			this.polyTree = polyTree;
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00007970 File Offset: 0x00005B70
		public Polygon Clip(ClipType type, Polygon polygon)
		{
			PolyTree polyTree = new PolyTree();
			Polygon.clipper.AddPath(this.polyTree.Contour, 0, true);
			Polygon.clipper.AddPath(polygon.polyTree.Contour, (PolyType)1, true);
			Polygon.clipper.Execute(type, polyTree);
			return new Polygon(polyTree);
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x000079C8 File Offset: 0x00005BC8
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

		// Token: 0x0400007A RID: 122
		private static Clipper clipper = new Clipper(0);

		// Token: 0x0400007B RID: 123
		private PolyTree polyTree;
	}
}
