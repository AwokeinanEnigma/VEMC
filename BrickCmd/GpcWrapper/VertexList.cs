using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GpcWrapper
{
	// Token: 0x02000023 RID: 35
	public class VertexList
	{
		// Token: 0x060001C8 RID: 456 RVA: 0x00007AEA File Offset: 0x00005CEA
		public VertexList()
		{
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x00007AF4 File Offset: 0x00005CF4
		public VertexList(PointF[] p)
		{
			this.NofVertices = p.Length;
			this.Vertex = new Vertex[this.NofVertices];
			for (int i = 0; i < p.Length; i++)
			{
				this.Vertex[i] = new Vertex((double)p[i].X, (double)p[i].Y);
			}
		}

		// Token: 0x060001CA RID: 458 RVA: 0x00007B60 File Offset: 0x00005D60
		public GraphicsPath ToGraphicsPath()
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLines(this.ToPoints());
			return graphicsPath;
		}

		// Token: 0x060001CB RID: 459 RVA: 0x00007B80 File Offset: 0x00005D80
		public PointF[] ToPoints()
		{
			PointF[] array = new PointF[this.NofVertices];
			for (int i = 0; i < this.NofVertices; i++)
			{
				array[i] = new PointF((float)this.Vertex[i].X, (float)this.Vertex[i].Y);
			}
			return array;
		}

		// Token: 0x060001CC RID: 460 RVA: 0x00007BE0 File Offset: 0x00005DE0
		public GraphicsPath TristripToGraphicsPath()
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			for (int i = 0; i < this.NofVertices - 2; i++)
			{
				graphicsPath.AddPolygon(new PointF[]
				{
					new PointF((float)this.Vertex[i].X, (float)this.Vertex[i].Y),
					new PointF((float)this.Vertex[i + 1].X, (float)this.Vertex[i + 1].Y),
					new PointF((float)this.Vertex[i + 2].X, (float)this.Vertex[i + 2].Y)
				});
			}
			return graphicsPath;
		}

		// Token: 0x060001CD RID: 461 RVA: 0x00007CC4 File Offset: 0x00005EC4
		public override string ToString()
		{
			string text = "Polygon with " + this.NofVertices + " vertices: ";
			for (int i = 0; i < this.NofVertices; i++)
			{
				text += this.Vertex[i].ToString();
				if (i != this.NofVertices - 1)
				{
					text += ",";
				}
			}
			return text;
		}

		// Token: 0x04000083 RID: 131
		public int NofVertices;

		// Token: 0x04000084 RID: 132
		public Vertex[] Vertex;
	}
}
