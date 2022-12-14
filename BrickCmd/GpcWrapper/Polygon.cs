using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GpcWrapper
{
    public class Polygon
    {
        public Polygon()
        {
        }
        public Polygon(GraphicsPath path)
        {
            NofContours = 0;
            byte[] pathTypes = path.PathTypes;
            PointF[] pathPoints = path.PathPoints;
            foreach (byte b in pathTypes)
            {
                if ((b & 128) != 0)
                {
                    NofContours++;
                }
            }
            ContourIsHole = new bool[NofContours];
            Contour = new VertexList[NofContours];
            for (int j = 0; j < NofContours; j++)
            {
                ContourIsHole[j] = (j == 0);
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
                    Contour[num++] = vertexList;
                    arrayList.Clear();
                }
            }
        }
        public static Polygon FromFile(string filename, bool readHoleFlags)
        {
            return GpcWrapper.ReadPolygon(filename, readHoleFlags);
        }
        public void AddContour(VertexList contour, bool contourIsHole)
        {
            bool[] array = new bool[NofContours + 1];
            VertexList[] array2 = new VertexList[NofContours + 1];
            for (int i = 0; i < NofContours; i++)
            {
                array[i] = ContourIsHole[i];
                array2[i] = Contour[i];
            }
            array[NofContours] = contourIsHole;
            array2[NofContours++] = contour;
            ContourIsHole = array;
            Contour = array2;
        }
        public GraphicsPath ToGraphicsPath()
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            for (int i = 0; i < NofContours; i++)
            {
                PointF[] array = Contour[i].ToPoints();
                if (ContourIsHole[i])
                {
                    Array.Reverse(array);
                }
                graphicsPath.AddPolygon(array);
            }
            return graphicsPath;
        }
        public override string ToString()
        {
            string text = "Polygon with " + NofContours.ToString() + " contours.\r\n";
            for (int i = 0; i < NofContours; i++)
            {
                if (ContourIsHole[i])
                {
                    text += "Hole: ";
                }
                else
                {
                    text += "Contour: ";
                }
                text += Contour[i].ToString();
            }
            return text;
        }
        public Tristrip ClipToTristrip(GpcOperation operation, Polygon polygon)
        {
            return GpcWrapper.ClipToTristrip(operation, this, polygon);
        }
        public Polygon Clip(GpcOperation operation, Polygon polygon)
        {
            return GpcWrapper.Clip(operation, this, polygon);
        }
        public Tristrip ToTristrip()
        {
            return GpcWrapper.PolygonToTristrip(this);
        }
        public void Save(string filename, bool writeHoleFlags)
        {
            GpcWrapper.SavePolygon(filename, writeHoleFlags, this);
        }
        public int NofContours;
        public bool[] ContourIsHole;
        public VertexList[] Contour;
    }
}
