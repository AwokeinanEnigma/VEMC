using System;
using System.Collections.Generic;
using System.Xml.Linq;
using VEMC;

namespace TiledSharp
{
    public class TmxObjectGroup : ITmxElement
    {
        public string Name { get; private set; }
        public TmxColor Color { get; private set; }
        public double Opacity { get; private set; }
        public bool Visible { get; private set; }
        public TmxList<TmxObjectGroup.TmxObject> Objects { get; private set; }
        public PropertyDict Properties { get; private set; }
        public TmxObjectGroup(XElement xObjectGroup)
        {
            Name = (string)xObjectGroup.Attribute("name") ?? String.Empty;
            Color = new TmxColor(xObjectGroup.Attribute("color"));
            Opacity = (double?)xObjectGroup.Attribute("opacity") ?? 1.0;
            Visible = (bool?)xObjectGroup.Attribute("visible") ?? true;


            Objects = new TmxList<TmxObject>();
            foreach (var e in xObjectGroup.Elements("object"))
                Objects.Add(new TmxObject(e));

            Properties = new PropertyDict(xObjectGroup.Element("properties"));
        }
        public class TmxObject : ITmxElement
        {
            // Many TmxObjectTypes are distinguished by null values in fields
            // It might be smart to subclass TmxObject
            public int Id { get; private set; }
            public string Name { get; private set; }
            public TmxObjectType ObjectType { get; private set; }
            public string Type { get; private set; }
            public int X { get; private set; }
            public int Y { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Rotation { get; private set; }
            public TmxLayerTile Tile { get; private set; }
            public bool Visible { get; private set; }
            public List<Tuple<int, int>> Points { get; private set; }
            public PropertyDict Properties { get; private set; }
            public TmxObject(XElement xObject)
            {
                Id = (int?)xObject.Attribute("id") ?? 0;
                Name = (string)xObject.Attribute("name") ?? String.Empty;
                Debug.Log($"Object name: {Name}");
                X = ((int)(float)xObject.Attribute("x"));
                Y = ((int)(float)xObject.Attribute("y"));
                Width = (int?)(float)xObject.Attribute("width") ?? 0;
                Height = (int?)(float)xObject.Attribute("height") ?? 0;
                if (xObject.Attribute("class") != null)
                    Type = (string)xObject.Attribute("class") ?? String.Empty;
                else
                    Type = (string)xObject.Attribute("type") ?? String.Empty;
                Visible = (bool?)xObject.Attribute("visible") ?? true;
                Rotation = (int?)xObject.Attribute("rotation") ?? 0;

                var xGid = xObject.Attribute("gid");
                var xEllipse = xObject.Element("ellipse");
                var xPolygon = xObject.Element("polygon");
                var xPolyline = xObject.Element("polyline");

                if (xGid != null)
                {
                    Tile = new TmxLayerTile((uint)xGid, X, Y);
                    ObjectType = TmxObjectType.Tile;
                }
                else if (xEllipse != null)
                {
                    ObjectType = TmxObjectType.Ellipse;
                }
                else if (xPolygon != null)
                {
                    Points = ParsePoints(xPolygon);
                    ObjectType = TmxObjectType.Polygon;
                }
                else if (xPolyline != null)
                {
                    Points = ParsePoints(xPolyline);
                    ObjectType = TmxObjectType.Polyline;
                }
                else ObjectType = TmxObjectType.Basic;

                Properties = new PropertyDict(xObject.Element("properties"));
            }
            public List<Tuple<int, int>> ParsePoints(XElement xPoints)
            {
                List<Tuple<int, int>> list = new List<Tuple<int, int>>();
                string text = (string)xPoints.Attribute("points");
                string[] array = text.Split(new char[]
                {
                    ' '
                });
                foreach (string text2 in array)
                {
                    string[] array3 = text2.Split(new char[]
                    {
                        ','
                    });
                    float num = float.Parse(array3[0]);
                    float num2 = float.Parse(array3[1]);
                    int item = (int)Math.Round(num);
                    int item2 = (int)Math.Round(num2);
                    list.Add(Tuple.Create<int, int>(item, item2));
                }
                return list;
            }
        }
        public enum TmxObjectType : byte
        {
            Basic,
            Tile,
            Ellipse,
            Polygon,
            Polyline
        }
    }
}
