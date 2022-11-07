using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GpcWrapper
{
	public class VertexList
	{
		public VertexList()
		{
		}
		public VertexList(PointF[] p)
		{
			this.NofVertices = p.Length;
			this.Vertex = new Vertex[this.NofVertices];
			for (int i = 0; i < p.Length; i++)
			{
				this.Vertex[i] = new Vertex((double)p[i].X, (double)p[i].Y);
			}
		}
		public GraphicsPath ToGraphicsPath()
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLines(this.ToPoints());
			return graphicsPath;
		}
		public PointF[] ToPoints()
		{
			PointF[] array = new PointF[this.NofVertices];
			for (int i = 0; i < this.NofVertices; i++)
			{
				array[i] = new PointF((float)this.Vertex[i].X, (float)this.Vertex[i].Y);
			}
			return array;
		}
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
		public int NofVertices;
		public Vertex[] Vertex;
	}
}
