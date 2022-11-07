using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GpcWrapper
{
	// Token: 0x02000024 RID: 36
	public class Polygon
	{
		// Token: 0x060001CE RID: 462 RVA: 0x00007D33 File Offset: 0x00005F33
		public Polygon()
		{
		}

		// Token: 0x060001CF RID: 463 RVA: 0x00007D3C File Offset: 0x00005F3C
		public Polygon(GraphicsPath path)
		{
			this.NofContours = 0;
			byte[] pathTypes = path.PathTypes;
			PointF[] pathPoints = path.PathPoints;
			foreach (byte b in pathTypes)
			{
				if ((b & 128) != 0)
				{
					this.NofContours++;
				}
			}
			this.ContourIsHole = new bool[this.NofContours];
			this.Contour = new VertexList[this.NofContours];
			for (int j = 0; j < this.NofContours; j++)
			{
				this.ContourIsHole[j] = (j == 0);
			}
			int num = 0;
			ArrayList arrayList = new ArrayList();
			for (int k = 0; k < pathPoints.Length; k++)
			{
				arrayList.Add(pathPoints[k]);
				if ((path.PathTypes[k] & 128) != 0)
				{
					PointF[] p = (PointF[])arrayList.ToArray(typeof(PointF));
					VertexList vertexList = new VertexList(p);
					this.Contour[num++] = vertexList;
					arrayList.Clear();
				}
			}
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00007E55 File Offset: 0x00006055
		public static Polygon FromFile(string filename, bool readHoleFlags)
		{
			return GpcWrapper.ReadPolygon(filename, readHoleFlags);
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x00007E60 File Offset: 0x00006060
		public void AddContour(VertexList contour, bool contourIsHole)
		{
			bool[] array = new bool[this.NofContours + 1];
			VertexList[] array2 = new VertexList[this.NofContours + 1];
			for (int i = 0; i < this.NofContours; i++)
			{
				array[i] = this.ContourIsHole[i];
				array2[i] = this.Contour[i];
			}
			array[this.NofContours] = contourIsHole;
			array2[this.NofContours++] = contour;
			this.ContourIsHole = array;
			this.Contour = array2;
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x00007EDC File Offset: 0x000060DC
		public GraphicsPath ToGraphicsPath()
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			for (int i = 0; i < this.NofContours; i++)
			{
				PointF[] array = this.Contour[i].ToPoints();
				if (this.ContourIsHole[i])
				{
					Array.Reverse(array);
				}
				graphicsPath.AddPolygon(array);
			}
			return graphicsPath;
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00007F28 File Offset: 0x00006128
		public override string ToString()
		{
			string text = "Polygon with " + this.NofContours.ToString() + " contours.\r\n";
			for (int i = 0; i < this.NofContours; i++)
			{
				if (this.ContourIsHole[i])
				{
					text += "Hole: ";
				}
				else
				{
					text += "Contour: ";
				}
				text += this.Contour[i].ToString();
			}
			return text;
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x00007F9A File Offset: 0x0000619A
		public Tristrip ClipToTristrip(GpcOperation operation, Polygon polygon)
		{
			return GpcWrapper.ClipToTristrip(operation, this, polygon);
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x00007FA4 File Offset: 0x000061A4
		public Polygon Clip(GpcOperation operation, Polygon polygon)
		{
			return GpcWrapper.Clip(operation, this, polygon);
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x00007FAE File Offset: 0x000061AE
		public Tristrip ToTristrip()
		{
			return GpcWrapper.PolygonToTristrip(this);
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x00007FB6 File Offset: 0x000061B6
		public void Save(string filename, bool writeHoleFlags)
		{
			GpcWrapper.SavePolygon(filename, writeHoleFlags, this);
		}

		// Token: 0x04000085 RID: 133
		public int NofContours;

		// Token: 0x04000086 RID: 134
		public bool[] ContourIsHole;

		// Token: 0x04000087 RID: 135
		public VertexList[] Contour;
	}
}
